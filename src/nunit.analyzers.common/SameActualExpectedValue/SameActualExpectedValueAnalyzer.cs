using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
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

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            var actualSyntax = actualOperation.Syntax;

            var sameExpectedExpressions = constraintExpression.ConstraintParts
                .Select(part => part.GetExpectedArgument()?.Syntax)
                .Where(e => e is not null && actualSyntax.IsEquivalentTo(e));

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
