using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.TypeOfConstraint
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypeOfAnalyzer : BaseAssertionAnalyzer
    {
        internal const string IsConstraintIsTrue = nameof(IsConstraintIsTrue);

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TypeOf,
            title: TypeOfConstants.Title,
            messageFormat: TypeOfConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: TypeOfConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            if (!this.IsGetTypeInvocation(actualOperation))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if (constraintPart.HasIncompatiblePrefixes())
                    continue;

                var expectedArgument = constraintPart.GetExpectedArgument();

                if (expectedArgument is null)
                    continue;

                if (this.IsGetTypeInvocation(expectedArgument) ||
                    expectedArgument.Syntax is TypeOfExpressionSyntax)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        actualOperation.Syntax.GetLocation()));
                }
            }
        }

        private bool IsGetTypeInvocation(IOperation actualOperation)
        {
            if (actualOperation is not IInvocationOperation invocation)
                return false;

            var methodSymbol = invocation.TargetMethod;

            return methodSymbol is not null
                && !methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 0
                && methodSymbol.Name == nameof(Type.GetType);
        }
    }
}
