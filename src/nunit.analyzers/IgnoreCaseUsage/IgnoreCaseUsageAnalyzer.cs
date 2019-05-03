using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.IgnoreCaseUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IgnoreCaseUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string[] SupportedIsMethods = new[]
        {
            NunitFrameworkConstants.NameOfIsEqualTo,
            NunitFrameworkConstants.NameOfIsEquivalentTo,
            NunitFrameworkConstants.NameOfIsSupersetOf,
            NunitFrameworkConstants.NameOfIsSubsetOf
        };

        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            AnalyzerIdentifiers.IgnoreCaseUsage,
            IgnoreCaseUsageAnalyzerConstants.Title,
            IgnoreCaseUsageAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            // e.g. Is.EqualTo(expected).IgnoreCase
            // Need to check type of expected 
            var ignoreCaseAccessSyntax = context.Node as MemberAccessExpressionSyntax;
            var expectedType = GetExpectedTypeSymbol(ignoreCaseAccessSyntax, context);

            if (expectedType == null)
                return;

            if (!IsTypeSupported(expectedType))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    ignoreCaseAccessSyntax.Name.GetLocation()));
            }
        }

        private static bool IsTypeSupported(ITypeSymbol type, HashSet<ITypeSymbol> checkedTypes = null)
        {
            // Protection against possible infinite recursion
            checkedTypes = checkedTypes ?? new HashSet<ITypeSymbol>();
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

            // Cannot determine if DictionaryEntry is valid
            if (fullName == "System.Collections.DictionaryEntry")
                return true;

            if (fullName == "System.Collections.Generic.KeyValuePair`2"
                || fullName == "System.Tuple`2")
            {
                return namedType.TypeArguments.Any(t => IsTypeSupported(t, checkedTypes));
            }

            // Only value might be supported for Dictionary
            if (fullName == "System.Collections.Generic.Dictionary`2"
                && namedType.TypeArguments.Length == 2)
            {
                return IsTypeSupported(namedType.TypeArguments[1], checkedTypes);
            }

            var allInterfaces = namedType.AllInterfaces.ToList();

            if (namedType.TypeKind == TypeKind.Interface)
                allInterfaces.Add(namedType);

            var iEnumerableInterface = allInterfaces.FirstOrDefault(i =>
                i.GetFullMetadataName() == "System.Collections.Generic.IEnumerable`1");
            var genericArgument = iEnumerableInterface?.TypeArguments.FirstOrDefault();

            if (genericArgument != null)
                return IsTypeSupported(genericArgument, checkedTypes);

            // Exception - if it implements only non-generic IEnumerable.
            // It might be invalid, but we cannot determine that
            if (genericArgument == null && allInterfaces.Any(i =>
                i.GetFullMetadataName() == "System.Collections.IEnumerable"))
            {
                return true;
            }

            return false;
        }

        private static ITypeSymbol GetExpectedTypeSymbol(MemberAccessExpressionSyntax ignoreCaseAccessSyntax, SyntaxNodeAnalysisContext context)
        {
            if (ignoreCaseAccessSyntax?.Name.ToString() != NunitFrameworkConstants.NameOfIgnoreCase)
                return null;

            if (!(ignoreCaseAccessSyntax.Expression is InvocationExpressionSyntax invocationExpression))
                return null;

            if (!(invocationExpression.Expression is MemberAccessExpressionSyntax isMethodAccess
                && SupportedIsMethods.Contains(isMethodAccess.Name.ToString())))
            {
                return null;
            }

            var expectedArgument = invocationExpression.ArgumentList.Arguments.FirstOrDefault();

            if (expectedArgument == null)
                return null;

            var expectedType = context.SemanticModel.GetTypeInfo(expectedArgument.Expression).Type;

            if (expectedType == null || expectedType is IErrorTypeSymbol)
                return null;

            return expectedType;
        }
    }
}
