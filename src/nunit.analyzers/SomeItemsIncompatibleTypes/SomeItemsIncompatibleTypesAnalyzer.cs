using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.SomeItemsIncompatibleTypes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SomeItemsIncompatibleTypesAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.SomeItemsIncompatibleTypes,
            title: SomeItemsIncompatibleTypesConstants.Title,
            messageFormat: SomeItemsIncompatibleTypesConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: SomeItemsIncompatibleTypesConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if ((!IsDoesContain(constraintPart) && !IsContainsItem(constraintPart))
                    || constraintPart.Root?.Type.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfSomeItemsConstraint)
                {
                    continue;
                }

                if (constraintPart.HasIncompatiblePrefixes())
                    return;

                var expectedType = constraintPart.GetExpectedArgument()?.Type;
                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                if (actualType == null || expectedType == null)
                    continue;

                if (actualType.IsIEnumerable(out var elementType))
                {
                    // Cannot determine element type for non-generic IEnumerable, therefore consider valid.
                    if (elementType == null)
                        continue;

                    // Valid, if collection element type matches expected type.
                    if (NUnitEqualityComparerHelper.CanBeEqual(elementType, expectedType, context.Compilation))
                        continue;
                }

                var actualTypeDisplay = actualType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var expectedTypeDisplay = expectedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    constraintPart.GetLocation(),
                    ConstraintDiagnosticDescription(constraintPart),
                    actualTypeDisplay,
                    expectedTypeDisplay));
            }
        }

        private static bool IsDoesContain(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.GetConstraintName() == NunitFrameworkConstants.NameOfDoesContain;
        }

        private static bool IsContainsItem(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.HelperClass?.Name == NunitFrameworkConstants.NameOfContains
                && constraintPart.GetConstraintName() == NunitFrameworkConstants.NameOfContainsItem;
        }

        private static string ConstraintDiagnosticDescription(ConstraintExpressionPart constraintPart)
        {
            return constraintPart.HelperClass?.Name != null
                ? $"{constraintPart.HelperClass?.Name}.{constraintPart.GetConstraintName()}"
                : (constraintPart.GetConstraintName() ?? "");
        }
    }
}
