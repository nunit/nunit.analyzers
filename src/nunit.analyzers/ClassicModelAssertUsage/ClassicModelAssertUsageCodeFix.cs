using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    public abstract class ClassicModelAssertUsageCodeFix
        : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        protected abstract void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.First();
            var invocationNode = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;
            var invocationIdentifier = diagnostic.Properties[AnalyzerPropertyKeys.ModelName];

            if (invocationNode == null)
                return;

            // First, replace the original method invocation name to "That".
            var newInvocationNode = invocationNode.ReplaceNode(
                invocationNode.DescendantNodes(_ => true).Where(_ =>
                    _.IsKind(SyntaxKind.IdentifierName) &&
                    ((IdentifierNameSyntax)_).Identifier.Text == invocationIdentifier).Single(),
                SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfAssertThat));

            // Now, replace the arguments.
            var arguments = invocationNode.ArgumentList.Arguments.ToList();
            this.UpdateArguments(diagnostic, arguments);

            var newArgumentsList = invocationNode.ArgumentList.WithArguments(arguments);
            newInvocationNode = newInvocationNode.WithArgumentList(newArgumentsList);

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(invocationNode, newInvocationNode);

            context.RegisterCodeFix(
                CodeAction.Create(
                    CodeFixConstants.TransformToConstraintModelDescription,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    CodeFixConstants.TransformToConstraintModelDescription), diagnostic);
        }
    }
}
