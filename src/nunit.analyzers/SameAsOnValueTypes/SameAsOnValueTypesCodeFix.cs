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

namespace NUnit.Analyzers.SameAsOnValueTypes
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class SameAsOnValueTypesCodeFix : CodeFixProvider
    {
        internal const string UseIsEqualToDescription = "Use IsEqualTo on value types";

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.SameAsOnValueTypes);

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
            var argumentNode = node as ArgumentSyntax;
            if (argumentNode is null)
                return;

            var invocationNode = argumentNode.Parent?.Parent as InvocationExpressionSyntax;

            if (invocationNode is null)
                return;

            var assertExpression = invocationNode.Expression as MemberAccessExpressionSyntax;

            if (assertExpression is null)
                return;

            ExpressionSyntax original = assertExpression;
            ExpressionSyntax replacement;

            switch (assertExpression.Name.ToString())
            {
                case NUnitFrameworkConstants.NameOfAssertAreSame:
                    replacement = assertExpression.WithName(SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssertAreEqual));
                    break;
                case NUnitFrameworkConstants.NameOfAssertAreNotSame:
                    replacement = assertExpression.WithName(SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssertAreNotEqual));
                    break;
                case NUnitFrameworkConstants.NameOfIsSameAs:
                    replacement = assertExpression.WithName(SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEqualTo));
                    break;
                default:
                    return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(original, replacement);

            context.RegisterCodeFix(
                CodeAction.Create(
                    UseIsEqualToDescription,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    UseIsEqualToDescription), diagnostic);
        }
    }
}
