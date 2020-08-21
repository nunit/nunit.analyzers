using System.Collections.Generic;
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

            MethodDeclarationSyntax newExpression;
            if (HasExplicitNonPublicAccessModifier(originalExpression))
            {
                var replacedModifiers = ReplaceModifiersWithPublic(originalExpression);
                newExpression = originalExpression.WithModifiers(replacedModifiers);
            }
            else
            {
                var addedPublicModifier = AddPublicModifier(originalExpression);
                var returnTypeWithoutLeadingTrivia = StripLeadingTriviaFromReturnType(originalExpression);
                newExpression = originalExpression
                    .WithModifiers(addedPublicModifier)
                    .WithReturnType(returnTypeWithoutLeadingTrivia);
            }

            var newRoot = root.ReplaceNode(originalExpression, newExpression);

            var codeAction = CodeAction.Create(
                CodeFixConstants.MakeTestMethodPublic,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                CodeFixConstants.MakeTestMethodPublic);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static SyntaxTokenList ReplaceModifiersWithPublic(MethodDeclarationSyntax originalExpression)
        {
            var firstAccessModifier = true;
            var newSyntaxTokens = new List<SyntaxToken>();

            foreach (var syntaxToken in originalExpression.Modifiers)
            {
                if (IsNonPublicAccessModifier(syntaxToken))
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

            return new SyntaxTokenList(newSyntaxTokens);
        }

        private static SyntaxTokenList AddPublicModifier(MethodDeclarationSyntax originalExpression)
        {
            var modifiers = originalExpression.Modifiers;
            var syntaxTriviaList = modifiers.Any()
                ? modifiers.First().LeadingTrivia
                : originalExpression.ReturnType.GetLeadingTrivia();

            var publicSyntax = SyntaxFactory.Token(
                        syntaxTriviaList,
                        SyntaxKind.PublicKeyword,
                        SyntaxTriviaList.Create(SyntaxFactory.Whitespace(" ")));

            var publicModifierInserted = modifiers.Insert(0, publicSyntax);

            if (modifiers.Any())
            {
                var nextModifier = publicModifierInserted[1];
                var nextModifierWithNoLeadingTrivia = nextModifier.WithLeadingTrivia(SyntaxTriviaList.Empty);
                publicModifierInserted = publicModifierInserted.Replace(nextModifier, nextModifierWithNoLeadingTrivia);
            }

            return publicModifierInserted;
        }

        private static TypeSyntax StripLeadingTriviaFromReturnType(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            if (methodDeclarationSyntax.ReturnType.HasLeadingTrivia)
            {
                return methodDeclarationSyntax.ReturnType.WithLeadingTrivia(SyntaxTriviaList.Empty);
            }

            return methodDeclarationSyntax.ReturnType;
        }

        private static bool HasExplicitNonPublicAccessModifier(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return methodDeclarationSyntax.Modifiers.Any(m => IsNonPublicAccessModifier(m));
        }

        private static bool IsNonPublicAccessModifier(SyntaxToken syntaxToken) =>
            syntaxToken.IsKind(SyntaxKind.PrivateKeyword) ||
            syntaxToken.IsKind(SyntaxKind.ProtectedKeyword) ||
            syntaxToken.IsKind(SyntaxKind.InternalKeyword);
    }
}
