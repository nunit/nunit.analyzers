using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

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

            // First, replace the original method invocation name to "That".
            var newInvocationNode = invocationNode.ReplaceNode(
                invocationNode.DescendantNodes(_ => true).Where(_ =>
                    _.IsKind(SyntaxKind.IdentifierName) &&
                    (_ as IdentifierNameSyntax).Identifier.Text == invocationIdentifier).Single(),
                SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfAssertThat));

            // Now, replace the arguments.
            var arguments = invocationNode.ArgumentList.Arguments.ToList();
            this.UpdateArguments(diagnostic, arguments);

            var totalArgumentCount = (2 * arguments.Count) - 1;
            var newArgumentList = new SyntaxNodeOrToken[totalArgumentCount];

            context.CancellationToken.ThrowIfCancellationRequested();

            for (var x = 0; x < totalArgumentCount; x++)
            {
                if (x % 2 == 0)
                {
                    newArgumentList[x] = arguments[x / 2];
                }
                else
                {
                    newArgumentList[x] = SyntaxFactory.Token(SyntaxKind.CommaToken);
                }
            }

            newInvocationNode = newInvocationNode.WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(newArgumentList)));

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
