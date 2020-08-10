using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
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


        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintOperation))
            {
                return;
            }

            foreach (var constraintPart in constraintOperation.ConstraintParts)
            {
                if (constraintPart.GetConstraintName() != NunitFrameworkConstants.NameOfIsSameAs
                    || constraintPart.Root?.Type.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfSameAsConstraint)
                {
                    continue;
                }

                if (constraintPart.GetPrefixesNames().Any(p => p != NunitFrameworkConstants.NameOfIsNot))
                    return;

                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                if (actualType == null)
                    continue;

                var expectedArgumentOperation = constraintPart.GetExpectedArgument();
                var expectedType = expectedArgumentOperation?.Type;

                if (expectedType != null && !CanBeSameType(actualType, expectedType, context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        expectedArgumentOperation!.Syntax.GetLocation()));
                }
            }
        }

        private static bool CanBeSameType(ITypeSymbol actualType, ITypeSymbol expectedType, Compilation compilation)
        {
            var conversion = compilation.ClassifyConversion(actualType, expectedType);
            return conversion.IsIdentity || conversion.IsReference;
        }
    }
}
