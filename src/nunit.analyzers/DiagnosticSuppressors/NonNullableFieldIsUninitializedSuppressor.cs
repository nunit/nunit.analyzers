#if !NETSTANDARD1_6

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.DiagnosticSuppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonNullableFieldIsUninitializedSuppressor : DiagnosticSuppressor
    {
        internal static readonly SuppressionDescriptor NullableFieldInitializedInSetUp = new SuppressionDescriptor(
            id: AnalyzerIdentifiers.NonNullableFieldIsUninitialized,
            suppressedDiagnosticId: "CS8618",
            justification: "Field is initialized in SetUp or OneTimeSetUp method");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(NullableFieldInitializedInSetUp);

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

                var classDeclaration = node.Ancestors().OfType<ClassDeclarationSyntax>().First();
                var fieldDeclarations = classDeclaration.Members.OfType<FieldDeclarationSyntax>().ToArray();

                string fieldName;

                if (node is VariableDeclaratorSyntax variableDeclarator)
                {
                    fieldName = variableDeclarator.Identifier.Text;
                }
                else if (node is ConstructorDeclarationSyntax)
                {
                    // Unfortunately the actual field name is not directly accessible.
                    // It seems to be in Argument[1], but that field is an internal property.
                    // Resort to reflection.
                    IReadOnlyList<object?>? arguments = (IReadOnlyList<object?>?)diagnostic.GetType()
                        .GetProperty("Arguments", BindingFlags.NonPublic | BindingFlags.Instance)?
                        .GetValue(diagnostic);
                    if (arguments != null && arguments.Count == 2 && arguments[1] is string possibleFieldName)
                    {
                        fieldName = possibleFieldName;
                    }
                    else
                    {
                        // Don't know how to extract field name.
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                // Verify that the name found is actually a field name.
                if (!fieldDeclarations.SelectMany(x => x.Declaration.Variables).Any(x => x.Identifier.Text == fieldName))
                {
                    continue;
                }

                var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToArray();

                foreach (var method in methods)
                {
                    var allAttributes = method.AttributeLists.SelectMany(list => list.Attributes.Select(a => a.Name.ToString()))
                                                             .ToImmutableHashSet();
                    if (allAttributes.Contains("SetUp") || allAttributes.Contains("OneTimeSetUp"))
                    {
                        // Find (OneTime)SetUps method and check for assignment to this field.
                        if (FieldIsAssignedIn(method, fieldName))
                        {
                            context.ReportSuppression(Suppression.Create(NullableFieldInitializedInSetUp, diagnostic));
                        }
                    }
                }
            }
        }

        private static bool FieldIsAssignedIn(MethodDeclarationSyntax method, string fieldName)
        {
            if (method.ExpressionBody != null)
            {
                return FieldIsAssignedIn(method.ExpressionBody.Expression, fieldName);
            }

            if (method.Body is null)
            {
                return false;
            }

            foreach (var statement in method.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax expressionStatement &&
                    FieldIsAssignedIn(expressionStatement.Expression, fieldName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool FieldIsAssignedIn(ExpressionSyntax? expressionStatement, string fieldName)
        {
            if (expressionStatement is AssignmentExpressionSyntax assignmentExpression)
            {
                if (assignmentExpression.Left.ToString() == fieldName)
                {
                    return true;
                }
                else if (assignmentExpression.Left is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is ThisExpressionSyntax &&
                    memberAccessExpression.Name.ToString() == fieldName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

#endif
