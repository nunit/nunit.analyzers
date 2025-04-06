using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.TestFixtureShouldBeAbstract
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class TestFixtureShouldBeAbstractCodeFix : CodeFixProvider
    {
        internal const string MakeBaseTestFixtureAbstract = "Make base test fixture abstract";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.BaseTestFixtureIsNotAbstract);

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

            if (node is not ClassDeclarationSyntax originalExpression)
                return;

            // Add the abstract modifier
            SyntaxTokenList updatedModifiers = AddAbstractModifier(originalExpression);

            ClassDeclarationSyntax newExpression = originalExpression.WithoutLeadingTrivia()
                                                                     .WithModifiers(updatedModifiers);

            var newRoot = root.ReplaceNode(originalExpression, newExpression);

            var codeAction = CodeAction.Create(
                MakeBaseTestFixtureAbstract,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                MakeBaseTestFixtureAbstract);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static SyntaxTokenList AddAbstractModifier(ClassDeclarationSyntax originalExpression)
        {
            var modifiers = originalExpression.Modifiers;

            var abstractSyntax = SyntaxFactory.Token(SyntaxKind.AbstractKeyword)
                                              .WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Whitespace(" ")));

            if (!modifiers.Any())
            {
                // Get leading trivia from declaration
                abstractSyntax = abstractSyntax.WithLeadingTrivia(originalExpression.GetLeadingTrivia());
            }

            return modifiers.Add(abstractSyntax);
        }
    }
}
