using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.UseCollectionConstraint
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseCollectionConstraintAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.UsePropertyConstraint,
            title: UseCollectionConstraintConstants.Title,
            messageFormat: UseCollectionConstraintConstants.MessageFormat,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: UseCollectionConstraintConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            // Check if actual operation is a member access operation for either .Length or .Count
            if (actualOperation is IMemberReferenceOperation referenceOperation &&
                IsInteger(referenceOperation.Type) &&
                referenceOperation.Instance is IOperation instance &&
                (referenceOperation.Member.Name is NUnitFrameworkConstants.NameOfHasLength ||
                referenceOperation.Member.Name is NUnitFrameworkConstants.NameOfHasCount))
            {
                // constraint operation must be Is.
                foreach (var constraintPart in constraintExpression.ConstraintParts)
                {
                    if (!constraintPart.HasIncompatiblePrefixes()
                        && constraintPart.Root is not null
                        && constraintPart.HelperClass?.Name == NUnitFrameworkConstants.NameOfIs)
                    {
                        // Only raise to use Has.Property if the type at hand is IEnumerable.
                        INamedTypeSymbol enumerable = context.Compilation.GetTypeByMetadataName("System.Collections.IEnumerable")!;

                        if (instance.Type?.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, enumerable)) == true)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                descriptor,
                                referenceOperation.Syntax.GetLocation(),
                                referenceOperation.Member.Name));
                        }
                    }
                }
            }

            static bool IsInteger(ITypeSymbol? type)
            {
                if (type is null)
                {
                    return false;
                }

                SpecialType specialType = type.SpecialType;
                return specialType is >= SpecialType.System_Byte and <= SpecialType.System_UInt64;
            }
        }
    }
}
