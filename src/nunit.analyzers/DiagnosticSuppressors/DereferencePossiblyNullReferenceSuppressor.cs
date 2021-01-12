#if !NETSTANDARD1_6

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                StatementSyntax? statement = node?.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
                BlockSyntax? parent = node?.Ancestors().OfType<BlockSyntax>().FirstOrDefault();

                if (node is null || parent is null)
                {
                    continue;
                }

                if (IsInsideAssertMultiple(parent))
                {
                    continue;
                }

                string possibleNullReference = node.ToString();
                if (node is CastExpressionSyntax castExpression)
                {
                    // Drop the cast.
                    possibleNullReference = castExpression.Expression.ToString();
                }

                if (IsAssertThrows(node))
                {
                    context.ReportSuppression(Suppression.Create(SuppressionDescriptors[diagnostic.Id], diagnostic));
                    continue;
                }

                var siblings = parent.ChildNodes().ToList();

                int nodeIndex = siblings.FindIndex(x => x == statement);

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
                                if (IsAssertThrows(assignmentExpression.Right))
                                {
                                    context.ReportSuppression(Suppression.Create(SuppressionDescriptors[diagnostic.Id], diagnostic));
                                }

                                // Stop searching for Assert before the assignment.
                                break;
                            }
                        }

                        // Check if this is Assert.NotNull or Assert.IsNotNull for the same symbol
                        if (expressionStatement.Expression is InvocationExpressionSyntax invocationExpression &&
                            invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                            memberAccessExpression.Expression is IdentifierNameSyntax identifierName &&
                            identifierName.Identifier.Text == "Assert")
                        {
                            var member = memberAccessExpression.Name.Identifier.Text;
                            if (member == "NotNull" || member == "IsNotNull" || member == "That")
                            {
                                if (member == "That")
                                {
                                    // We must check the 2nd argument for anything but "Is.Null"
                                    // E.g.: Is.Not.Null.And.Not.Empty.
                                    ArgumentSyntax? secondArgument = invocationExpression.ArgumentList.Arguments.ElementAtOrDefault(1);
                                    if (secondArgument?.ToString() == "Is.Null")
                                    {
                                        continue;
                                    }
                                }

                                ArgumentSyntax firstArgument = invocationExpression.ArgumentList.Arguments.First();
                                if (CoveredBy(firstArgument.Expression.ToString(), possibleNullReference))
                                {
                                    context.ReportSuppression(Suppression.Create(SuppressionDescriptors[diagnostic.Id], diagnostic));
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
                                if (IsAssertThrows(variable.Initializer?.Value))
                                {
                                    context.ReportSuppression(Suppression.Create(SuppressionDescriptors[diagnostic.Id], diagnostic));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsAssertThrows(SyntaxNode? node)
        {
            return (node is InvocationExpressionSyntax invocationExpression &&
                IsAssertThrows(invocationExpression)) ||
                (node is ArgumentSyntax argument && IsAssertThrows(argument.Expression));
        }

        private static bool IsAssertThrows(InvocationExpressionSyntax invocationExpression)
        {
            return invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                memberAccessExpression.Expression is IdentifierNameSyntax identifierName &&
                identifierName.Identifier.Text == "Assert" &&
                memberAccessExpression.Name.Identifier.Text == "Throws";
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

        private static bool IsInsideAssertMultiple(SyntaxNode parent)
        {
            var possibleAssertMultiple = parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (possibleAssertMultiple != null)
            {
                if (possibleAssertMultiple.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is IdentifierNameSyntax identifierName &&
                    identifierName.Identifier.Text == "Assert")
                {
                    if (memberAccessExpression.Name.Identifier.Text == "Multiple")
                    {
                        return true;
                    }
                }
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
