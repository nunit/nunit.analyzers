using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

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
            var actualOperation = assertOperation.GetArgumentOperation(NUnitFrameworkConstants.NameOfActualParameter)
                ?? assertOperation.GetArgumentOperation(NUnitFrameworkConstants.NameOfConditionParameter);

            if (actualOperation is null)
                return;

            var isConstraintTrue = IsConstraintIsTrueOrEmpty(assertOperation);

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

        private static bool? IsConstraintIsTrueOrEmpty(IInvocationOperation assertOperation)
        {
            var expectedOperation = assertOperation.GetArgumentOperation(NUnitFrameworkConstants.NameOfExpressionParameter);

            if (expectedOperation is null)
                return true;

            if (expectedOperation is not IPropertyReferenceOperation propertyReference
                || propertyReference.Property.ContainingType.Name != NUnitFrameworkConstants.NameOfIs)
            {
                return null;
            }

            return propertyReference.Property.Name switch
            {
                NUnitFrameworkConstants.NameOfIsTrue => true,
                NUnitFrameworkConstants.NameOfIsFalse => false,
                _ => null
            };
        }
    }
}
