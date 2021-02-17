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
                if (ShouldBeSuppressed(node, out SyntaxNode? suppressionCause) && !IsInsideAssertMultiple(suppressionCause))
                {
                    context.ReportSuppression(Suppression.Create(SuppressionDescriptors[diagnostic.Id], diagnostic));
                }
            }
        }

        private static bool IsInsideAssertMultiple(SyntaxNode node)
        {
            InvocationExpressionSyntax? possibleAssertMultiple;

            while ((possibleAssertMultiple = node.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault()) != null)
            {
                // Is the statement inside a Block which is part of an Assert.Multiple.
                if (IsAssert(possibleAssertMultiple, "Multiple"))
                {
                    return true;
                }

                // Keep looking at possible parent nested expression.
                node = possibleAssertMultiple;
            }

            return false;
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

                    // Check if this is Assert.NotNull or Assert.IsNotNull for the same symbol
                    if (IsAssert(expressionStatement.Expression, out string member, out ArgumentListSyntax? argumentList))
                    {
                        if (member == NunitFrameworkConstants.NameOfAssertNotNull ||
                            member == NunitFrameworkConstants.NameOfAssertIsNotNull ||
                            member == NunitFrameworkConstants.NameOfAssertThat)
                        {
                            if (member == NunitFrameworkConstants.NameOfAssertThat)
                            {
                                // We must check the 2nd argument for anything but "Is.Null"
                                // E.g.: Is.Not.Null.And.Not.Empty.
                                ArgumentSyntax? secondArgument = argumentList.Arguments.ElementAtOrDefault(1);
                                if (secondArgument?.ToString() == "Is.Null")
                                {
                                    continue;
                                }
                            }

                            ArgumentSyntax firstArgument = argumentList.Arguments.First();
                            if (CoveredBy(firstArgument.Expression.ToString(), possibleNullReference))
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
            return IsAssert(expression,
                NunitFrameworkConstants.NameOfAssertThrows,
                NunitFrameworkConstants.NameOfAssertCatch,
                NunitFrameworkConstants.NameOfAssertThrowsAsync,
                NunitFrameworkConstants.NameOfAssertCatchAsync);
        }

        private static bool IsAssert(ExpressionSyntax? expression, params string[] requestedMembers)
        {
            return IsAssert(expression, out string member, out _) && requestedMembers.Contains(member);
        }

        private static bool IsAssert(ExpressionSyntax? expression,
                                     out string member,
                                     [NotNullWhen(true)] out ArgumentListSyntax? argumentList)
        {
            if (expression is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                memberAccessExpression.Expression is IdentifierNameSyntax identifierName &&
                identifierName.Identifier.Text == NunitFrameworkConstants.NameOfAssert)
            {
                member = memberAccessExpression.Name.Identifier.Text;
                argumentList = invocationExpression.ArgumentList;
                return true;
            }
            else
            {
                member = string.Empty;
                argumentList = null;
                return false;
            }
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
