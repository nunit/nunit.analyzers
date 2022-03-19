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
                // https://github.com/nunit/nunit.analyzers/issues/431
                // There are too many exceptions for the DelayedConstraint
                // Items like Has.Count, Is.Empty, Does.Contain etc will re-evaluate when called again.
                // For now only raise when the value is a value type.
                if (constraintPart.HelperClass?.Name == NUnitFrameworkConstants.NameOfThrows ||
                    (constraintPart.Suffixes.Any(IsDelayedConstraint) && actualOperation.Type.TypeKind != TypeKind.Class))
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
