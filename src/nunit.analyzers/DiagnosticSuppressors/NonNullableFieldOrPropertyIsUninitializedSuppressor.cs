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
    public class NonNullableFieldOrPropertyIsUninitializedSuppressor : DiagnosticSuppressor
    {
        internal static readonly SuppressionDescriptor NullableFieldOrPropertyInitializedInSetUp = new SuppressionDescriptor(
            id: AnalyzerIdentifiers.NonNullableFieldOrPropertyIsUninitialized,
            suppressedDiagnosticId: "CS8618",
            justification: "Field/Property is initialized in SetUp or OneTimeSetUp method");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(NullableFieldOrPropertyInitializedInSetUp);

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
                var propertyDeclarations = classDeclaration.Members.OfType<PropertyDeclarationSyntax>().ToArray();

                string fieldOrPropertyName;

                if (node is VariableDeclaratorSyntax variableDeclarator)
                {
                    fieldOrPropertyName = variableDeclarator.Identifier.Text;
                }
                else if (node is PropertyDeclarationSyntax propertyDeclaration)
                {
                    fieldOrPropertyName = propertyDeclaration.Identifier.Text;
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
                        fieldOrPropertyName = possibleFieldName;
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

                // Verify that the name found is actually a field or a property name.
                if (!fieldDeclarations.SelectMany(x => x.Declaration.Variables).Any(x => x.Identifier.Text == fieldOrPropertyName) &&
                    !propertyDeclarations.Any(x => x.Identifier.Text == fieldOrPropertyName))
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
                        if (IsAssignedIn(method, fieldOrPropertyName))
                        {
                            context.ReportSuppression(Suppression.Create(NullableFieldOrPropertyInitializedInSetUp, diagnostic));
                        }
                    }
                }
            }
        }

        private static bool IsAssignedIn(MethodDeclarationSyntax method, string fieldOrPropertyName)
        {
            if (method.ExpressionBody != null)
            {
                return IsAssignedIn(method.ExpressionBody.Expression, fieldOrPropertyName);
            }

            if (method.Body is null)
            {
                return false;
            }

            foreach (var statement in method.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax expressionStatement &&
                    IsAssignedIn(expressionStatement.Expression, fieldOrPropertyName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAssignedIn(ExpressionSyntax? expressionStatement, string fieldOrPropertyName)
        {
            if (expressionStatement is AssignmentExpressionSyntax assignmentExpression)
            {
                if (assignmentExpression.Left.ToString() == fieldOrPropertyName)
                {
                    return true;
                }
                else if (assignmentExpression.Left is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is ThisExpressionSyntax &&
                    memberAccessExpression.Name.ToString() == fieldOrPropertyName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

#endif
