using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ConstActualValueUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class ConstActualValueUsageCodeFix : CodeFixProvider
    {
        private static readonly string[] SupportedClassicAsserts = new[]
        {
            NunitFrameworkConstants.NameOfAssertAreEqual,
            NunitFrameworkConstants.NameOfAssertAreNotEqual,
            NunitFrameworkConstants.NameOfAssertAreSame,
            NunitFrameworkConstants.NameOfAssertAreNotSame
        };

        private static readonly string[] SupportedIsConstraints = new[]
        {
            NunitFrameworkConstants.NameOfIsEqualTo,
            NunitFrameworkConstants.NameOfIsSameAs,
            NunitFrameworkConstants.NameOfIsSamePath
        };

        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AnalyzerIdentifiers.ConstActualValueUsage);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

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

            var codeAction = CodeAction.Create(
                CodeFixConstants.SwapArgumentsDescription,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                CodeFixConstants.SwapArgumentsDescription);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static bool TryFindArguments(SemanticModel semanticModel, InvocationExpressionSyntax invocationSyntax,
            [NotNullWhen(true)] out ExpressionSyntax? expectedArgument, [NotNullWhen(true)] out ExpressionSyntax? actualArgument)
        {
            expectedArgument = null;
            actualArgument = null;

            var methodSymbol = semanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;

            if (methodSymbol == null || !methodSymbol.ContainingType.IsAssert())
                return false;

            // option 1: Classic assert (e.g. Assert.AreEqual(expected, actual) )
            if (SupportedClassicAsserts.Contains(methodSymbol.Name) && methodSymbol.Parameters.Length >= 2)
            {
                expectedArgument = invocationSyntax.ArgumentList.Arguments[0].Expression;
                actualArgument = invocationSyntax.ArgumentList.Arguments[1].Expression;
                return true;
            }

            // option 2: Assert with 'actual' and 'constraint' parameters
            // (e.g. Assert.That(actual, Is.EqualTo(expected)))
            if (methodSymbol.Name == NunitFrameworkConstants.NameOfAssertThat
                && methodSymbol.Parameters.Length >= 2)
            {
                actualArgument = invocationSyntax.ArgumentList.Arguments[0].Expression;

                var constraintExpression = invocationSyntax.ArgumentList.Arguments[1].Expression as InvocationExpressionSyntax;

                if (constraintExpression == null)
                    return false;

                expectedArgument = constraintExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression;

                if (expectedArgument == null)
                    return false;

                if (constraintExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression
                    && SupportedIsConstraints.Contains(memberAccessExpression.Name.ToString()))
                {
                    var expressionString = memberAccessExpression.Expression.ToString();

                    // e.g. Is.EqualTo or Is.Not.EqualTo
                    if (expressionString == NunitFrameworkConstants.NameOfIs
                        || expressionString == $"{NunitFrameworkConstants.NameOfIs}.{NunitFrameworkConstants.NameOfIsNot}")
                    {
                        return true;
                    }

                    // other cases are not supported
                    return false;
                }
            }

            return false;
        }
    }
}
