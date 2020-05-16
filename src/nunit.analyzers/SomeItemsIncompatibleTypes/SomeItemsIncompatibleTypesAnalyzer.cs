using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Syntax;

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
            defaultSeverity: DiagnosticSeverity.Warning,
            description: SomeItemsIncompatibleTypesConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            var cancellationToken = context.CancellationToken;
            var semanticModel = context.SemanticModel;

            if (!AssertHelper.TryGetActualAndConstraintExpressions(assertExpression, semanticModel,
                out var actualExpression, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if ((!IsDoesContain(constraintPart) && !IsContainsItem(constraintPart))
                    || constraintPart.GetConstraintTypeSymbol()?.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfSomeItemsConstraint)
                {
                    continue;
                }

                if (HasIncompatibleOperators(constraintPart))
                    return;

                var expectedExpression = constraintPart.GetExpectedArgumentExpression();

                if (expectedExpression == null)
                    continue;

                var expectedType = semanticModel.GetTypeInfo(expectedExpression, cancellationToken).Type;
                var actualType = AssertHelper.GetUnwrappedActualType(actualExpression, semanticModel, cancellationToken);

                if (actualType == null || expectedType == null)
                    continue;

                if (actualType.IsIEnumerable(out var elementType))
                {
                    // Cannot determine element type for non-generic IEnumerable, therefore consider valid.
                    if (elementType == null)
                        continue;

                    // Valid, if collection element type matches expected type.
                    if (NUnitEqualityComparerHelper.CanBeEqual(elementType, expectedType, semanticModel.Compilation))
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

        private static bool IsDoesContain(ConstraintPartExpression constraintPart)
        {
            return constraintPart.GetConstraintName() == NunitFrameworkConstants.NameOfDoesContain;
        }

        private static bool IsContainsItem(ConstraintPartExpression constraintPart)
        {
            return constraintPart.GetHelperClassName() == NunitFrameworkConstants.NameOfContains
                && constraintPart.GetConstraintName() == NunitFrameworkConstants.NameOfContainsItem;
        }

        private static bool HasIncompatibleOperators(ConstraintPartExpression constraintPart)
        {
            return constraintPart.GetPrefixesNames().Any(p => p != NunitFrameworkConstants.NameOfDoesNot);
        }

        private static string ConstraintDiagnosticDescription(ConstraintPartExpression constraintPart)
        {
            return constraintPart.GetHelperClassName() != null
                ? $"{constraintPart.GetHelperClassName()}.{constraintPart.GetConstraintName()}"
                : (constraintPart.GetConstraintName() ?? "");
        }
    }
}
