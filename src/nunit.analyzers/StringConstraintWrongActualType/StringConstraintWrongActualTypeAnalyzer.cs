using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.StringConstraintWrongActualType
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringConstraintWrongActualTypeAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.StringConstraintWrongActualType,
            title: StringConstraintWrongActualTypeConstants.Title,
            messageFormat: StringConstraintWrongActualTypeConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: StringConstraintWrongActualTypeConstants.Description);

        private static readonly HashSet<string> SupportedConstraints = new()
        {
            NUnitFrameworkConstants.FullNameOfEndsWithConstraint,
            NUnitFrameworkConstants.FullNameOfRegexConstraint,
            NUnitFrameworkConstants.FullNameOfEmptyStringConstraint,
            NUnitFrameworkConstants.FullNameOfSamePathConstraint,
            NUnitFrameworkConstants.FullNameOfSamePathOrUnderConstraint,
            NUnitFrameworkConstants.FullNameOfStartsWithConstraint,
            NUnitFrameworkConstants.FullNameOfSubPathConstraint,
            NUnitFrameworkConstants.FullNameOfSubstringConstraint
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

            // Allow 'object' and 'dynamic' to avoid lots of false positives
            if (actualType is null
                || actualType.TypeKind == TypeKind.Error
                || actualType.TypeKind == TypeKind.Dynamic
                || actualType.SpecialType == SpecialType.System_Object
                || actualType.SpecialType == SpecialType.System_String)
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                // Only 'Not' prefix supported
                if (constraintPart.HasIncompatiblePrefixes())
                    return;

                var constraintType = constraintPart.Root?.Type;

                if (constraintType is not null && SupportedConstraints.Contains(constraintType.GetFullMetadataName()))
                {
                    var actualTypeDisplay = actualType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        constraintPart.GetLocation(),
                        constraintType.Name,
                        actualTypeDisplay));
                }
            }
        }
    }
}
