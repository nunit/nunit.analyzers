using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;

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

            // Allowed - string, char
            if (IsStringOrChar(expectedType))
                return;

            var allInterfaces = expectedType.AllInterfaces.ToList();

            if (expectedType.TypeKind == TypeKind.Interface && expectedType is INamedTypeSymbol namedInterface)
                allInterfaces.Add(namedInterface);

            var iEnumerableInterface = allInterfaces
                .FirstOrDefault(i => i.Name == nameof(IEnumerable) && i.IsGenericType);
            var genericArgument = iEnumerableInterface?.TypeArguments.FirstOrDefault();

            // Collection of strings/chars is allowed
            if (genericArgument != null && IsStringOrChar(genericArgument))
                return;

            // Dictionary with string/char value is allowed
            if (genericArgument != null && genericArgument.Name == "KeyValuePair"
                && genericArgument is INamedTypeSymbol namedType
                && namedType.TypeArguments.Length == 2)
            {
                var valueType = namedType.TypeArguments[1];

                if (IsStringOrChar(valueType))
                    return;
            }

            // Exception - if it implements only non-generic IEnumerable.
            // It might be invalid, but we cannot determine that
            if (genericArgument == null && allInterfaces
                .Any(i => i.Name == nameof(IEnumerable) && !i.IsGenericType))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                descriptor,
                ignoreCaseAccessSyntax.Name.GetLocation()));
        }

        private static bool IsStringOrChar(ITypeSymbol typeSymbol)
        {
            return typeSymbol.SpecialType == SpecialType.System_String
                || typeSymbol.SpecialType == SpecialType.System_Char;
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
