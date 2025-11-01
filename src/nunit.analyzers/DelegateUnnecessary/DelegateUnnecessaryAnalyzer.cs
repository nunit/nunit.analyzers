using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.DelegateUnnecessary
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DelegateUnnecessaryAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor Descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.DelegateUnnecessary,
            title: DelegateUnnecessaryConstants.Title,
            messageFormat: DelegateUnnecessaryConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: DelegateUnnecessaryConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            if (actualOperation.Kind != OperationKind.DelegateCreation)
            {
                return;
            }

            Operations.ConstraintExpressionPart[] constraintParts = constraintExpression.ConstraintParts;
            if (constraintParts.Length == 0 ||
                constraintParts[0].HelperClass?.Name == NUnitFrameworkConstants.NameOfThrows ||
                constraintParts.Any(constraintPart => constraintPart.Suffixes.Any(IsDelayedConstraint)))
            {
                return;
            }

            ImmutableDictionary<string, string?>? properties = null;

            if (actualOperation.Type?.ToString()?.Contains("System.Threading.Tasks.Task") is true)
            {
                var builder = ImmutableDictionary.CreateBuilder<string, string?>();
                builder.Add(AnalyzerPropertyKeys.IsAsync, null);
                properties = builder.ToImmutable();
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                actualOperation.Syntax.GetLocation(),
                properties,
                actualOperation.ToString()));
        }

        private static bool IsDelayedConstraint(IOperation operation)
        {
            return operation.Type is INamedTypeSymbol namedType &&
                namedType.GetFullMetadataName().StartsWith(NUnitFrameworkConstants.FullNameOfDelayedConstraint, StringComparison.Ordinal);
        }
    }
}
