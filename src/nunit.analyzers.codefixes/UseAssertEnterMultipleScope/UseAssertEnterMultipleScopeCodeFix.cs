using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseAssertEnterMultipleScope
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class UseAssertEnterMultipleScopeCodeFix : CodeFixProvider
    {
        internal const string UseAssertEnterMultipleScopeMethod = "Use Assert.EnterMultipleScope method";

        internal const string SystemThreadingTasksNamespace = "System.Threading.Tasks";
        internal const string TaskTypeName = "Task";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.UseAssertEnterMultipleScope);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var node = root.FindNode(context.Span);

            var expressionStatementSyntax = node.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            if (expressionStatementSyntax is null)
            {
                return;
            }

            var (blockSyntax, lambdaHasAsyncKeyword) = FindBlockSyntax(node);
            if (blockSyntax is null)
            {
                return;
            }

            var newBodySyntax = CreateUsingStatementSyntax(blockSyntax, expressionStatementSyntax.GetTrailingTrivia());

            // Replace body syntax and add annotation to have a reference to the new body syntax in the updated root
            var annotation = new SyntaxAnnotation();
            var annotatedNewSyntax = newBodySyntax.WithAdditionalAnnotations(annotation);
            var rootWithAnnotatedBody = root.ReplaceNode(expressionStatementSyntax, annotatedNewSyntax);
            var newBodySyntaxInUpdatedRoot = rootWithAnnotatedBody.GetAnnotatedNodes(annotation).First();

            var newRoot = UpdateTestMethodSignatureIfNecessary(rootWithAnnotatedBody, newBodySyntaxInUpdatedRoot, lambdaHasAsyncKeyword);

            var codeAction = CodeAction.Create(
                UseAssertEnterMultipleScopeMethod,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UseAssertEnterMultipleScopeMethod);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static (BlockSyntax? Block, bool HasAsyncKeyword) FindBlockSyntax(SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax invocationExprSyntax ||
                invocationExprSyntax.ArgumentList.Arguments.Count != 1 ||
                invocationExprSyntax.ArgumentList.Arguments[0].Expression
                    is not ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax)
            {
                return (null, false);
            }

            return (parenthesizedLambdaExpressionSyntax.Block, parenthesizedLambdaExpressionSyntax.AsyncKeyword != default);
        }

        private static UsingStatementSyntax CreateUsingStatementSyntax(BlockSyntax blockSyntax, SyntaxTriviaList trailingTrivia) =>
            SyntaxFactory.UsingStatement(blockSyntax)
                .WithExpression(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssert),
                            SyntaxFactory.IdentifierName(NUnitV4FrameworkConstants.NameOfEnterMultipleScope))))
                .WithTrailingTrivia(trailingTrivia);

        private static SyntaxNode UpdateTestMethodSignatureIfNecessary(
            SyntaxNode newRoot,
            SyntaxNode newSyntaxInTree,
            bool lambdaHasAsyncKeyword)
        {
            var methodDeclaration = newSyntaxInTree.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (!lambdaHasAsyncKeyword || methodDeclaration is null || IsAsyncTaskMethod(methodDeclaration))
            {
                return newRoot;
            }

            var namespaceDeclaration = methodDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
            var compilationUnit = methodDeclaration.FirstAncestorOrSelf<CompilationUnitSyntax>();
            var systemThreadingTasksUsingExists =
                compilationUnit?.Usings.Any(IsUsingSystemThreadingTasks) is true ||
                namespaceDeclaration?.Usings.Any(IsUsingSystemThreadingTasks) is true;

            var taskTypeName = GetTaskTypeSyntax(systemThreadingTasksUsingExists);

            var newMethodDeclaration = methodDeclaration
                .WithModifiers(methodDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)))
                .WithReturnType(taskTypeName);

            return newRoot.ReplaceNode(methodDeclaration, newMethodDeclaration);
        }

        private static bool IsUsingSystemThreadingTasks(UsingDirectiveSyntax u) =>
            u.Name.ToString() == SystemThreadingTasksNamespace;

        private static bool IsAsyncTaskMethod(MethodDeclarationSyntax methodDeclaration)
        {
            var isAsync = methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword);
            var returnsTask = (methodDeclaration.ReturnType is IdentifierNameSyntax id && id.Identifier.Text == TaskTypeName)
                || (methodDeclaration.ReturnType is QualifiedNameSyntax qn && qn.ToString() == $"{SystemThreadingTasksNamespace}.{TaskTypeName}");

            return isAsync && returnsTask;
        }

        private static TypeSyntax GetTaskTypeSyntax(bool systemThreadingTasksUsingExists)
            => systemThreadingTasksUsingExists
                ? SyntaxFactory.ParseTypeName(TaskTypeName)
                : QualifiedNameFromParts("System", "Threading", "Tasks", TaskTypeName);

        private static NameSyntax QualifiedNameFromParts(params string[] parts)
        {
            NameSyntax name = SyntaxFactory.IdentifierName(parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                name = SyntaxFactory.QualifiedName(name, SyntaxFactory.IdentifierName(parts[i]));
            }

            return name;
        }
    }
}
