using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.SameAsIncompatibleTypes
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SameAsIncompatibleTypesAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.SameAsIncompatibleTypes,
            title: SameAsIncompatibleTypesConstants.Title,
            messageFormat: SameAsIncompatibleTypesConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: SameAsIncompatibleTypesConstants.Description);

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

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if (constraintPart.GetConstraintName() != NunitFrameworkConstants.NameOfIsSameAs
                    || constraintPart.GetConstraintTypeSymbol()?.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfSameAsConstraint)
                {
                    continue;
                }

                if (constraintPart.GetPrefixesNames().Any(p => p != NunitFrameworkConstants.NameOfIsNot))
                    return;

                var actualType = AssertHelper.GetUnwrappedActualType(actualExpression, semanticModel, cancellationToken);

                if (actualType == null)
                    continue;

                var expectedArgumentExpression = constraintPart.GetExpectedArgumentExpression();

                if (expectedArgumentExpression == null)
                    continue;

                var expectedType = semanticModel.GetTypeInfo(expectedArgumentExpression, cancellationToken).Type;

                if (expectedType != null && !CanBeSameType(actualType, expectedType, semanticModel))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        expectedArgumentExpression.GetLocation()));
                }
            }
        }

        private static bool CanBeSameType(ITypeSymbol actualType, ITypeSymbol expectedType, SemanticModel semanticModel)
        {
            var conversion = semanticModel.Compilation.ClassifyConversion(actualType, expectedType);
            return conversion.IsIdentity || conversion.IsReference;
        }
    }
}
