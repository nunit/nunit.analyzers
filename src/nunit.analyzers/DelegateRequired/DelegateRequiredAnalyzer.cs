using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.DelegateRequired
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DelegateRequiredAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.DelegateRequired,
            title: DelegateRequiredConstants.Title,
            messageFormat: DelegateRequiredConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: DelegateRequiredConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            if (actualOperation.Type.TypeKind == TypeKind.Delegate)
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if (constraintPart.HelperClass?.Name == NUnitFrameworkConstants.NameOfThrows ||
                    constraintPart.Suffixes.Any(IsDelayedConstraint))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptor,
                        actualOperation.Syntax.GetLocation(),
                        actualOperation.ToString()));
                }
            }
        }

        private static bool IsDelayedConstraint(IOperation operation)
        {
            return operation.Type is INamedTypeSymbol namedType &&
                namedType.GetFullMetadataName().StartsWith(NUnitFrameworkConstants.FullNameOfDelayedConstraint, StringComparison.Ordinal);
        }
    }
}
