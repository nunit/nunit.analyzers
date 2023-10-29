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
            NUnitFrameworkConstants.NameOfIsEqualTo,
            NUnitFrameworkConstants.NameOfIsLessThan,
            NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo,
            NUnitFrameworkConstants.NameOfIsGreaterThan,
            NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo,
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

                var withinSuffix = constraintPart.GetSuffix(NUnitFrameworkConstants.NameOfEqualConstraintWithin) as IInvocationOperation;

                if (withinSuffix is null)
                    continue;

                var expectedType = constraintPart.GetExpectedArgument()?.Type;

                if (expectedType is null || expectedType.TypeKind == TypeKind.Error)
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
            checkedTypes ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            if (!checkedTypes.Add(type))
                return false;

            // Allowed - numeric or date types
            if (type.SpecialType == SpecialType.System_Decimal ||
                type.SpecialType == SpecialType.System_Double ||
                type.SpecialType == SpecialType.System_Single ||
                /* type.SpecialType == SpecialType.System_Char ||  // Exception has separate comparision */
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

            if (type.SpecialType == SpecialType.System_Object)
                return true; // We have no idea of the underlying type.

            if (type.SpecialType == SpecialType.System_String)
                return false; // Even though it implements IEnumerable, it doesn't support Tolerance.

            if (type.TypeKind == TypeKind.Enum)
                return false;

            if (type is IArrayTypeSymbol arrayType && IsTypeSupported(arrayType.ElementType, checkedTypes))
                return true;

            if (type is not INamedTypeSymbol namedType)
                return false;

            bool implementsNonGenericIEnumerable = false;

            foreach (var interfaceType in type.Interfaces)
            {
                string interfaceTypeName = interfaceType.GetFullMetadataName();
                if (interfaceTypeName.StartsWith("System.Collections.Generic.IEnumerable`", StringComparison.Ordinal))
                {
                    return IsTypeSupported(interfaceType.TypeArguments[0], checkedTypes);
                }

                if (interfaceTypeName.Equals("System.Collections.IEnumerable", StringComparison.Ordinal))
                    implementsNonGenericIEnumerable = true;
            }

            // Allowed - tuples having any element of any supported type
            if (namedType.IsTupleType)
                return namedType.TupleElements.Any(e => IsTypeSupported(e.Type, checkedTypes));

            string fullName = namedType.GetFullMetadataName();

            if (fullName.StartsWith("System.Tuple`", StringComparison.Ordinal) ||
                fullName.StartsWith("System.ValueTuple", StringComparison.Ordinal))
            {
                return namedType.TypeArguments.Any(t => IsTypeSupported(t, checkedTypes));
            }

            if (fullName.Equals("System.TimeSpan", StringComparison.Ordinal))
                return true;

            if (fullName.Equals("System.DateTimeOffset", StringComparison.Ordinal))
                return true;

            // Check for Nullable<T>
            if (fullName.Equals("System.Nullable`1", StringComparison.Ordinal))
            {
                return IsTypeSupported(namedType.TypeArguments[0]);
            }

            if (fullName.StartsWith("System.Collections.Generic.KeyValuePair`", StringComparison.Ordinal))
            {
                // We pass tolerance to the Value Type.
                return IsTypeSupported(namedType.TypeArguments[1], checkedTypes);
            }

            if (implementsNonGenericIEnumerable ||
                fullName.Equals("System.Collections.DictionaryEntry", StringComparison.Ordinal))
            {
                return true; // Non-generic collections, we have no idea of the actual type used.
            }

            // If the type overrides Equals, NUnit won't use tolerance
            return type.GetMembers("Equals").Length == 0;
        }
    }
}
