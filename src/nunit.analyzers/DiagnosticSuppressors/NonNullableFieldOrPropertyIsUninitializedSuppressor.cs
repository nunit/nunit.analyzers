#if !NETSTANDARD1_6

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;

using BindingFlags = System.Reflection.BindingFlags;

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
                SyntaxTree? sourceTree = diagnostic.Location.SourceTree;

                if (sourceTree is null)
                {
                    continue;
                }

                SyntaxNode node = sourceTree.GetRoot(context.CancellationToken)
                                            .FindNode(diagnostic.Location.SourceSpan);

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
                var classDeclaration = node.Ancestors().OfType<ClassDeclarationSyntax>().First();
                var fieldDeclarations = classDeclaration.Members.OfType<FieldDeclarationSyntax>();
                var propertyDeclarations = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();

                if (!fieldDeclarations.SelectMany(x => x.Declaration.Variables).Any(x => x.Identifier.Text == fieldOrPropertyName) &&
                    !propertyDeclarations.Any(x => x.Identifier.Text == fieldOrPropertyName))
                {
                    continue;
                }

                SemanticModel model = context.GetSemanticModel(sourceTree);

                var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
                foreach (var method in methods)
                {
                    var isSetup = method.AttributeLists.SelectMany(list => list.Attributes.Select(a => a.Name.ToString()))
                                                       .Any(name => name == "SetUp" || name == "OneTimeSetUp");
                    if (isSetup)
                    {
                        // Find (OneTime)SetUps method and check for assignment to this field.
                        if (IsAssignedIn(model, classDeclaration, method, fieldOrPropertyName))
                        {
                            context.ReportSuppression(Suppression.Create(NullableFieldOrPropertyInitializedInSetUp, diagnostic));
                        }
                    }
                }
            }
        }

        private static bool IsAssignedIn(
            SemanticModel model,
            ClassDeclarationSyntax classDeclaration,
            MethodDeclarationSyntax method,
            string fieldOrPropertyName)
        {
            if (method.ExpressionBody != null)
            {
                return IsAssignedIn(model, classDeclaration, method.ExpressionBody.Expression, fieldOrPropertyName);
            }

            if (method.Body is null)
            {
                return false;
            }

            foreach (var statement in method.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax expressionStatement &&
                    IsAssignedIn(model, classDeclaration, expressionStatement.Expression, fieldOrPropertyName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAssignedIn(
            SemanticModel model,
            ClassDeclarationSyntax classDeclaration,
            InvocationExpressionSyntax invocationExpression,
            string fieldOrPropertyName)
        {
            // Check semantic model for actual called method match found by compiler.
            IMethodSymbol? calledMethod = model.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

            // Find the corresponding declaration
            MethodDeclarationSyntax? method = calledMethod?.DeclaringSyntaxReferences.FirstOrDefault()?
                                                           .GetSyntax() as MethodDeclarationSyntax;
            if (method?.Parent == classDeclaration)
            {
                // We only get here if the method is in our source code and our class.
                return IsAssignedIn(model, classDeclaration, method, fieldOrPropertyName);
            }

            return false;
        }

        private static bool IsAssignedIn(
            SemanticModel model,
            ClassDeclarationSyntax classDeclaration,
            ExpressionSyntax? expressionStatement,
            string fieldOrPropertyName)
        {
            if (expressionStatement is AssignmentExpressionSyntax assignmentExpression)
            {
                if (assignmentExpression.Left.ToString() == fieldOrPropertyName)
                {
                    return true;
                }
                else if (assignmentExpression.Left is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is ThisExpressionSyntax &&
                    memberAccessExpression.Name.Identifier.Text == fieldOrPropertyName)
                {
                    return true;
                }
            }
            else if (expressionStatement is InvocationExpressionSyntax invocationExpression)
            {
                string? identifier = null;

                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                   memberAccessExpression.Expression is ThisExpressionSyntax)
                {
                    identifier = memberAccessExpression.Name.Identifier.Text;
                }
                else if (invocationExpression.Expression is IdentifierNameSyntax identifierName)
                {
                    identifier = identifierName.Identifier.Text;
                }

                if (!string.IsNullOrEmpty(identifier) &&
                    IsAssignedIn(model, classDeclaration, invocationExpression, fieldOrPropertyName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

#endif
