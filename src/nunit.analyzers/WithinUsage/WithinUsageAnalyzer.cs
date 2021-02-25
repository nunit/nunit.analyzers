using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.WithinUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WithinUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly string[] SupportedIsMethods = new[]
        {
            NunitFrameworkConstants.NameOfIsEqualTo,
            NunitFrameworkConstants.NameOfIsLessThan,
            NunitFrameworkConstants.NameOfIsLessThanOrEqualTo,
            NunitFrameworkConstants.NameOfIsGreaterThan,
            NunitFrameworkConstants.NameOfIsGreaterThanOrEqualTo,
        };

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.WithinIncompatibleTypes,
            title: WithinUsageAnalyzerConstants.Title,
            messageFormat: WithinUsageAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: WithinUsageAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation, out _, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                if (!SupportedIsMethods.Contains(constraintPart.GetConstraintName()))
                    continue;

                // e.g. Is.EqualTo(expected).Within(0.1)
                // Need to check type of expected

                var withinSuffix = constraintPart.GetSuffix(NunitFrameworkConstants.NameOfEqualConstraintWithin) as IInvocationOperation;

                if (withinSuffix == null)
                    continue;

                var expectedType = constraintPart.GetExpectedArgument()?.Type;

                if (expectedType == null || expectedType.TypeKind == TypeKind.Error)
                    return;

                if (!IsTypeSupported(expectedType))
                {
                    var syntax = withinSuffix.Syntax is InvocationExpressionSyntax expressionSyntax &&
                        expressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessSyntax
                            ? memberAccessSyntax.Name
                            : withinSuffix.Syntax;

                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        syntax.GetLocation()));
                }
            }
        }

        private static bool IsTypeSupported(ITypeSymbol type, HashSet<ITypeSymbol>? checkedTypes = null)
        {
            // Protection against possible infinite recursion
            // TODO Remove suppression when the below is released:
            // https://github.com/dotnet/roslyn-analyzers/issues/4568
#pragma warning disable RS1024 // Compare symbols correctly
            checkedTypes ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            if (!checkedTypes.Add(type))
                return false;

            // Allowed - numeric or date types
            if (type.SpecialType == SpecialType.System_Decimal ||
                type.SpecialType == SpecialType.System_Double ||
                type.SpecialType == SpecialType.System_Single ||
                type.SpecialType == SpecialType.System_Char ||
                type.SpecialType == SpecialType.System_Byte ||
                type.SpecialType == SpecialType.System_SByte ||
                type.SpecialType == SpecialType.System_Int16 ||
                type.SpecialType == SpecialType.System_Int32 ||
                type.SpecialType == SpecialType.System_Int64 ||
                type.SpecialType == SpecialType.System_UInt16 ||
                type.SpecialType == SpecialType.System_UInt32 ||
                type.SpecialType == SpecialType.System_UInt64 ||
                type.SpecialType == SpecialType.System_DateTime)
            {
                return true;
            }

            if (!(type is INamedTypeSymbol namedType))
                return false;

            // Allowed - tuples having any element of any supported type
            if (namedType.IsTupleType)
                return namedType.TupleElements.Any(e => IsTypeSupported(e.Type, checkedTypes));

            var fullName = namedType.GetFullMetadataName();

            if (fullName.StartsWith("System.Tuple`", StringComparison.Ordinal) ||
                fullName.StartsWith("System.ValueTuple", StringComparison.Ordinal))
            {
                return namedType.TypeArguments.Any(t => IsTypeSupported(t, checkedTypes));
            }

            if (fullName.Equals("System.TimeSpan", StringComparison.Ordinal))
                return true;

            if (fullName.Equals("System.DateTimeOffset", StringComparison.Ordinal))
                return true;

            return false;
        }
    }
}
