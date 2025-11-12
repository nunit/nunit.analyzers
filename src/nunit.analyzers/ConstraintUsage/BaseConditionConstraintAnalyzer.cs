using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;
using static NUnit.Analyzers.Constants.NUnitLegacyFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    public abstract class BaseConditionConstraintAnalyzer : BaseAssertionAnalyzer
    {
        internal const string SuggestedConstraintString = nameof(SuggestedConstraintString);
        internal const string SwapOperands = nameof(SwapOperands);

        protected static readonly string[] SupportedPositiveAssertMethods =
        [
            NameOfAssertThat,
            NameOfAssertTrue,
            NameOfAssertIsTrue
        ];

        protected static readonly string[] SupportedNegativeAssertMethods =
        [
            NameOfAssertFalse,
            NameOfAssertIsFalse
        ];

        protected static bool IsRefStruct(IOperation operation)
        {
            return operation.Type?.TypeKind == TypeKind.Struct && operation.Type.IsRefLikeType;
        }

        protected static bool IsBinaryOperationNotUsingRefStructOperands(IOperation actual, BinaryOperatorKind binaryOperator)
        {
            return actual is IBinaryOperation binaryOperation &&
                binaryOperation.OperatorKind == binaryOperator &&
                !IsRefStruct(binaryOperation.LeftOperand) &&
                !IsRefStruct(binaryOperation.RightOperand);
        }

        protected virtual (DiagnosticDescriptor? descriptor, string? suggestedConstraint, bool swapOperands) GetDiagnosticDataWithPossibleSwapOperands(
            OperationAnalysisContext context, IOperation actual, bool negated)
        {
            var (diagnosticDescriptor, suggestedConstraint) = this.GetDiagnosticData(context, actual, negated);
            return (diagnosticDescriptor, suggestedConstraint, false);
        }

        protected virtual (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData(
            OperationAnalysisContext context, IOperation actual, bool negated)
        {
            throw new InvalidOperationException("You must override one of the 'GetDiagnosticData' versions");
        }

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            var negated = false;

            if (SupportedNegativeAssertMethods.Contains(assertOperation.TargetMethod.Name))
            {
                negated = true;
            }
            else if (!SupportedPositiveAssertMethods.Contains(assertOperation.TargetMethod.Name))
            {
                return;
            }

            if (AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                bool? isTrue = constraintExpression.IsTrueOrFalse();

                if (isTrue is null)
                    return; // Neither true nor false, cannot analyze.

                if (isTrue is false)
                    negated = !negated;
            }
            else
            {
                // Classic Assert methods like Assert.IsTrue(condition)
                actualOperation = assertOperation.GetArgumentOperation(NameOfConditionParameter);
            }

            if (actualOperation is null)
                return;

            if (IsPrefixNotOperation(actualOperation, out var unwrappedActual))
            {
                negated = !negated;
            }

            var (descriptor, suggestedConstraint, swapOperands) = this.GetDiagnosticDataWithPossibleSwapOperands(context, unwrappedActual, negated);

            if (descriptor is not null && suggestedConstraint is not null)
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add(SuggestedConstraintString, suggestedConstraint);
                properties.Add(SwapOperands, swapOperands.ToString());
                var diagnostic = Diagnostic.Create(descriptor, actualOperation.Syntax.GetLocation(), properties.ToImmutable(), suggestedConstraint);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsPrefixNotOperation(IOperation operation, out IOperation operand)
        {
            if (operation is IUnaryOperation unaryOperation
                && unaryOperation.OperatorKind == UnaryOperatorKind.Not)
            {
                operand = unaryOperation.Operand;
                return true;
            }
            else
            {
                operand = operation;
                return false;
            }
        }
    }
}
