using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.SameAsOnValueTypes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SameAsOnValueTypesAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.SameAsOnValueTypes,
            title: SameAsOnValueTypesConstants.Title,
            messageFormat: SameAsOnValueTypesConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: SameAsOnValueTypesConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            IOperation? actualOperation;
            IOperation? expectedOperation;

            if (assertOperation.TargetMethod.Name.Equals(NUnitLegacyFrameworkConstants.NameOfAssertAreSame, StringComparison.Ordinal) ||
                assertOperation.TargetMethod.Name.Equals(NUnitLegacyFrameworkConstants.NameOfAssertAreNotSame, StringComparison.Ordinal))
            {
                actualOperation = assertOperation.GetArgumentOperation(NUnitFrameworkConstants.NameOfActualParameter);
                expectedOperation = assertOperation.GetArgumentOperation(NUnitFrameworkConstants.NameOfExpectedParameter);

                CheckActualVsExpectedOperation(context, actualOperation, expectedOperation);
            }
            else
            {
                if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                    out actualOperation, out var constraintExpression))
                {
                    return;
                }

                foreach (var constraintPartExpression in constraintExpression.ConstraintParts)
                {
                    if (constraintPartExpression.HasIncompatiblePrefixes()
                        || constraintPartExpression.HasUnknownExpressions())
                    {
                        return;
                    }

                    var constraintMethod = constraintPartExpression.GetConstraintMethod();

                    if (constraintMethod?.Name != NUnitFrameworkConstants.NameOfIsSameAs
                        || constraintMethod.ReturnType?.GetFullMetadataName() != NUnitFrameworkConstants.FullNameOfSameAsConstraint)
                    {
                        continue;
                    }

                    expectedOperation = constraintPartExpression.GetExpectedArgument();

                    CheckActualVsExpectedOperation(context, actualOperation, expectedOperation);
                }
            }
        }

        private static void CheckActualVsExpectedOperation(OperationAnalysisContext context, IOperation? actualOperation, IOperation? expectedOperation)
        {
            if (actualOperation is null || expectedOperation is null)
                return;

            var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);
            var expectedType = expectedOperation.Type;

            if (actualType is null || expectedType is null)
                return;

            if (expectedType.IsValueType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    expectedOperation.Syntax.GetLocation()));
            }
            else if (actualType.IsValueType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    actualOperation.Syntax.GetLocation()));
            }
        }
    }
}
