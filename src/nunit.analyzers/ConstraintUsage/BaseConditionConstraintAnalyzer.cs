using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Extensions;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    public abstract class BaseConditionConstraintAnalyzer : BaseAssertionAnalyzer
    {
        internal const string SuggestedConstraintString = nameof(SuggestedConstraintString);

        protected static readonly string[] SupportedPositiveAssertMethods = new[]
        {
            NameOfAssertThat,
            NameOfAssertTrue,
            NameOfAssertIsTrue
        };

        protected static readonly string[] SupportedNegativeAssertMethods = new[]
        {
            NameOfAssertFalse,
            NameOfAssertIsFalse
        };

        protected abstract (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData(
            OperationAnalysisContext context, IOperation actual, bool negated);

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

            var constraintExpression = assertOperation.GetArgumentOperation(NameOfExpressionParameter);

            // Constraint should be either absent, or Is.True, or Is.False
            if (constraintExpression != null)
            {
                if (!(constraintExpression is IPropertyReferenceOperation propertyReference
                    && propertyReference.Property.ContainingType.Name == NameOfIs
                    && (propertyReference.Property.Name == NameOfIsTrue
                        || propertyReference.Property.Name == NameOfIsFalse)))
                {
                    return;
                }

                if (propertyReference.Property.Name == NameOfIsFalse)
                {
                    negated = !negated;
                }
            }

            var actual = assertOperation.GetArgumentOperation(NameOfActualParameter)
                ?? assertOperation.GetArgumentOperation(NameOfConditionParameter);

            if (actual == null)
                return;

            if (IsPrefixNotOperation(actual, out var unwrappedActual))
            {
                negated = !negated;
            }

            var (descriptor, suggestedConstraint) = this.GetDiagnosticData(context, unwrappedActual ?? actual, negated);

            if (descriptor != null && suggestedConstraint != null)
            {
                var properties = ImmutableDictionary.Create<string, string?>().Add(SuggestedConstraintString, suggestedConstraint);
                var diagnostic = Diagnostic.Create(descriptor, actual.Syntax.GetLocation(), properties, suggestedConstraint);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsPrefixNotOperation(IOperation operation, [NotNullWhen(true)] out IOperation? operand)
        {
            if (operation is IUnaryOperation unaryOperation
                && unaryOperation.OperatorKind == UnaryOperatorKind.Not)
            {
                operand = unaryOperation.Operand;
                return true;
            }
            else
            {
                operand = null;
                return false;
            }
        }
    }
}
