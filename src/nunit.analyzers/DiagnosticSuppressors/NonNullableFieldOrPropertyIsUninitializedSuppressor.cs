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
    public sealed class NonNullableFieldOrPropertyIsUninitializedSuppressor : DiagnosticSuppressor
    {
        internal static readonly SuppressionDescriptor NullableFieldOrPropertyInitializedInSetUp = new(
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
                    if (arguments is not null && arguments.Count == 2 && arguments[1] is string possibleFieldName)
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
                    var declaredMethod = model.GetDeclaredSymbol(method) as IMethodSymbol;
                    if (declaredMethod is null)
                    {
                        continue;
                    }

                    // Look for attribute not only on the method itself but
                    // also on the base method in case this is an override.
                    IEnumerable<AttributeData> attributes = Enumerable.Empty<AttributeData>();
                    do
                    {
                        attributes = attributes.Concat(declaredMethod.GetAttributes());
                        declaredMethod = declaredMethod.OverriddenMethod;
                    }
                    while (declaredMethod is not null);

                    var isSetup = attributes.Any(x => x.AttributeClass?.Name is "SetUpAttribute" or "OneTimeSetUpAttribute");

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
            if (method.ExpressionBody is not null)
            {
                return IsAssignedIn(model, classDeclaration, method.ExpressionBody.Expression, fieldOrPropertyName);
            }

            if (method.Body is not null)
            {
                return IsAssignedIn(model, classDeclaration, method.Body, fieldOrPropertyName);
            }

            return false;
        }

        private static bool IsAssignedIn(
            SemanticModel model,
            ClassDeclarationSyntax classDeclaration,
            StatementSyntax statement,
            string fieldOrPropertyName)
        {
            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    return IsAssignedIn(model, classDeclaration, expressionStatement.Expression, fieldOrPropertyName);

                case BlockSyntax block:
                    return IsAssignedIn(model, classDeclaration, block.Statements, fieldOrPropertyName);

                case TryStatementSyntax tryStatement:
                    return IsAssignedIn(model, classDeclaration, tryStatement.Block, fieldOrPropertyName) ||
                        (tryStatement.Finally is not null &&
                        IsAssignedIn(model, classDeclaration, tryStatement.Finally.Block, fieldOrPropertyName));

                default:
                    // Any conditional statement does not guarantee assignment.
                    return false;
            }
        }

        private static bool IsAssignedIn(
            SemanticModel model,
            ClassDeclarationSyntax classDeclaration,
            SyntaxList<StatementSyntax> statements,
            string fieldOrPropertyName)
        {
            foreach (var statement in statements)
            {
                if (IsAssignedIn(model, classDeclaration, statement, fieldOrPropertyName))
                    return true;
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
            if (expressionStatement is AwaitExpressionSyntax awaitExpression)
            {
                expressionStatement = awaitExpression.Expression;
                if (expressionStatement is InvocationExpressionSyntax awaitInvocationExpression &&
                    awaitInvocationExpression.Expression is MemberAccessExpressionSyntax awaitMemberAccessExpression &&
                    awaitMemberAccessExpression.Name.Identifier.Text == "ConfigureAwait")
                {
                    expressionStatement = awaitMemberAccessExpression.Expression;
                }
            }

            if (expressionStatement is AssignmentExpressionSyntax assignmentExpression)
            {
                if (assignmentExpression.Left is TupleExpressionSyntax tupleExpression)
                {
                    foreach (var argument in tupleExpression.Arguments)
                    {
                        if (GetIdentifier(argument.Expression) == fieldOrPropertyName)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (GetIdentifier(assignmentExpression.Left) == fieldOrPropertyName)
                    {
                        return true;
                    }
                }
            }
            else if (expressionStatement is InvocationExpressionSyntax invocationExpression)
            {
                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is InvocationExpressionSyntax awaitedInvocationExpression &&
                    memberAccessExpression.Name.Identifier.Text == "Wait")
                {
                    invocationExpression = awaitedInvocationExpression;
                }

                string? identifier = GetIdentifier(invocationExpression.Expression);

                if (!string.IsNullOrEmpty(identifier) &&
                    IsAssignedIn(model, classDeclaration, invocationExpression, fieldOrPropertyName))
                {
                    return true;
                }
            }

            return false;
        }

        private static string? GetIdentifier(ExpressionSyntax expression)
        {
            if (expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }
            else if (expression is MemberAccessExpressionSyntax memberAccessExpression &&
                memberAccessExpression.Expression is ThisExpressionSyntax)
            {
                return memberAccessExpression.Name.Identifier.Text;
            }

            return null;
        }
    }
}

#endif
