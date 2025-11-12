using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.InstanceOf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InstanceOfAnalyzer : BaseAssertionAnalyzer
    {
        internal const string IsConstraintIsTrue = nameof(IsConstraintIsTrue);

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.InstanceOf,
            title: InstanceOfConstants.Title,
            messageFormat: InstanceOfConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: InstanceOfConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            if (actualOperation is null)
                return;

            var isConstraintTrue = constraintExpression.IsTrueOrFalse();

            if (actualOperation is IIsTypeOperation isTypeOperation && isConstraintTrue is not null)
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add(IsConstraintIsTrue, isConstraintTrue.ToString());

                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    actualOperation.Syntax.GetLocation(),
                    properties.ToImmutable(),
                    isTypeOperation.TypeOperand.ToString()));
            }
        }
    }
}
