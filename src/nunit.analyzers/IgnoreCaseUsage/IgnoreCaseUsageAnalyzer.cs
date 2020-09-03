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

namespace NUnit.Analyzers.IgnoreCaseUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IgnoreCaseUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly string[] SupportedIsMethods = new[]
        {
            NunitFrameworkConstants.NameOfIsEqualTo,
            NunitFrameworkConstants.NameOfIsEquivalentTo,
            NunitFrameworkConstants.NameOfIsSupersetOf,
            NunitFrameworkConstants.NameOfIsSubsetOf
        };

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IgnoreCaseUsage,
            title: IgnoreCaseUsageAnalyzerConstants.Title,
            messageFormat: IgnoreCaseUsageAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: IgnoreCaseUsageAnalyzerConstants.Description);

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

                // e.g. Is.EqualTo(expected).IgnoreCase
                // Need to check type of expected

                var ignoreCaseSuffix = constraintPart.GetSuffix(NunitFrameworkConstants.NameOfIgnoreCase) as IPropertyReferenceOperation;

                if (ignoreCaseSuffix == null)
                    continue;

                var expectedType = constraintPart.GetExpectedArgument()?.Type;

                if (expectedType == null || expectedType.TypeKind == TypeKind.Error)
                    return;

                if (!IsTypeSupported(expectedType))
                {
                    var syntax = ignoreCaseSuffix.Syntax is MemberAccessExpressionSyntax memberAccessSyntax
                        ? memberAccessSyntax.Name
                        : ignoreCaseSuffix.Syntax;

                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        syntax.GetLocation()));
                }
            }
        }

        private static bool IsTypeSupported(ITypeSymbol type, HashSet<ITypeSymbol>? checkedTypes = null)
        {
            // Protection against possible infinite recursion
            checkedTypes ??= new HashSet<ITypeSymbol>();
            if (!checkedTypes.Add(type))
                return false;

            // Allowed - string, char
            if (type.SpecialType == SpecialType.System_String || type.SpecialType == SpecialType.System_Char)
                return true;

            if (type is IArrayTypeSymbol arrayType)
                return IsTypeSupported(arrayType.ElementType, checkedTypes);

            if (!(type is INamedTypeSymbol namedType))
                return false;

            if (namedType.IsTupleType)
                return namedType.TupleElements.Any(e => IsTypeSupported(e.Type, checkedTypes));

            var fullName = namedType.GetFullMetadataName();

            // Cannot determine if DictionaryEntry is valid, since not generic
            if (fullName == "System.Collections.DictionaryEntry")
                return true;

            if (fullName == "System.Collections.Generic.KeyValuePair`2")
                return namedType.TypeArguments.Any(t => IsTypeSupported(t, checkedTypes));

            if (fullName.StartsWith("System.Tuple`", StringComparison.Ordinal))
                return namedType.TypeArguments.Any(t => IsTypeSupported(t, checkedTypes));

            // Only value might be supported for Dictionary
            if (fullName == "System.Collections.Generic.Dictionary`2"
                && namedType.TypeArguments.Length == 2)
            {
                return IsTypeSupported(namedType.TypeArguments[1], checkedTypes);
            }

            if (namedType.IsIEnumerable(out var elementType))
            {
                if (elementType != null)
                {
                    return IsTypeSupported(elementType, checkedTypes);
                }
                else
                {
                    // If it implements only non-generic IEnumerable,
                    // IgnoreCase usage might be invalid, but we cannot determine that.
                    return true;
                }
            }

            return false;
        }
    }
}
