using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.ConstActualValueUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstActualValueUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ConstActualValueUsage,
            title: ConstActualValueUsageAnalyzerConstants.Title,
            messageFormat: ConstActualValueUsageAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ConstActualValueUsageAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            static bool IsStringEmpty(IOperation operation)
            {
                return operation is IFieldReferenceOperation propertyReference
                    && propertyReference.Field.Name == nameof(string.Empty)
                    && propertyReference.Field.ContainingType.SpecialType == SpecialType.System_String;
            }

            void Report(IOperation operation)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    operation.Syntax.GetLocation()));
            }

            var actualOperation = assertOperation.GetArgumentOperation(NUnitFrameworkConstants.NameOfActualParameter);

            if (actualOperation is null)
                return;

            if (AssertHelper.IsLiteralOperation(actualOperation))
            {
                Report(actualOperation);
                return;
            }

            if (!actualOperation.ConstantValue.HasValue && !IsStringEmpty(actualOperation))
                return;

            // The actual expression is a constant field, check if expected is also constant
            var expectedOperation = GetExpectedOperation(assertOperation);

            if (expectedOperation is not null && !expectedOperation.ConstantValue.HasValue && !IsStringEmpty(expectedOperation))
            {
                Report(actualOperation);
            }
        }

        private static IOperation? GetExpectedOperation(IInvocationOperation assertOperation)
        {
            var expectedOperation = assertOperation.GetArgumentOperation(NUnitFrameworkConstants.NameOfExpectedParameter);

            // Check for Assert.That IsEqualTo constraint
            if (expectedOperation is null &&
                AssertHelper.TryGetActualAndConstraintOperations(assertOperation, out _, out var constraintExpression))
            {
                expectedOperation = constraintExpression.ConstraintParts
                    .Select(part => part.GetExpectedArgument())
                    .Where(e => e is not null)
                    .FirstOrDefault();
            }

            return expectedOperation;
        }
    }
}
