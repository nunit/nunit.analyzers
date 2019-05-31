using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.SameActualExpectedValue
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SameActualExpectedValueAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            AnalyzerIdentifiers.SameActualExpectedValue,
            SameActualExpectedValueAnalyzerConstants.Title,
            SameActualExpectedValueAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            if (!AssertExpressionHelper.TryGetActualAndConstraintExpressions(assertExpression,
                out var actualExpression, out var constraintExpression))
            {
                return;
            }

            var expectedExpressions = AssertExpressionHelper.GetExpectedArguments(constraintExpression, context.SemanticModel);
            var sameExpectedExpressions = expectedExpressions.Where(e => e.expectedArgument.IsEquivalentTo(actualExpression));

            foreach (var (expected, _) in sameExpectedExpressions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    expected.GetLocation()));
            }
        }
    }
}
