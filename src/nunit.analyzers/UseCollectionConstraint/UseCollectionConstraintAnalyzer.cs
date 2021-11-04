using System.Collections.Immutable;
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
                        context.ReportDiagnostic(Diagnostic.Create(
                            descriptor,
                            referenceOperation.Syntax.GetLocation(),
                            referenceOperation.Member.Name));
                    }
                }
            }
        }
    }
}
