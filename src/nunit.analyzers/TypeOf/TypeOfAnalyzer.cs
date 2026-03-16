using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

/*
 * // generic 'expected'
-Assert.That(0.GetType(), Is.EqualTo(typeof(int)));
+Assert.That(0, Is.TypeOf<int>());

// non-generic 'expected'
-Assert.That(0.GetType(), Is.EqualTo(1.GetType()));
+Assert.That(0, Is.TypeOf(1.GetType()));
*/

namespace NUnit.Analyzers.TypeOfAnalyzer
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

            if (actualOperation is null)
                return;

            /* 
            var isConstraintTrue = constraintExpression.IsTrueOrFalse();

            if (actualOperation is IIsTypeOperation isTypeOperation && isConstraintTrue is not null)
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add(IsConstraintIsTrue, isConstraintTrue.ToString());

                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    actualOperation.Syntax.GetLocation(),
                    properties.ToImmutable(),
                    isTypeOperation.TypeOperand.ToString()));
            }
            */

            // Report if the actual operation ends with a GetType() invocation, and the constraint is a Is.EqualTo constraint
            // We then need to determine if the body of Is.EqualTo is a GetType() or a typeof() expression, and report accordingly
             if (actualOperation is IInvocationOperation getTypeInvocation &&
                getTypeInvocation.TargetMethod.Name == "GetType" &&
                constraintExpression.IsEqualToConstraint(out var equalToConstraint) &&
                equalToConstraint.Body is IInvocationOperation equalToGetTypeInvocation &&
                equalToGetTypeInvocation.TargetMethod.Name == "GetType")
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    actualOperation.Syntax.GetLocation(),
                    equalToGetTypeInvocation.Instance?.Syntax.ToString()));
            }
            else if (actualOperation is IInvocationOperation getTypeInvocation2 &&
                getTypeInvocation2.TargetMethod.Name == "GetType" &&
                constraintExpression.IsEqualToConstraint(out var equalToConstraint2) &&
                equalToConstraint2.Body is ITypeOfOperation typeOfOperation)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    actualOperation.Syntax.GetLocation(),
                    typeOfOperation.TypeOperand.ToString()));
            }
    }
}
