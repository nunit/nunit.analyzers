using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Syntax;

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

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            var cancellationToken = context.CancellationToken;
            var semanticModel = context.SemanticModel;

            if (!AssertHelper.TryGetActualAndConstraintExpressions(assertExpression, semanticModel,
                out var actualExpression, out var constraintExpression))
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

                var expectedArgumentExpression = constraintPartExpression.GetExpectedArgumentExpression();

                if (expectedArgumentExpression == null)
                    continue;

                var actualTypeInfo = semanticModel.GetTypeInfo(actualExpression, cancellationToken);
                var actualType = actualTypeInfo.Type ?? actualTypeInfo.ConvertedType;

                if (actualType == null)
                    continue;

                actualType = AssertHelper.UnwrapActualType(actualType);

                var expectedType = semanticModel.GetTypeInfo(expectedArgumentExpression, cancellationToken).Type;

                if (!NUnitEqualityComparerHelper.CanBeEqual(actualType, expectedType, semanticModel.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        expectedArgumentExpression.GetLocation()));
                }
            }
        }

        private static bool HasIncompatiblePrefixes(ConstraintPartExpression constraintPartExpression)
        {
            // Currently only 'Not' suffix supported, as all other suffixes change actual type for constraint
            // (e.g. All, Some, Property, Count, etc.)

            return constraintPartExpression.GetPrefixesNames().Any(s => s != NunitFrameworkConstants.NameOfIsNot);
        }

        private static bool HasCustomEqualityComparer(ConstraintPartExpression constraintPartExpression)
        {
            return constraintPartExpression.GetSuffixesNames().Any(s => s == NunitFrameworkConstants.NameOfUsing);
        }
    }
}
