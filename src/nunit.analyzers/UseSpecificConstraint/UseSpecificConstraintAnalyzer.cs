using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.UseSpecificConstraint
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseSpecificConstraintAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor simplifyConstraint = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.UseSpecificConstraint,
            title: UseSpecificConstraintConstants.UseSpecificConstraintTitle,
            messageFormat: UseSpecificConstraintConstants.UseSpecificConstraintMessage,
            category: Categories.Style,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: UseSpecificConstraintConstants.UseSpecificConstraintDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(simplifyConstraint);

        protected override void AnalyzeAssertInvocation(Version nunitVersion, OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);
            if (actualType is null)
                return;

            foreach (var constraintPartExpression in constraintExpression.ConstraintParts)
            {
                if (constraintPartExpression.HasIncompatiblePrefixes()
                    || constraintPartExpression.HasCustomComparer()
                    || constraintPartExpression.HasUnknownExpressions())
                {
                    return;
                }

                var constraintMethod = constraintPartExpression.GetConstraintMethod();
                if (constraintMethod?.Name != NUnitFrameworkConstants.NameOfIsEqualTo)
                    continue;

                var expectedOperation = constraintPartExpression.GetExpectedArgument();
                if (expectedOperation is null)
                    continue;

                string? constraint = null;

                // Look for both direct `0` and cast `(short)0` to catch all 0 values.
                ILiteralOperation? literalOperation = (expectedOperation is IConversionOperation conversionOperation ?
                    conversionOperation.Operand : expectedOperation) as ILiteralOperation;

                if (literalOperation is not null)
                {
                    if (literalOperation.ConstantValue.HasValue)
                    {
                        constraint = literalOperation.ConstantValue.Value switch
                        {
                            null => NUnitFrameworkConstants.NameOfIsNull,
                            false => NUnitFrameworkConstants.NameOfIsFalse,
                            true => NUnitFrameworkConstants.NameOfIsTrue,
                            0 or 0d or 0f or 0m => NUnitFrameworkConstants.NameOfIsZero,
                            _ => null,
                        };

                        if (constraint is null &&
                            literalOperation.ConstantValue.Value is IConvertible convertible)
                        {
                            if (convertible.ToDouble(null) == 0)
                            {
                                // Catches all other 0 values: (byte)0, (short)0, 0u, 0L, 0uL
                                constraint = NUnitFrameworkConstants.NameOfIsZero;
                            }
                        }
                    }
                }
                else if (expectedOperation is IDefaultValueOperation defaultValueOperation)
                {
                    if (defaultValueOperation.Type is INamedTypeSymbol defaultType)
                    {
                        if (defaultType.SpecialType == SpecialType.System_Object ||
                            defaultType.SpecialType == SpecialType.System_String)
                        {
                            constraint = NUnitFrameworkConstants.NameOfIsNull;
                        }
                        else if (nunitVersion.Major >= 4)
                        {
                            // We cannot use `Is.Default` if the actual type is `object`.
                            // Note that the case `default(object)` is handled above.
                            if (actualType.SpecialType == SpecialType.System_Object)
                                continue;

                            constraint = NUnitV4FrameworkConstants.NameOfIsDefault;
                        }
                        else if (defaultType.SpecialType == SpecialType.System_Boolean)
                        {
                            constraint = NUnitFrameworkConstants.NameOfIsFalse;
                        }
                    }
                }

                if (constraint is not null)
                {
                    SyntaxNode syntax = constraintPartExpression.Root!.Syntax;
                    var diagnostic = Diagnostic.Create(simplifyConstraint, syntax.GetLocation(),
                        new Dictionary<string, string?>
                        {
                            [AnalyzerPropertyKeys.SpecificConstraint] = constraint,
                        }.ToImmutableDictionary(),
                        expectedOperation.Syntax.ToString(), constraint);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
