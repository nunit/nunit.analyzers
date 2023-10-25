using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.NullConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NullConstraintUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.NullConstraintUsage,
            title: NullConstraintUsageAnalyzerConstants.Title,
            messageFormat: NullConstraintUsageAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: NullConstraintUsageAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if (!constraintPart.HasIncompatiblePrefixes()
                    && constraintPart.Root is not null
                    && constraintPart.HelperClass?.Name == NUnitFrameworkConstants.NameOfIs
                    && constraintPart.GetConstraintName() == NUnitFrameworkConstants.NameOfIsNull)
                {
                    var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                    if (actualType is null)
                        return;

                    if (actualType.IsValueType && actualType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                    {
                        var typeDisplay = actualType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                        context.ReportDiagnostic(Diagnostic.Create(
                            descriptor,
                            constraintPart.Root.Syntax.GetLocation(),
                            typeDisplay));
                    }
                }
            }
        }
    }
}
