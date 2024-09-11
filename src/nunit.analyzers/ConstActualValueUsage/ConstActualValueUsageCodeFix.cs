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
        internal const string SwapArgumentsDescription = "Swap actual and expected arguments";

        private static readonly string[] SupportedClassicAsserts = new[]
        {
            NUnitFrameworkConstants.NameOfAssertAreEqual,
            NUnitFrameworkConstants.NameOfAssertAreNotEqual,
            NUnitFrameworkConstants.NameOfAssertAreSame,
            NUnitFrameworkConstants.NameOfAssertAreNotSame,
        };

        private static readonly string[] SupportedStringAsserts = new[]
        {
            NUnitFrameworkConstants.NameOfStringAssertAreEqualIgnoringCase,
            NUnitFrameworkConstants.NameOfStringAssertAreNotEqualIgnoringCase,
            NUnitFrameworkConstants.NameOfStringAssertContains,
            NUnitFrameworkConstants.NameOfStringAssertDoesNotContain,
            NUnitFrameworkConstants.NameOfStringAssertDoesNotEndWith,
            NUnitFrameworkConstants.NameOfStringAssertDoesNotMatch,
            NUnitFrameworkConstants.NameOfStringAssertDoesNotStartWith,
            NUnitFrameworkConstants.NameOfStringAssertEndsWith,
            NUnitFrameworkConstants.NameOfStringAssertIsMatch,
            NUnitFrameworkConstants.NameOfStringAssertStartsWith,
        };

        private static readonly string[] SupportedIsConstraints = new[]
        {
            NUnitFrameworkConstants.NameOfIsEqualTo,
            NUnitFrameworkConstants.NameOfIsSameAs,
            NUnitFrameworkConstants.NameOfIsSamePath
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

            if (root is null || semanticModel is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var argumentSyntax = root.FindNode(context.Span);
            var invocationSyntax = argumentSyntax.Ancestors()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (invocationSyntax is null)
                return;

            if (!TryFindArguments(semanticModel, invocationSyntax,
                out var expectedArgument,
                out var actualArgument))
            {
                return;
            }

            var newRoot = root
                .ReplaceNodes(new[] { expectedArgument, actualArgument },
                    (node, _) => node == actualArgument
                        ? expectedArgument.WithTriviaFrom(actualArgument)
                        : actualArgument.WithTriviaFrom(expectedArgument));

            var codeAction = CodeAction.Create(
                SwapArgumentsDescription,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                SwapArgumentsDescription);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static bool TryFindArguments(SemanticModel semanticModel, InvocationExpressionSyntax invocationSyntax,
            [NotNullWhen(true)] out ExpressionSyntax? expectedArgument, [NotNullWhen(true)] out ExpressionSyntax? actualArgument)
        {
            expectedArgument = null;
            actualArgument = null;

            var methodSymbol = semanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;

            if (methodSymbol is null)
                return false;

            // option 1: Classic assert (e.g. ClassicAssert.AreEqual(expected, actual) )
            if ((IsSupportedClassicAssert(methodSymbol) || IsSupportedStringAssert(methodSymbol))
                && methodSymbol.Parameters.Length >= 2)
            {
                expectedArgument = invocationSyntax.ArgumentList.Arguments[0].Expression;
                actualArgument = invocationSyntax.ArgumentList.Arguments[1].Expression;
                return true;
            }

            // option 2: Assert with 'actual' and 'constraint' parameters
            // (e.g. Assert.That(actual, Is.EqualTo(expected)))
            if (methodSymbol.ContainingType.IsAssert() && methodSymbol.Name == NUnitFrameworkConstants.NameOfAssertThat
                && methodSymbol.Parameters.Length >= 2)
            {
                actualArgument = invocationSyntax.ArgumentList.Arguments[0].Expression;

                var constraintExpression = invocationSyntax.ArgumentList.Arguments[1].Expression as InvocationExpressionSyntax;

                if (constraintExpression is null)
                    return false;

                if (!IsSupportedIsConstraint(ref constraintExpression, out var memberAccessExpression))
                {
                    return false;
                }

                var expressionString = memberAccessExpression.Expression.ToString();

                // e.g. Is.EqualTo or Is.Not.EqualTo
                if (expressionString != NUnitFrameworkConstants.NameOfIs
                    && expressionString != $"{NUnitFrameworkConstants.NameOfIs}.{NUnitFrameworkConstants.NameOfIsNot}")
                {
                    // other cases are not supported
                    return false;
                }

                expectedArgument = constraintExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression;

                return expectedArgument is not null;
            }

            return false;
        }

        private static bool IsSupportedClassicAssert(IMethodSymbol methodSymbol)
        {
            return (methodSymbol.ContainingType.IsClassicAssert() || methodSymbol.ContainingType.IsAssert()) &&
                SupportedClassicAsserts.Contains(methodSymbol.Name);
        }

        private static bool IsSupportedIsConstraint(ref InvocationExpressionSyntax invocationExpression, [NotNullWhen(true)]out MemberAccessExpressionSyntax? memberAccessExpression)
        {
            memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpression is null)
            {
                return false;
            }

            if (memberAccessExpression.Name.ToString() == NUnitFrameworkConstants.NameOfEqualConstraintWithin)
            {
                // e.g. Is.EqualTo(1).Within(1) or Is.Not.EqualTo(1).Within(1)
                if (memberAccessExpression.Expression is not InvocationExpressionSyntax innerInvocationExpression
                    || innerInvocationExpression.Expression is not MemberAccessExpressionSyntax left)
                {
                    return false;
                }

                memberAccessExpression = left;
                invocationExpression = innerInvocationExpression;
            }

            return SupportedIsConstraints.Contains(memberAccessExpression.Name.ToString());
        }

        private static bool IsSupportedStringAssert(IMethodSymbol methodSymbol)
        {
            return methodSymbol.ContainingType.IsStringAssert() && SupportedStringAsserts.Contains(methodSymbol.Name);
        }
    }
}
