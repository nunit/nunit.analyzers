using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.WithinUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class WithinUsageCodeFix : CodeFixProvider
    {
        internal const string RemoveWithinDescription = "Remove redundant Within() call";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AnalyzerIdentifiers.WithinIncompatibleTypes);

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

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var withinCallNode = node as IdentifierNameSyntax;
            if (withinCallNode is null)
                return;

            var memberAccessExpression = withinCallNode.Parent as MemberAccessExpressionSyntax;
            if (memberAccessExpression is null)
                return;

            var expressionToKeep = memberAccessExpression.Expression;

            var argument = memberAccessExpression.Parent?.Parent as ArgumentSyntax;
            if (argument is null)
                return;

            var expressionToReplace = argument.Expression;

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(expressionToReplace, expressionToKeep);

            context.RegisterCodeFix(
                CodeAction.Create(
                    RemoveWithinDescription,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    RemoveWithinDescription), diagnostic);
        }
    }
}
