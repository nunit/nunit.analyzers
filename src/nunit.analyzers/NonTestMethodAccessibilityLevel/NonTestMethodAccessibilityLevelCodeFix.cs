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

namespace NUnit.Analyzers.NonTestMethodAccessibilityLevel
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class NonTestMethodAccessibilityLevelCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.NonTestMethodIsPublic);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();

            SyntaxNode node = root.FindNode(context.Span);

            if (!(node is MethodDeclarationSyntax originalExpression))
                return;

            if (HasExplicitPublicAccessModifier(originalExpression))
            {
                SyntaxTokenList replacedModifiers = ReplaceModifiersWithPrivate(originalExpression);
                MethodDeclarationSyntax newExpression = originalExpression.WithModifiers(replacedModifiers);

                SyntaxNode newRoot = root.ReplaceNode(originalExpression, newExpression);

                var codeAction = CodeAction.Create(
                    CodeFixConstants.MakeNonTestMethodPrivate,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    CodeFixConstants.MakeNonTestMethodPrivate);

                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
        }

        private static SyntaxTokenList ReplaceModifiersWithPrivate(MethodDeclarationSyntax originalExpression)
        {
            var firstAccessModifier = true;
            var isProtected = false;
            var newSyntaxTokens = new List<SyntaxToken>();

            foreach (var syntaxToken in originalExpression.Modifiers)
            {
                if (IsPublicAccessModifier(syntaxToken))
                {
                    if (firstAccessModifier && !isProtected)
                    {
                        firstAccessModifier = false;
                        var newPrivateToken = SyntaxFactory.Token(
                            syntaxToken.LeadingTrivia,
                            SyntaxKind.PrivateKeyword,
                            syntaxToken.TrailingTrivia);
                        newSyntaxTokens.Add(newPrivateToken);
                    }
                }
                else
                {
                    if (syntaxToken.IsKind(SyntaxKind.ProtectedKeyword))
                        isProtected = true;

                    newSyntaxTokens.Add(syntaxToken);
                }
            }

            return new SyntaxTokenList(newSyntaxTokens);
        }

        private static bool HasExplicitPublicAccessModifier(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return methodDeclarationSyntax.Modifiers.Any(m => IsPublicAccessModifier(m));
        }

        private static bool IsPublicAccessModifier(SyntaxToken syntaxToken) =>
            syntaxToken.IsKind(SyntaxKind.PublicKeyword) ||
            syntaxToken.IsKind(SyntaxKind.InternalKeyword);
    }
}
