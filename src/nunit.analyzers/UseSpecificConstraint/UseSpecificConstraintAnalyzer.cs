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
                        object? constantValue = literalOperation.ConstantValue.Value;

                        constraint = constantValue switch
                        {
                            null => NUnitFrameworkConstants.NameOfIsNull,
                            false => NUnitFrameworkConstants.NameOfIsFalse,
                            true => NUnitFrameworkConstants.NameOfIsTrue,
                            0 or 0d or 0f or 0m => NUnitFrameworkConstants.NameOfIsZero,
                            _ => null,
                        };

                        if (constraint is null &&
                            constantValue is not null &&
                            constantValue.GetType().IsPrimitive &&
                            constantValue is IConvertible convertible)
                        {
                            // We do not know what exceptions this conversion may throw, so we catch all exceptions.
#pragma warning disable CA1031 // Do not catch general exception types
                            try
                            {
                                if (convertible.ToDouble(null) == 0)
                                {
                                    // Catches all other 0 values: (byte)0, (short)0, 0u, 0L, 0uL
                                    constraint = NUnitFrameworkConstants.NameOfIsZero;
                                }
                            }
                            catch
                            {
                                // Ignore conversion errors, we cannot use this value as a constraint.
                            }
#pragma warning restore CA1031 // Do not catch general exception types
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

                            if (!SymbolEqualityComparer.Default.Equals(actualType, defaultType))
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
