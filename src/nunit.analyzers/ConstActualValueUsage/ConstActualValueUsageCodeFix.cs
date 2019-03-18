using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.ConstActualValueUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class ConstActualValueUsageCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.ConstActualValueUsage);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();

            var argumentSyntax = root.FindNode(context.Span);
            var invocationSyntax = argumentSyntax.Ancestors()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (invocationSyntax == null)
                return;

            if (!TryFindArguments(semanticModel, invocationSyntax,
                out var expectedArgument,
                out var actualArgument))
            {
                return;
            }

            var newRoot = root
                .ReplaceNodes(new[] { expectedArgument, actualArgument },
                    (node, _) => node == actualArgument ? expectedArgument : actualArgument);

            context.RegisterCodeFix(
                CodeAction.Create(
                    CodeFixConstants.SwapArgumentsDescription,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot))), context.Diagnostics);
        }

        private static bool TryFindArguments(SemanticModel semanticModel, InvocationExpressionSyntax invocationSyntax,
            out ExpressionSyntax expectedArgument, out ExpressionSyntax actualArgument)
        {
            expectedArgument = null;
            actualArgument = null;

            var methodSymbol = semanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;

            if (methodSymbol == null)
                return false;

            // option 1: Assert with 'expected' and 'actual' parameters (e.g. Assert.AreEqual(expected, actual) )
            if (methodSymbol.Parameters.Length >= 2
                && methodSymbol.Parameters[0].Name == NunitFrameworkConstants.NameOfExpectedParameter
                && methodSymbol.Parameters[1].Name == NunitFrameworkConstants.NameOfActualParameter)
            {
                expectedArgument = invocationSyntax.ArgumentList.Arguments[0].Expression;
                actualArgument = invocationSyntax.ArgumentList.Arguments[1].Expression;
                return true;
            }

            // option 2: Assert with 'actual' and 'constraint' parameters, and provided constraint has 'expected' parameter
            // (e.g. Assert.That(actual, Is.EqualTo(expected)))
            if (methodSymbol.Parameters.Length >= 2
                && methodSymbol.Parameters[0].Name == NunitFrameworkConstants.NameOfActualParameter
                && methodSymbol.Parameters[1].Name == NunitFrameworkConstants.NameOfExpressionParameter)
            {
                actualArgument = invocationSyntax.ArgumentList.Arguments[0].Expression;

                var constraintExpression = invocationSyntax.ArgumentList.Arguments[1].Expression as InvocationExpressionSyntax;

                if (constraintExpression == null)
                    return false;

                var constraintMethod = semanticModel.GetSymbolInfo(constraintExpression).Symbol as IMethodSymbol;

                if (constraintMethod == null)
                    return false;

                if (constraintMethod.Parameters.Length == 1
                    && constraintMethod.Parameters[0].Name == NunitFrameworkConstants.NameOfExpectedParameter)
                {
                    expectedArgument = constraintExpression.ArgumentList.Arguments[0].Expression;
                    return true;
                }
            }

            return false;
        }
    }
}
