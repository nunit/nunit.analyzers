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
        private const string IsNull = $"{NameOfIs}.{NameOfIsNull}";
        private const string IsNotNotNull = $"{NameOfIs}.{NameOfIsNot}.{NameOfIsNull}";

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
            bool isNull;

            // 'actual == expected', 'Equals(actual, expected)' or 'actual.Equals(expected)'
            if (IsBinaryOperationNotUsingRefStructOperands(actual, BinaryOperatorKind.Equals, out isNull)
                || IsStaticObjectEquals(actual, out isNull)
                || IsInstanceObjectEquals(actual, out isNull))
            {
                shouldReport = true;
            }

            // 'actual is null'
            else if (IsPatternOperationWithConstantNull(actual))
            {
                shouldReport = true;
                isNull = true;
            }

            // 'actual != expected'
            else if (IsBinaryOperationNotUsingRefStructOperands(actual, BinaryOperatorKind.NotEquals, out isNull))
            {
                shouldReport = true;
                negated = !negated;
            }

            // 'actual is not null'
            else if (IsNegatedPatternOperationWithConstantNull(actual))
            {
                shouldReport = true;
                isNull = true;
                negated = !negated;
            }

            if (shouldReport)
            {
                var suggestedConstraint = isNull
                    ? negated ? IsNotNotNull : IsNull
                    : negated ? IsNotEqualTo : IsEqualTo;
                return (descriptor, suggestedConstraint);
            }

            return (null, null);
        }

        private static bool IsStaticObjectEquals(IOperation operation, out bool atLeastOneOperandIsNull)
        {
            atLeastOneOperandIsNull = false;

            if (operation is not IInvocationOperation invocation)
                return false;

            var methodSymbol = invocation.TargetMethod;

            var result = methodSymbol is not null
                && methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 2
                && methodSymbol.Name == nameof(object.Equals)
                && methodSymbol.ContainingType.SpecialType == SpecialType.System_Object;

            if (!result)
            {
                return false;
            }

            atLeastOneOperandIsNull =
                IsOperationWithNullConstant(invocation.Arguments[0].Value)
                || IsOperationWithNullConstant(invocation.Arguments[1].Value);

            return true;
        }

        private static bool IsInstanceObjectEquals(IOperation operation, out bool isArgumentNull)
        {
            isArgumentNull = false;

            if (operation is not IInvocationOperation invocation)
                return false;

            var methodSymbol = invocation.TargetMethod;

            var result = methodSymbol is not null
                && !methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.Name == nameof(object.Equals)
                && invocation.Arguments.Length == 1
                && !IsRefStruct(invocation.Arguments[0])
                && invocation.Instance is not null
                && !IsRefStruct(invocation.Instance);

            if (!result)
            {
                return false;
            }

            isArgumentNull = IsOperationWithNullConstant(invocation.Arguments[0].Value);
            return true;
        }
    }
}
