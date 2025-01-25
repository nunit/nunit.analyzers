using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.DelegateRequired
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class DelegateRequiredCodeFix : CodeFixProvider
    {
        internal const string UseAnonymousLambdaDescription = "Use an anonymous lambda so the Assert does the evaluation";
        internal const string UseMethodGroupDescription = "Use a method group delegate so the Assert does the evaluation";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.DelegateRequired);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var argumentNode = node as ArgumentSyntax;
            if (argumentNode is null)
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            ArgumentSyntax original = argumentNode;
            ArgumentSyntax replacement = argumentNode.WithExpression(SyntaxFactory.ParenthesizedLambdaExpression(argumentNode.Expression));

            context.RegisterCodeFix(
                CodeAction.Create(
                    UseAnonymousLambdaDescription,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(original, replacement))),
                    UseAnonymousLambdaDescription), diagnostic);

            if (argumentNode.Expression is InvocationExpressionSyntax invocationNode &&
                invocationNode.ArgumentList.Arguments.Count == 0)
            {
                var methodGroup = SyntaxFactory.IdentifierName(invocationNode.Expression.ToString());
                replacement = argumentNode.WithExpression(methodGroup);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        UseMethodGroupDescription,
                        _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(original, replacement))),
                        UseMethodGroupDescription), diagnostic);
            }
        }
    }
}
