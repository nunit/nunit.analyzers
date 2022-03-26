#if !NETSTANDARD1_6

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.DiagnosticSuppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DereferencePossiblyNullReferenceSuppressor : DiagnosticSuppressor
    {
        private const string Justification = "Expression was checked in an Assert.NotNull, Assert.IsNotNull or Assert.That call";

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
                "CS8629"); // Nullable value type may be null.

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.CreateRange(SuppressionDescriptors.Values);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                SyntaxNode? node = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken)
                                                                  .FindNode(diagnostic.Location.SourceSpan);

                if (node is null)
                {
                    continue;
                }

                // Was the offending variable assigned or verified to be not null inside an Assert.Multiple?
                // NUnit doesn't throw on failures and therefore the compiler is correct.
                if (ShouldBeSuppressed(node, out SyntaxNode? suppressionCause) && !AssertHelper.IsInsideAssertMultiple(suppressionCause))
                {
                    context.ReportSuppression(Suppression.Create(SuppressionDescriptors[diagnostic.Id], diagnostic));
                }
            }
        }

        private static bool ShouldBeSuppressed(SyntaxNode node, [NotNullWhen(true)] out SyntaxNode? suppressionCause)
        {
            suppressionCause = default(SyntaxNode);

            if (IsKnownToBeNotNull(node))
            {
                // Known to be not null value assigned or passed to non-nullable type.
                suppressionCause = node;
                return true;
            }

            string possibleNullReference = node.ToString();
            if (node is CastExpressionSyntax castExpression)
            {
                // Drop the cast.
                possibleNullReference = castExpression.Expression.ToString();
            }

            bool validatedNotNull;

            do
            {
                BlockSyntax? parent = node.Ancestors().OfType<BlockSyntax>().FirstOrDefault();

                if (parent is null)
                {
                    return false;
                }

                validatedNotNull = IsValidatedNotNull(possibleNullReference, parent, node, out suppressionCause);
                if (parent.Parent is null)
                {
                    return false;
                }

                node = parent.Parent;
            }
            while (!validatedNotNull);

            return validatedNotNull;
        }

        private static bool IsValidatedNotNull(string possibleNullReference, BlockSyntax parent, SyntaxNode node,
                                               [NotNullWhen(true)] out SyntaxNode? suppressionCause)
        {
            suppressionCause = default(SyntaxNode);

            StatementSyntax? statement = node?.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            var siblings = parent.ChildNodes().ToList();

            // Look in earlier statements to see if the variable was previously checked for null.
            for (int nodeIndex = siblings.FindIndex(x => x == statement); --nodeIndex >= 0;)
            {
                SyntaxNode previous = siblings[nodeIndex];

                suppressionCause = previous;
                if (previous is ExpressionStatementSyntax expressionStatement)
                {
                    if (expressionStatement.Expression is AssignmentExpressionSyntax assignmentExpression)
                    {
                        // Is the offending symbol assigned here?
                        if (InvalidatedBy(assignmentExpression.Left.ToString(), possibleNullReference))
                        {
                            return IsKnownToBeNotNull(assignmentExpression.Right);
                        }
                    }

                    // Check if this is an Assert for the same symbol
                    if (AssertHelper.IsAssert(expressionStatement.Expression, out string member, out ArgumentListSyntax? argumentList))
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
                    }
                }
                else if (previous is LocalDeclarationStatementSyntax localDeclarationStatement)
                {
                    VariableDeclarationSyntax declaration = localDeclarationStatement.Declaration;
                    foreach (var variable in declaration.Variables)
                    {
                        if (variable.Identifier.ToString() == possibleNullReference)
                        {
                            return IsKnownToBeNotNull(variable.Initializer?.Value);
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

#endif
