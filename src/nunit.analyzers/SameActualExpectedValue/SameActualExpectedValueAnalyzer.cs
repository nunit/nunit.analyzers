using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.SameActualExpectedValue
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SameActualExpectedValueAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            AnalyzerIdentifiers.SameActualExpectedValue,
            SameActualExpectedValueAnalyzerConstants.Title,
            SameActualExpectedValueAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is InvocationExpressionSyntax invocationSyntax))
                return;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;

            if (methodSymbol == null
                || !methodSymbol.ContainingType.IsAssert()
                || methodSymbol.Name != NunitFrameworkConstants.NameOfAssertThat
                || methodSymbol.Parameters.Length < 2)
            {
                return;
            }

            var actualExpression = invocationSyntax.ArgumentList.Arguments[0].Expression;
            var constraintExpression = invocationSyntax.ArgumentList.Arguments[1].Expression;

            var expectedExpressions = GetExpectedArgumentsFromConstraintExpression(constraintExpression, context.SemanticModel);
            var sameExpectedExpressions = expectedExpressions.Where(e => e.IsEquivalentTo(actualExpression));

            foreach (var expected in sameExpectedExpressions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    expected.GetLocation()));
            }
        }

        private static List<ExpressionSyntax> GetExpectedArgumentsFromConstraintExpression(ExpressionSyntax constraintExpression, SemanticModel semanticModel)
        {
            var expectedArguments = new List<ExpressionSyntax>();

            var constraintParts = SplitConstraintByOperators(constraintExpression);

            foreach (var constraintPart in constraintParts)
            {
                var invocations = constraintPart.SplitCallChain()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(i => i.ArgumentList.Arguments.Count == 1);

                foreach (var invocation in invocations)
                {
                    var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;

                    if (symbol is IMethodSymbol methodSymbol
                        && methodSymbol.Parameters.Length == 1
                        && methodSymbol.Parameters[0].Name == NunitFrameworkConstants.NameOfExpectedParameter)
                    {
                        var argument = invocation.ArgumentList.Arguments[0];
                        expectedArguments.Add(argument.Expression);
                    }
                }
            }

            return expectedArguments;
        }

        private static IEnumerable<ExpressionSyntax> SplitConstraintByOperators(ExpressionSyntax constraintExpression)
        {
            if (constraintExpression is BinaryExpressionSyntax binaryExpression)
            {
                foreach (var leftPart in SplitConstraintByOperators(binaryExpression.Left))
                    yield return leftPart;

                foreach (var rightPart in SplitConstraintByOperators(binaryExpression.Right))
                    yield return rightPart;
            }
            else
            {
                yield return constraintExpression;
            }
        }
    }
}
