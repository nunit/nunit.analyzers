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

            if (assertOperation.TargetMethod.Name.Equals(NunitFrameworkConstants.NameOfAssertAreSame, StringComparison.Ordinal) ||
                assertOperation.TargetMethod.Name.Equals(NunitFrameworkConstants.NameOfAssertAreNotSame, StringComparison.Ordinal))
            {
                actualOperation = assertOperation.GetArgumentOperation(NunitFrameworkConstants.NameOfActualParameter);
                expectedOperation = assertOperation.GetArgumentOperation(NunitFrameworkConstants.NameOfExpectedParameter);

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
                    if (HasIncompatiblePrefixes(constraintPartExpression)
                        || constraintPartExpression.HasUnknownExpressions())
                    {
                        return;
                    }

                    var constraintMethod = constraintPartExpression.GetConstraintMethod();

                    if (constraintMethod?.Name != NunitFrameworkConstants.NameOfIsSameAs
                        || constraintMethod.ReturnType?.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfSameAsConstraint)
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
            if (actualOperation == null || expectedOperation == null)
                return;

            var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);
            var expectedType = expectedOperation.Type;

            if (actualType == null || expectedType == null)
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

        private static bool HasIncompatiblePrefixes(ConstraintExpressionPart constraintPartExpression)
        {
            // Currently only 'Not' suffix supported, as all other suffixes change actual type for constraint
            // (e.g. All, Some, Property, Count, etc.)

            return constraintPartExpression.GetPrefixesNames().Any(s => s != NunitFrameworkConstants.NameOfIsNot);
        }
    }
}
