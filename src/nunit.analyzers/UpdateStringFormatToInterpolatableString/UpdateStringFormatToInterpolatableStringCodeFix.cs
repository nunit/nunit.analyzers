using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.UpdateStringFormatToInterpolatableString
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    internal class UpdateStringFormatToInterpolatableStringCodeFix : CodeFixProvider
    {
        internal const string UpdateStringFormatToInterpolatableStringDescription =
            "Replace format specification and params with interpolatable string";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null || semanticModel is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var invocationNode = node as InvocationExpressionSyntax;

            if (invocationNode is null)
                return;

            List<ArgumentSyntax> arguments = invocationNode.ArgumentList.Arguments.ToList();

            var minimumNumberOfArguments = int.Parse(diagnostic.Properties[AnalyzerPropertyKeys.MinimumNumberOfArguments]!, CultureInfo.InvariantCulture);
            bool argsIsArray = !string.IsNullOrEmpty(diagnostic.Properties[AnalyzerPropertyKeys.ArgsIsArray]);

            CodeFixHelper.UpdateStringFormatToFormattableString(argsIsArray, arguments, minimumNumberOfArguments);

            var newArgumentsList = invocationNode.ArgumentList.WithArguments(arguments);
            var newInvocationNode = invocationNode.WithArgumentList(newArgumentsList);

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(invocationNode, newInvocationNode);

            var codeAction = CodeAction.Create(
                UpdateStringFormatToInterpolatableStringDescription,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UpdateStringFormatToInterpolatableStringDescription);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }
    }
}
