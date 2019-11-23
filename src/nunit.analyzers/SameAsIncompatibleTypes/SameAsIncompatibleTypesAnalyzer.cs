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

            if (!AssertExpressionHelper.TryGetActualAndConstraintExpressions(assertExpression,
                out var actualExpression, out var constraintExpression))
            {
                return;
            }

            var sameAsExpectedExpressions = AssertExpressionHelper
                .GetExpectedArguments(constraintExpression, semanticModel, cancellationToken)
                .Where(ex => ex.constraintMethod.Name == NunitFrameworkConstants.NameOfIsSameAs
                    && ex.constraintMethod.ReturnType.GetFullMetadataName() == NunitFrameworkConstants.FullNameOfSameAsConstraint)
                .Select(ex => ex.expectedArgument)
                .ToArray();

            if (sameAsExpectedExpressions.Length == 0)
                return;

            var actualTypeInfo = semanticModel.GetTypeInfo(actualExpression, cancellationToken);
            var actualType = actualTypeInfo.Type ?? actualTypeInfo.ConvertedType;
            actualType = UnwrapActualType(actualType);

            if (actualType == null)
                return;

            foreach (var expectedArgumentExpression in sameAsExpectedExpressions)
            {
                var expectedType = semanticModel.GetTypeInfo(expectedArgumentExpression, cancellationToken).Type;

                if (expectedType != null && !CanBeSameType(actualType, expectedType, semanticModel))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        expectedArgumentExpression.GetLocation()));
                }
            }
        }

        private static ITypeSymbol UnwrapActualType(ITypeSymbol actualType)
        {
            if (actualType is INamedTypeSymbol namedType && namedType.DelegateInvokeMethod != null)
                actualType = namedType.DelegateInvokeMethod.ReturnType;

            if (actualType.IsAwaitable(out var awaitReturnType))
                actualType = awaitReturnType;

            return actualType;
        }

        private static bool CanBeSameType(ITypeSymbol actualType, ITypeSymbol expectedType, SemanticModel semanticModel)
        {
            var conversion = semanticModel.Compilation.ClassifyConversion(actualType, expectedType);
            return conversion.IsIdentity || conversion.IsReference;
        }
    }
}
