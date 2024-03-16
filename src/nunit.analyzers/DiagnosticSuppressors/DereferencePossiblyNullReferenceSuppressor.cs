using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.DiagnosticSuppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DereferencePossiblyNullReferenceSuppressor : DiagnosticSuppressor
    {
        private const string Justification = "Expression was checked in an ClassicAssert.NotNull, ClassicAssert.IsNotNull or Assert.That call";

        // Numbers from: https://cezarypiatek.github.io/post/non-nullable-references-in-dotnet-core/
        public static ImmutableDictionary<string, SuppressionDescriptor> SuppressionDescriptors { get; } =
            CreateSuppressionDescriptors(
                "CS8600", // Converting null literal or possible null value to non-nullable type.
                "CS8601", // Possible null reference assignment.
                "CS8602", // Dereference of a possibly null reference.
                "CS8603", // Possible null reference return.
                "CS8604", // Possible null reference argument.
                "CS8605", // Unboxing a possibly null value.
                "CS8606", // Possible null reference assignment to iteration variable.
                "CS8607", // A possible null value may not be passed to a target marked with the [DisallowNull] attribute.
                "CS8629", // Nullable value type may be null.
                "CS8634", // Nullability of type argument 'x' doesn't match 'class' constraint.
                "CS8714"); // Nullability of type argument 'x' doesn't match 'notnull' constraint.

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.CreateRange(SuppressionDescriptors.Values);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                SyntaxTree? root = diagnostic.Location.SourceTree;
                if (root is null)
                    continue;

                SyntaxNode? node = root.GetRoot(context.CancellationToken)
                                       .FindNode(diagnostic.Location.SourceSpan);

                if (node is null)
                {
                    continue;
                }

                bool shouldBeSuppressed;
                if (node is IdentifierNameSyntax && diagnostic.Id is "CS8634" or "CS8714" &&
                    node.Parent is InvocationExpressionSyntax invocationExpression)
                {
                    // The compiler complains about the method, not about the operand, we need to determine which one
                    // The Diagnostics arguments give both the method and the type parameter.
                    // Unfortunately the types (SourceOrdinaryMethodSymbol and SourceTypeParameterSymbol) are internal
                    // and don't implement the standard IMethodSymbol and ITypeParameterSymbol interfaces either.
                    // This forces us to get the semantic model here and find them again.
                    object[] arguments = diagnostic.Arguments();
                    if (arguments.Length != 3)
                        continue;

                    SemanticModel semanticModel = context.GetSemanticModel(root);
                    IMethodSymbol? method = semanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken)
                                                         .Symbol as IMethodSymbol;
                    if (method is null)
                        continue;

                    string typeName = arguments[1].ToString()!;
                    shouldBeSuppressed = ShouldBeSuppressed(method, typeName, invocationExpression);
                }
                else
                {
                    shouldBeSuppressed = ShouldBeSuppressed(node);
                }

                if (shouldBeSuppressed)
                {
                    context.ReportSuppression(Suppression.Create(SuppressionDescriptors[diagnostic.Id], diagnostic));
                }
            }
        }

        private static bool ShouldBeSuppressed(SyntaxNode node)
        {
            if (node is ArgumentSyntax argument)
            {
                // This is just a wrapper, remove it.
                node = argument.Expression;
            }

            string possibleNullReference = node.ToString();
            if (node is CastExpressionSyntax castExpression)
            {
                // Drop the cast.
                possibleNullReference = castExpression.Expression.ToString();
            }
            else if (node.IsKind(SyntaxKind.CoalesceExpression) && node is BinaryExpressionSyntax binaryExpression)
            {
                // The compiler complains about the whole expression instead of just the right operand.
                possibleNullReference = binaryExpression.Right.ToString();
            }
            else if (node is ConditionalExpressionSyntax conditionalExpression)
            {
                // The compiler complains about the whole expression instead of just one of the operands.
                // Try to determine which one.
                ExpressionSyntax? conditionalExpressionPath = DetermineConditionalExpressionPath(conditionalExpression);

                if (conditionalExpressionPath is null)
                {
                    // We don't know.
                    return false;
                }

                possibleNullReference = conditionalExpressionPath.ToString();
            }

            for (SyntaxNode? currentNode = node; currentNode is not null;)
            {
                StatementSyntax? statement = currentNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();

                if (statement is null)
                {
                    break;
                }

                BlockSyntax? block = statement.Parent as BlockSyntax;

                // If the statement is inside an Assert.Multiple NUnit doesn't throw
                // flow continues with actual null values and therefore these shouldn't be suppressed.
                if (!AssertHelper.IsInsideAssertMultiple(statement))
                {
                    if (IsKnownToBeNotNull(currentNode) ||
                        (block is not null && IsValidatedNotNullByPreviousStatementInSameBlock(possibleNullReference, block, statement)))
                    {
                        return true;
                    }
                }

                currentNode = statement.Parent;
            }

            return false;
        }

        private static bool ShouldBeSuppressed(IMethodSymbol method, string nonMatchedParameterType, InvocationExpressionSyntax invocationExpression)
        {
            var actualArguments = invocationExpression.ArgumentList.Arguments;

            IMethodSymbol originalMethod = method.OriginalDefinition;
            for (int i = 0; i < originalMethod.Parameters.Length; i++)
            {
                IParameterSymbol parameterSymbol = originalMethod.Parameters[i];
                if (parameterSymbol.IsParams)
                {
                    if (parameterSymbol.Type is IArrayTypeSymbol arrayType &&
                        arrayType.ElementType is ITypeParameterSymbol typeParameter &&
                        typeParameter.Name == nonMatchedParameterType)
                    {
                        // Verify all actual arguments match the constraint.
                        for (int j = i; j < actualArguments.Count; j++)
                        {
                            if (!ShouldBeSuppressed(actualArguments[j]))
                                return false;
                        }
                    }
                }
                else
                {
                    if (parameterSymbol.Type is ITypeParameterSymbol typeParameter &&
                        typeParameter.Name == nonMatchedParameterType)
                    {
                        if (!ShouldBeSuppressed(invocationExpression.ArgumentList.Arguments[i]))
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determine which side of the conditional expression if failing nullability.
        /// </summary>
        /// <remarks>
        /// We only try to detect simple cases where the operands being tested for <see langword="null"/> is one of the operands being returned:
        /// <code>variable is not null ? variable : otherExpression</code>
        /// </para>
        /// We recognize the 'is' pattern operations and normal equality.
        /// </remarks>
        /// <param name="conditionalExpression">Conditional expression to investigate.</param>
        /// <returns>Either the 'WhenTrue' or the 'WhenFalse' part of the <paramref name="conditionalExpression"/>.</returns>
        private static ExpressionSyntax? DetermineConditionalExpressionPath(ConditionalExpressionSyntax conditionalExpression)
        {
            string? testedAgainstNullExpression;
            bool isNot = false;

            if (conditionalExpression.Condition is IsPatternExpressionSyntax patternExpression &&
                patternExpression.Expression is IdentifierNameSyntax or MemberAccessExpressionSyntax)
            {
                // 'expression' is (not) null
                testedAgainstNullExpression = patternExpression.Expression.ToString();
                PatternSyntax pattern = patternExpression.Pattern;
                if (pattern is UnaryPatternSyntax unaryPattern && unaryPattern.IsKind(SyntaxKind.NotPattern))
                {
                    isNot = true;
                    pattern = unaryPattern.Pattern;
                }

                if (pattern is not ConstantPatternSyntax constantPattern || !constantPattern.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    // Comparison with anything but 'null' is not supported
                    return null;
                }
            }
            else if (conditionalExpression.Condition is BinaryExpressionSyntax binaryExpression &&
                (binaryExpression.IsKind(SyntaxKind.EqualsExpression) || binaryExpression.IsKind(SyntaxKind.NotEqualsExpression)))
            {
                isNot = binaryExpression.IsKind(SyntaxKind.NotEqualsExpression);

                if (binaryExpression.Right.IsKind(SyntaxKind.NullLiteralExpression) &&
                    binaryExpression.Left is IdentifierNameSyntax or MemberAccessExpressionSyntax)
                {
                    // 'expression' !=/== null
                    testedAgainstNullExpression = binaryExpression.Left.ToString();
                }
                else if (binaryExpression.Left.IsKind(SyntaxKind.NullLiteralExpression) &&
                    binaryExpression.Right is IdentifierNameSyntax or MemberAccessExpressionSyntax)
                {
                    // null !=/== 'expression'
                    testedAgainstNullExpression = binaryExpression.Right.ToString();
                }
                else
                {
                    // Not suppported comparison
                    return null;
                }
            }
            else
            {
                // Too complex an expression for us to handle.
                return null;
            }

            // Verify that the not null path is the identifier being tested.
            ExpressionSyntax notNullPath = isNot ? conditionalExpression.WhenTrue : conditionalExpression.WhenFalse;
            if (notNullPath.ToString().Equals(testedAgainstNullExpression, StringComparison.Ordinal))
            {
                return isNot ? conditionalExpression.WhenFalse : conditionalExpression.WhenTrue;
            }

            return null;
        }

        private static bool IsValidatedNotNullByPreviousStatementInSameBlock(string possibleNullReference, BlockSyntax block, StatementSyntax? statement)
        {
            var siblings = block.ChildNodes().ToList();
            int nodeIndex = statement is null ? siblings.Count : siblings.FindIndex(x => x == statement);

            // Look in earlier statements to see if the variable was previously checked for null.
            while (--nodeIndex >= 0)
            {
                SyntaxNode previous = siblings[nodeIndex];

                if (previous is ExpressionStatementSyntax expressionStatement)
                {
                    if (expressionStatement.Expression is AssignmentExpressionSyntax assignmentExpression)
                    {
                        // Is the offending symbol assigned here?
                        if (InvalidatedBy(assignmentExpression.Left.ToString(), possibleNullReference))
                        {
                            return IsKnownToBeNotNull(assignmentExpression.Right) ||
                                ShouldBeSuppressed(assignmentExpression.Right);
                        }
                    }
                    else
                    {
                        if (IsValidatedNotNullByExpression(possibleNullReference, expressionStatement.Expression))
                            return true;
                    }
                }
                else if (previous is LocalDeclarationStatementSyntax localDeclarationStatement)
                {
                    VariableDeclarationSyntax declaration = localDeclarationStatement.Declaration;
                    foreach (var variable in declaration.Variables)
                    {
                        if (variable.Identifier.ToString() == possibleNullReference)
                        {
                            ExpressionSyntax? initializer = variable.Initializer?.Value;
                            return initializer is not null &&
                                (IsKnownToBeNotNull(initializer) ||
                                ShouldBeSuppressed(initializer));
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsValidatedNotNullByExpression(string possibleNullReference, ExpressionSyntax expression)
        {
            // Check if this is an Assert for the same symbol
            if (AssertHelper.IsAssertClassicAssertOrAssume(expression, out string member, out ArgumentListSyntax? argumentList))
            {
                string firstArgument = argumentList.Arguments.First().Expression.ToString();

                if (member == NUnitFrameworkConstants.NameOfAssertThat)
                {
                    string? secondArgument =
                        argumentList.Arguments.ElementAtOrDefault(1)?.Expression.ToString();

                    // If test is on <nullable>.HasValue
                    if (IsHasValue(firstArgument, possibleNullReference))
                    {
                        // Could be:
                        // Assert.That(<nullable>.HasValue)
                        // Assert.That(<nullable>.HasValue, "Ensure Value Set")
                        // Assert.That(<nullable>.HasValue, Is.True)
                        if (secondArgument != "Is.False")
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // Null check, could be Is.Not.Null or more complex
                        // like Is.Not.Null.And.Not.Empty.
                        if (secondArgument != "Is.Null")
                        {
                            if (CoveredBy(firstArgument, possibleNullReference))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (member == NUnitFrameworkConstants.NameOfAssertNotNull ||
                    member == NUnitFrameworkConstants.NameOfAssertIsNotNull)
                {
                    if (CoveredBy(firstArgument, possibleNullReference))
                    {
                        return true;
                    }
                }
                else if (member == NUnitFrameworkConstants.NameOfAssertIsTrue ||
                         member == NUnitFrameworkConstants.NameOfAssertTrue)
                {
                    if (IsHasValue(firstArgument, possibleNullReference))
                    {
                        return true;
                    }
                }
                else if (member is NUnitFrameworkConstants.NameOfMultiple or NUnitFrameworkConstants.NameOfMultipleAsync)
                {
                    // Look up into the actual asserted parameter
                    if (argumentList.Arguments.FirstOrDefault()?.Expression is AnonymousFunctionExpressionSyntax anonymousFunction)
                    {
                        if (anonymousFunction.Block is not null)
                        {
                            if (IsValidatedNotNullByPreviousStatementInSameBlock(possibleNullReference, anonymousFunction.Block, null))
                                return true;
                        }
                        else if (anonymousFunction.ExpressionBody is not null)
                        {
                            if (IsValidatedNotNullByExpression(possibleNullReference, anonymousFunction.ExpressionBody))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsKnownToBeNotNull(SyntaxNode? node)
        {
            return (node is ExpressionSyntax expression && IsKnownToBeNotNull(expression)) ||
                (node is ArgumentSyntax argument && IsKnownToBeNotNull(argument.Expression));
        }

        private static bool IsKnownToBeNotNull(ExpressionSyntax? expression)
        {
            // For now, we only know that Assert.Throws either returns not-null or throws
            return AssertHelper.IsAssert(expression,
                NUnitFrameworkConstants.NameOfAssertThrows,
                NUnitFrameworkConstants.NameOfAssertCatch,
                NUnitFrameworkConstants.NameOfAssertThrowsAsync,
                NUnitFrameworkConstants.NameOfAssertCatchAsync);
        }

        private static bool InvalidatedBy(string assignment, string possibleNullReference)
        {
            if (assignment == possibleNullReference)
            {
                return true;
            }

            // a.B.C is invalidated when either a or a.B are assigned to.
            // But ab is not invalidated when a is assigned to
            return possibleNullReference.StartsWith(assignment, StringComparison.Ordinal) &&
                possibleNullReference[assignment.Length] == '.';
        }

        private static bool CoveredBy(string assertedNotNull, string possibleNullReference)
        {
            int exclamation = assertedNotNull.IndexOf('!');
            if (exclamation >= 0)
            {
                assertedNotNull = assertedNotNull.Replace("!", string.Empty);
            }

            if (possibleNullReference == assertedNotNull)
            {
                return true;
            }

            // If assertedNotNull is a?.B this covers both a.B and a.
            int question = assertedNotNull.IndexOf('?');
            if (question >= 0)
            {
                do
                {
                    string prefix = assertedNotNull.Substring(0, question)
                                                   .Replace("?", string.Empty);

                    if (possibleNullReference == prefix)
                    {
                        return true;
                    }

                    question = assertedNotNull.IndexOf('?', question + 1);
                }
                while (question > 0);

                return possibleNullReference == assertedNotNull.Replace("?", string.Empty);
            }

            return false;
        }

        private static bool IsHasValue(string argument, string possibleNullReference)
        {
            return argument == possibleNullReference + ".HasValue";
        }

        private static ImmutableDictionary<string, SuppressionDescriptor> CreateSuppressionDescriptors(params string[] suppressionDiagnosticsIds)
        {
            var builder = new Dictionary<string, SuppressionDescriptor>();
            foreach (var suppressionDiagnosticsId in suppressionDiagnosticsIds)
            {
                builder.Add(suppressionDiagnosticsId, CreateSuppressionDescriptor(suppressionDiagnosticsId));
            }

            return builder.ToImmutableDictionary();
        }

        private static SuppressionDescriptor CreateSuppressionDescriptor(string suppressedDiagnoticsId)
        {
            return new SuppressionDescriptor(
                id: AnalyzerIdentifiers.DereferencePossibleNullReference,
                suppressedDiagnosticId: suppressedDiagnoticsId,
                justification: Justification);
        }
    }
}
