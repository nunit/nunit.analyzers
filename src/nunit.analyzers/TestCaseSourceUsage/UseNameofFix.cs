using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.TestCaseSourceUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofFix))]
    [Shared]
    public class UseNameofFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.TestCaseSourceStringUsage);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is LiteralExpressionSyntax literal)
                {
                    var nameOfTarget = diagnostic.Properties[TestCaseSourceUsageConstants.PropertyKeyNameOfTarget];

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use nameof()",
                            _ => Task.FromResult(
                                context.Document.WithSyntaxRoot(
                                    syntaxRoot.ReplaceNode(
                                        literal,
                                        SyntaxFactory.ParseExpression($"nameof({nameOfTarget})")))),
                            nameof(UseNameofFix)),
                        diagnostic);
                }
            }
        }
    }
}
