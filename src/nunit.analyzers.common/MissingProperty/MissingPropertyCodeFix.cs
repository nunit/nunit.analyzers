using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.MissingProperty
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class MissingPropertyCodeFix : CodeFixProvider
    {
        internal const string UsePropertyDescriptionFormat = "Use '{0}' property";

        private static readonly Dictionary<string, string> supportedCodeFixes = new()
        {
            { NUnitFrameworkConstants.NameOfHasCount, NUnitFrameworkConstants.NameOfHasLength },
            { NUnitFrameworkConstants.NameOfHasLength, NUnitFrameworkConstants.NameOfHasCount }
        };

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.MissingProperty);

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

            if (node is not ExpressionSyntax originalExpression)
                return;

            var prefixName = originalExpression.GetName();

            var diagnostic = context.Diagnostics.First();

            if (originalExpression is MemberAccessExpressionSyntax &&
                prefixName is not null &&
                supportedCodeFixes.TryGetValue(prefixName, out var target) &&
                diagnostic.Properties.ContainsKey(target))
            {
                var memberAccess = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfHas),
                        SyntaxFactory.IdentifierName(target));

                var newRoot = root.ReplaceNode(originalExpression, memberAccess);

                var description = string.Format(CultureInfo.InvariantCulture,
                        UsePropertyDescriptionFormat, target);

                var codeAction = CodeAction.Create(
                    description,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    description);

                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
        }
    }
}
