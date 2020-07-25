using System;
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

        protected virtual void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments, TypeArgumentListSyntax typeArguments)
        {
            throw new InvalidOperationException($"Class must override {nameof(UpdateArguments)} accepting {nameof(TypeArgumentListSyntax)}");
        }

        protected virtual void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
        {
            throw new InvalidOperationException($"Class must override {nameof(UpdateArguments)}");
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.First();
            var invocationNode = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;

            if (invocationNode == null)
                return;

            var invocationIdentifier = diagnostic.Properties[AnalyzerPropertyKeys.ModelName];
            var isGenericMethod = diagnostic.Properties[AnalyzerPropertyKeys.IsGenericMethod] == true.ToString();

            // First, replace the original method invocation name to "That".
            var descendantNodes = invocationNode.DescendantNodes(_ => true).ToArray();

            SyntaxNode methodNode;
            TypeArgumentListSyntax? typeArguments;
            if (isGenericMethod)
            {
                methodNode = descendantNodes
                    .Where(_ => _.IsKind(SyntaxKind.GenericName) &&
                        ((GenericNameSyntax)_).Identifier.Text == invocationIdentifier)
                    .Single();
                typeArguments = descendantNodes
                    .Where(_ => _.IsKind(SyntaxKind.TypeArgumentList))
                    .Cast<TypeArgumentListSyntax>()
                    .First();
            }
            else
            {
                methodNode = descendantNodes
                    .Where(_ => _.IsKind(SyntaxKind.IdentifierName) &&
                        ((IdentifierNameSyntax)_).Identifier.Text == invocationIdentifier)
                    .Single();
                typeArguments = null;
            }

            var newInvocationNode = invocationNode.ReplaceNode(
                methodNode,
                SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfAssertThat));

            // Now, replace the arguments.
            var arguments = invocationNode.ArgumentList.Arguments.ToList();
            if (typeArguments == null)
                this.UpdateArguments(diagnostic, arguments);
            else
                this.UpdateArguments(diagnostic, arguments, typeArguments);

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
