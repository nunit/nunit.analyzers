using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualConstraintUsageAnalyzer : BaseConditionConstraintAnalyzer
    {
        private const string IsEqualTo = $"{NameOfIs}.{NameOfIsEqualTo}";
        private const string IsNotEqualTo = $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEqualTo}";

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.EqualConstraintUsage,
            title: EqualConstraintUsageConstants.Title,
            messageFormat: EqualConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: EqualConstraintUsageConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData(
            OperationAnalysisContext context, IOperation actual, bool negated)
        {
            var shouldReport = false;

            // 'actual == expected', 'Equals(actual, expected)' or 'actual.Equals(expected)'
            if (IsBinaryOperationNotUsingRefStructOperands(actual, BinaryOperatorKind.Equals)
                || IsStaticObjectEquals(actual)
                || IsInstanceObjectEquals(actual))
            {
                shouldReport = true;
            }

            // 'actual != expected'
            else if (IsBinaryOperationNotUsingRefStructOperands(actual, BinaryOperatorKind.NotEquals))
            {
                shouldReport = true;
                negated = !negated;
            }
            else if (actual is IIsPatternOperation isPatternOperation)
            {
                shouldReport = true;
                if (isPatternOperation.Pattern is INegatedPatternOperation)
                    negated = true;
            }

            if (shouldReport)
            {
                var suggestedConstraint = negated ? IsNotEqualTo : IsEqualTo;
                return (descriptor, suggestedConstraint);
            }

            return (null, null);
        }

        private static bool IsStaticObjectEquals(IOperation operation)
        {
            if (operation is not IInvocationOperation invocation)
                return false;

            var methodSymbol = invocation.TargetMethod;

            return methodSymbol is not null
                && methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 2
                && methodSymbol.Name == nameof(object.Equals)
                && methodSymbol.ContainingType.SpecialType == SpecialType.System_Object;
        }

        private static bool IsInstanceObjectEquals(IOperation operation)
        {
            if (operation is not IInvocationOperation invocation)
                return false;

            var methodSymbol = invocation.TargetMethod;

            return methodSymbol is not null
                && !methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.Name == nameof(object.Equals)
                && invocation.Arguments.Length == 1
                && !IsRefStruct(invocation.Arguments[0])
                && invocation.Instance is not null
                && !IsRefStruct(invocation.Instance);
        }
    }
}
