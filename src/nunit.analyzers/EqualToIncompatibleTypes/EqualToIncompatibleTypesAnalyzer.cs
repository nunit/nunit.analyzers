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
            defaultSeverity: DiagnosticSeverity.Warning,
            description: EqualToIncompatibleTypesConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPartExpression in constraintExpression.ConstraintParts)
            {
                if (HasIncompatiblePrefixes(constraintPartExpression)
                    || HasCustomEqualityComparer(constraintPartExpression)
                    || constraintPartExpression.HasUnknownExpressions())
                {
                    return;
                }

                var constraintMethod = constraintPartExpression.GetConstraintMethod();

                if (constraintMethod?.Name != NunitFrameworkConstants.NameOfIsEqualTo
                    || constraintMethod.ReturnType?.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfEqualToConstraint)
                {
                    continue;
                }

                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                var expectedArgumentExpression = constraintPartExpression.GetExpectedArgument();
                var expectedType = expectedArgumentExpression?.Type;

                if (actualType == null || expectedArgumentExpression == null || expectedType == null)
                    continue;

                if (!NUnitEqualityComparerHelper.CanBeEqual(actualType, expectedType, context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        expectedArgumentExpression.Syntax.GetLocation()));
                }
            }
        }

        private static bool HasIncompatiblePrefixes(ConstraintExpressionPart constraintPartExpression)
        {
            // Currently only 'Not' suffix supported, as all other suffixes change actual type for constraint
            // (e.g. All, Some, Property, Count, etc.)

            return constraintPartExpression.GetPrefixesNames().Any(s => s != NunitFrameworkConstants.NameOfIsNot);
        }

        private static bool HasCustomEqualityComparer(ConstraintExpressionPart constraintPartExpression)
        {
            return constraintPartExpression.GetSuffixesNames().Any(s => s == NunitFrameworkConstants.NameOfUsing);
        }
    }
}
