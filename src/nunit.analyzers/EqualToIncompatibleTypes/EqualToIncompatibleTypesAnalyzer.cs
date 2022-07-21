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

                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                foreach (var constraintPartExpression in constraintExpression.ConstraintParts)
                {
                    if (constraintPartExpression.HasIncompatiblePrefixes()
                        || HasCustomEqualityComparer(constraintPartExpression)
                        || constraintPartExpression.HasUnknownExpressions())
                    {
                        return;
                    }

                    // Check for Assert.That(..., Throws)
                    // Here the actualType has nothing to do with the delegate.
                    if (constraintPartExpression.HelperClass?.GetFullMetadataName() == NUnitFrameworkConstants.FullNameOfThrows)
                    {
                        if (constraintPartExpression.Root is IInvocationOperation invocationOperation)
                        {
                            if (invocationOperation.Arguments.Length == 1)
                            {
                                var argument = invocationOperation.Arguments[0].Value;
                                if (argument is ITypeOfOperation typeOfOperation)
                                {
                                    actualType = typeOfOperation.TypeOperand;
                                }
                                else
                                {
                                    // The actualType is the result of a runtime operation not know at analyzer time.
                                    // But at least it must be an exception.
                                    actualType = context.Compilation.GetTypeByMetadataName("System.Exception");
                                }
                            }
                            else if (invocationOperation.TargetMethod.TypeArguments.Length == 1)
                            {
                                actualType = invocationOperation.TargetMethod.TypeArguments[0];
                            }
                        }
                        else if (constraintPartExpression.Root is IPropertyReferenceOperation propertyReferenceOperation)
                        {
                            string typeName = propertyReferenceOperation.Property.Name switch
                            {
                                NUnitFrameworkConstants.NameOfThrowsArgumentException => "System.ArgumentException",
                                NUnitFrameworkConstants.NameOfThrowsArgumentNullException => "System.ArgumentNullException",
                                NUnitFrameworkConstants.NameOfThrowsInvalidOperationException => "System.InvalidOperationException",
                                NUnitFrameworkConstants.NameOfThrowsTargetInvocationException => "System.Reflection.TargetInvocationException",
                                _ => "System.Exception",
                            };

                            actualType = context.Compilation.GetTypeByMetadataName(typeName);
                        }

                        continue;
                    }

                    var constraintMethod = constraintPartExpression.GetConstraintMethod();

                    if (constraintMethod?.Name != NUnitFrameworkConstants.NameOfIsEqualTo
                        || constraintMethod.ReturnType?.GetFullMetadataName() != NUnitFrameworkConstants.FullNameOfEqualToConstraint)
                    {
                        continue;
                    }

                    expectedOperation = constraintPartExpression.GetExpectedArgument();

                    if (expectedOperation is not null)
                    {
                        CheckActualVsExpectedOperation(context, actualType, expectedOperation);
                    }
                }
            }
        }

        private static void CheckActualVsExpectedOperation(OperationAnalysisContext context, IOperation? actualOperation, IOperation? expectedOperation)
        {
            if (actualOperation is null || expectedOperation is null)
                return;

            ITypeSymbol? actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

            CheckActualVsExpectedOperation(context, actualType, expectedOperation);
        }

        private static void CheckActualVsExpectedOperation(OperationAnalysisContext context, ITypeSymbol? actualType, IOperation expectedOperation)
        {
            ITypeSymbol? expectedType = expectedOperation.Type;

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
