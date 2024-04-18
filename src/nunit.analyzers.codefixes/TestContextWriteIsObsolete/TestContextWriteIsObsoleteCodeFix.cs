using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.TestContextWriteIsObsolete
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class TestContextWriteIsObsoleteCodeFix : CodeFixProvider
    {
        internal const string InsertOutDescription = "Replace TestContext.Write with TestContext.Out.Write";

        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(AnalyzerIdentifiers.TestContextWriteIsObsolete);

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
            var invocationNode = node as InvocationExpressionSyntax;
            if (invocationNode is null)
                return;

            var memberAccessExpression = invocationNode.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpression is null)
                return;

            var updatedMemberAccessExpression =
                SyntaxFactory.MemberAccessExpression(
                    memberAccessExpression.Kind(),
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        memberAccessExpression.Expression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfOut)),
                    memberAccessExpression.Name);

            var newRoot = root.ReplaceNode(memberAccessExpression, updatedMemberAccessExpression);

            var codeAction = CodeAction.Create(
                InsertOutDescription,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                InsertOutDescription);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
