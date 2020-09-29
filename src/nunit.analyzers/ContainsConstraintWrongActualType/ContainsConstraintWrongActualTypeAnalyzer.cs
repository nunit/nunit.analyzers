using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.ContainsConstraintWrongActualType
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ContainsConstraintWrongActualTypeAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ContainsConstraintWrongActualType,
            title: ContainsConstraintWrongActualTypeConstants.Title,
            messageFormat: ContainsConstraintWrongActualTypeConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ContainsConstraintWrongActualTypeConstants.Description);

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
                if (constraintPart.GetPrefixesNames().Any(p => p != NunitFrameworkConstants.NameOfIsNot))
                    return;

                if (constraintPart.GetConstraintName() != NunitFrameworkConstants.NameOfDoesContain
                    || constraintPart.Root?.Type.GetFullMetadataName() != NunitFrameworkConstants.FullNameOfContainsConstraint)
                {
                    continue;
                }

                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                // Valid if actualType is String
                if (actualType == null
                    || actualType.TypeKind == TypeKind.Error
                    || actualType.TypeKind == TypeKind.Dynamic
                    || actualType.SpecialType == SpecialType.System_String)
                {
                    continue;
                }

                // Valid if actualType is collection of Strings
                if (actualType.IsIEnumerable(out var elementType)
                     && (elementType == null || elementType.SpecialType == SpecialType.System_String))
                {
                    continue;
                }

                var actualTypeDisplay = actualType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    constraintPart.GetLocation(),
                    actualTypeDisplay));
            }
        }
    }
}
