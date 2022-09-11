using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComparisonConstraintUsageAnalyzer : BaseConditionConstraintAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ComparisonConstraintUsage,
            title: ComparisonConstraintUsageConstants.Title,
            messageFormat: ComparisonConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ComparisonConstraintUsageConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override (DiagnosticDescriptor? descriptor, string? suggestedConstraint, bool swapOperands) GetDiagnosticDataWithPossibleSwapOperands(
            OperationAnalysisContext context, IOperation actual, bool negated)
        {
            // 'actual >= expected', 'actual > expected'
            // 'actual <= expected', 'actual < expected'
            if (actual is IBinaryOperation binaryOperation &&
                !IsRefStruct(binaryOperation.LeftOperand) && !IsRefStruct(binaryOperation.RightOperand))
            {
                bool swapOperands = false;

                if (AssertHelper.IsLiteralOperation(binaryOperation.LeftOperand) &&
                    !AssertHelper.IsLiteralOperation(binaryOperation.RightOperand))
                {
                    swapOperands = true;
                    negated = !negated;
                }

                string? suggestedConstraint = binaryOperation.OperatorKind switch
                {
                    BinaryOperatorKind.GreaterThanOrEqual =>
                        negated ? $"{NameOfIs}.{NameOfIsLessThan}" : $"{NameOfIs}.{NameOfIsGreaterThanOrEqualTo}",
                    BinaryOperatorKind.GreaterThan =>
                        negated ? $"{NameOfIs}.{NameOfIsLessThanOrEqualTo}" : $"{NameOfIs}.{NameOfIsGreaterThan}",
                    BinaryOperatorKind.LessThanOrEqual =>
                        negated ? $"{NameOfIs}.{NameOfIsGreaterThan}" : $"{NameOfIs}.{NameOfIsLessThanOrEqualTo}",
                    BinaryOperatorKind.LessThan =>
                        negated ? $"{NameOfIs}.{NameOfIsGreaterThanOrEqualTo}" : $"{NameOfIs}.{NameOfIsLessThan}",
                    _ => null,
                };

                return (descriptor, suggestedConstraint, swapOperands);
            }

            return (null, null, false);
        }
    }
}
