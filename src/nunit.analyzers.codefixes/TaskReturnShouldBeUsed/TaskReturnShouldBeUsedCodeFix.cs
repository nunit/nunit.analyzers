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
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.TaskReturnShouldBeUsed
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class TaskReturnShouldBeUsedCodeFix : CodeFixProvider
    {
        internal const string UseTaskReturn = "Use Task return";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.TaskReturnShouldBeUsed);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            SyntaxNode node = root.FindNode(context.Span);

            if (node is not InvocationExpressionSyntax invocationExpression)
                return;
            if (node.Parent is not SyntaxNode parent)
                return;

            SyntaxNode newRoot;

            var properties = context.Diagnostics[0].Properties;

            MethodDeclarationSyntax? methodDeclaration = parent.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (!properties.TryGetValue(AnalyzerPropertyKeys.IsAsync, out string? isAsync))
            {
                // The method is not async nor does it return Task, but it should return Task.
                if (methodDeclaration is null || IsOverride(methodDeclaration))
                {
                    // We cannot change the method return type to be 'Task' if it is an override.
                    newRoot = UseTaskResult(root, invocationExpression, properties);
                }
                else
                {
                    var updatedExpression = SyntaxFactory.AwaitExpression(invocationExpression);

                    newRoot = CodeFixHelper.UpdateTestMethodSignatureIfNecessary(root, invocationExpression, updatedExpression, shouldBeAsync: true);
                }
            }
            else if (isAsync == "false")
            {
                // The method returns a Task, but is not async.
                // This likely means that the method does a return Task.FromResult(...) or returns a Task directly.
                // We cannot await our expression without changing all those operations,
                // but we can change the statement to use .Result or .Wait() instead.
                newRoot = UseTaskResult(root, invocationExpression, properties);
            }
            else
            {
                // The method is async, so we can await the method.
                var updatedExpression = SyntaxFactory.AwaitExpression(invocationExpression);
                newRoot = CodeFixHelper.UpdateTestMethodSignatureIfNecessary(root, invocationExpression, updatedExpression, shouldBeAsync: true);
            }

            var codeAction = CodeAction.Create(
                UseTaskReturn,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UseTaskReturn);
            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static SyntaxNode UseTaskResult(SyntaxNode root, InvocationExpressionSyntax invocationExpression, ImmutableDictionary<string, string?> properties)
        {
            ExpressionSyntax updatedExpression;

            if (properties.ContainsKey(AnalyzerPropertyKeys.IsTaskT))
            {
                updatedExpression = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    invocationExpression,
                    SyntaxFactory.IdentifierName(nameof(Task<string>.Result)));
            }
            else
            {
                updatedExpression = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        invocationExpression,
                        SyntaxFactory.IdentifierName(nameof(Task.Wait))));
            }

            SyntaxNode newRoot = root.ReplaceNode(invocationExpression, updatedExpression);
            return newRoot;
        }

        private static bool IsOverride(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
        }
    }
}
