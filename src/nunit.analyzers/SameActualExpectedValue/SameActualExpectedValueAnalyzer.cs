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
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.SameActualExpectedValue,
            title: SameActualExpectedValueAnalyzerConstants.Title,
            messageFormat: SameActualExpectedValueAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: SameActualExpectedValueAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            if (!AssertHelper.TryGetActualAndConstraintExpressions(assertExpression, context.SemanticModel,
                out var actualExpression, out var constraintExpression))
            {
                return;
            }

            var sameExpectedExpressions = constraintExpression.ConstraintParts
                .Select(part => part.GetExpectedArgumentExpression())
                .Where(e => e != null && actualExpression.IsEquivalentTo(e));

            foreach (var expected in sameExpectedExpressions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    expected!.GetLocation(),
                    expected.ToString()));
            }
        }
    }
}
