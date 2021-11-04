using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.EqualToIncompatibleTypes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualToIncompatibleTypesAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.EqualToIncompatibleTypes,
            title: EqualToIncompatibleTypesConstants.Title,
            messageFormat: EqualToIncompatibleTypesConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: EqualToIncompatibleTypesConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            IOperation? actualOperation;
            IOperation? expectedOperation;

            if (assertOperation.TargetMethod.Name.Equals(NUnitFrameworkConstants.NameOfAssertAreEqual, StringComparison.Ordinal) ||
                assertOperation.TargetMethod.Name.Equals(NUnitFrameworkConstants.NameOfAssertAreNotEqual, StringComparison.Ordinal))
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
                        || HasCustomEqualityComparer(constraintPartExpression)
                        || constraintPartExpression.HasUnknownExpressions())
                    {
                        return;
                    }

                    var constraintMethod = constraintPartExpression.GetConstraintMethod();

                    if (constraintMethod?.Name != NUnitFrameworkConstants.NameOfIsEqualTo
                        || constraintMethod.ReturnType?.GetFullMetadataName() != NUnitFrameworkConstants.FullNameOfEqualToConstraint)
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

            if (!NUnitEqualityComparerHelper.CanBeEqual(actualType, expectedType, context.Compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    expectedOperation.Syntax.GetLocation()));
            }
        }

        private static bool HasCustomEqualityComparer(ConstraintExpressionPart constraintPartExpression)
        {
            return constraintPartExpression.GetSuffixesNames().Any(s => s == NUnitFrameworkConstants.NameOfUsing);
        }
    }
}
