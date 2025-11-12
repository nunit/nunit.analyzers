using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.MisusedConstraints
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MisusedConstraintsAnalyzer : BaseAssertionAnalyzer
    {
        private const string IsNotNull = $"{NUnitFrameworkConstants.NameOfIs}.{NUnitFrameworkConstants.NameOfIsNot}.{NUnitFrameworkConstants.NameOfIsNull}";
        private const string IsNotNullOrEmpty = $"{NUnitFrameworkConstants.NameOfIs}.{NUnitFrameworkConstants.NameOfIsNot}.{NUnitFrameworkConstants.NameOfIsNull}.{NUnitFrameworkConstants.NameOfConstraintExpressionOr}.{NUnitFrameworkConstants.NameOfIsEmpty}";
        private const string IsNotNullAndNotEmpty = $"{NUnitFrameworkConstants.NameOfIs}.{NUnitFrameworkConstants.NameOfIsNot}.{NUnitFrameworkConstants.NameOfIsNull}.{NUnitFrameworkConstants.NameOfConstraintExpressionAnd}.{NUnitFrameworkConstants.NameOfIsNot}.{NUnitFrameworkConstants.NameOfIsEmpty}";

        private static readonly string[] IsNotNullOrEmptyParts =
        [
            NUnitFrameworkConstants.NameOfIs,
            NUnitFrameworkConstants.NameOfIsNot,
            NUnitFrameworkConstants.NameOfIsNull,
            NUnitFrameworkConstants.NameOfConstraintExpressionOr,
            NUnitFrameworkConstants.NameOfIsEmpty,
        ];

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.MisusedConstraints,
            title: MisusedConstraintsAnalyzerConstants.Title,
            messageFormat: MisusedConstraintsAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: MisusedConstraintsAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            if (actualOperation is null || constraintExpression.Operation is null)
                return;

            // Consider the whole constraint expression, not just individual parts,
            if (IsNotNullOrEmptyOperation(constraintExpression.Operation))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        constraintExpression.Operation.Syntax.GetLocation(),
                        $"This is equivalent to {IsNotNull}, did you mean {IsNotNullAndNotEmpty}?"));
                return;
            }
        }

        private static bool IsNotNullOrEmptyOperation(IOperation operation)
        {
            // Quick check for the full string first
            if (operation.Syntax.ToString() == IsNotNullOrEmpty)
            {
                return true;
            }

            // Maybe they wrote it like:
            //  Is.Not.Null
            //    .Or.Empty
            if (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
            }

            // Walk up the .Not.Null.Or.Empty chain.
            int propertyIndex = IsNotNullOrEmptyParts.Length - 1;

            while (operation is IPropertyReferenceOperation propertyReference &&
                propertyIndex >= 0 &&
                propertyReference.Property.Name == IsNotNullOrEmptyParts[propertyIndex])
            {
                propertyIndex--;

                if (propertyReference.Instance is null)
                {
                    // Reached the root of the chain.
                    string owningType = propertyReference.Member.ContainingType.Name;
                    return propertyIndex == 0 && IsNotNullOrEmptyParts[0] == owningType;
                }

                operation = propertyReference.Instance;
            }

            return false;
        }
    }
}
