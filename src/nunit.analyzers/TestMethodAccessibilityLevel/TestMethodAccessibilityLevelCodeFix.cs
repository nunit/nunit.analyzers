using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.TestMethodAccessibilityLevel
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class TestMethodAccessibilityLevelCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.TestMethodIsNotPublic);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();

            var node = root.FindNode(context.Span);

            if (!(node is MethodDeclarationSyntax originalExpression))
                return;

            var newModifiers = ReplaceModifiers(originalExpression);
            var newExpression = originalExpression.WithModifiers(newModifiers);
            var newExpression2 = StripLeadingTriviaOnAddedModifier(originalExpression, newExpression);

            var newRoot = root.ReplaceNode(originalExpression, newExpression2);

            var codeAction = CodeAction.Create(
                CodeFixConstants.MakeTestMethodPublic,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                CodeFixConstants.MakeTestMethodPublic);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        static SyntaxTokenList ReplaceModifiers(MethodDeclarationSyntax originalExpression)
        {
            var firstAccessModifier = true;
            var newSyntaxTokens = new List<SyntaxToken>();

            foreach (var syntaxToken in originalExpression.Modifiers)
            {
                if (syntaxToken.IsKind(SyntaxKind.PrivateKeyword) ||
                    syntaxToken.IsKind(SyntaxKind.ProtectedKeyword) ||
                    syntaxToken.IsKind(SyntaxKind.InternalKeyword))
                {
                    if (firstAccessModifier)
                    {
                        firstAccessModifier = false;
                        var newPublicToken = SyntaxFactory.Token(
                            syntaxToken.LeadingTrivia,
                            SyntaxKind.PublicKeyword,
                            syntaxToken.TrailingTrivia);
                        newSyntaxTokens.Add(newPublicToken);
                    }
                }
                else
                {
                    newSyntaxTokens.Add(syntaxToken);
                }
            }

            if (firstAccessModifier)
            {
                newSyntaxTokens.Insert(0, SyntaxFactory.Token(
                            originalExpression.GetLeadingTrivia(),
                            SyntaxKind.PublicKeyword,
                            SyntaxTriviaList.Create(SyntaxFactory.Whitespace(" "))));
            }

            return new SyntaxTokenList(newSyntaxTokens);
        }

        static MethodDeclarationSyntax StripLeadingTriviaOnAddedModifier(
            MethodDeclarationSyntax originalExpression,
            MethodDeclarationSyntax newExpression)
        {
            if (!originalExpression.Modifiers.Any() && newExpression.ReturnType.HasLeadingTrivia)
            {
                var returnTypeWithNoLeadingTrivia = newExpression.ReturnType.WithLeadingTrivia(SyntaxTriviaList.Empty);
                newExpression = newExpression.WithReturnType(returnTypeWithNoLeadingTrivia);
            }

            return newExpression;
        }
    }
}
