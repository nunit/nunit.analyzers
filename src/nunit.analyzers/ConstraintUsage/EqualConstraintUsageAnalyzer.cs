using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualConstraintUsageAnalyzer : BaseConditionConstraintAnalyzer
    {
        private static readonly string IsEqualTo = $"{NameOfIs}.{NameOfIsEqualTo}";
        private static readonly string IsNotEqualTo = $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEqualTo}";

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.EqualConstraintUsage,
            title: EqualConstraintUsageConstants.Title,
            messageFormat: EqualConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: EqualConstraintUsageConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData(
            SyntaxNodeAnalysisContext context, ExpressionSyntax actual, bool negated)
        {
            var shouldReport = false;

            // 'actual == expected', 'Equals(actual, expected)' or 'actual.Equals(expected)'
            if (actual.IsKind(SyntaxKind.EqualsExpression)
                || IsStaticObjectEquals(actual, context)
                || IsInstanceObjectEquals(actual, context))
            {
                shouldReport = true;
            }

            // 'actual != expected'
            else if (actual.IsKind(SyntaxKind.NotEqualsExpression))
            {
                shouldReport = true;
                negated = !negated;
            }

            if (shouldReport)
            {
                var suggestedConstraint = negated ? IsNotEqualTo : IsEqualTo;
                return (descriptor, suggestedConstraint);
            }

            return (null, null);
        }

        private static bool IsStaticObjectEquals(ExpressionSyntax expressionSyntax, SyntaxNodeAnalysisContext context)
        {
            if (!(expressionSyntax is InvocationExpressionSyntax invocation))
                return false;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

            return methodSymbol != null
                && methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 2
                && methodSymbol.Name == nameof(object.Equals)
                && methodSymbol.ContainingType.SpecialType == SpecialType.System_Object;
        }

        private static bool IsInstanceObjectEquals(ExpressionSyntax expressionSyntax, SyntaxNodeAnalysisContext context)
        {
            if (!(expressionSyntax is InvocationExpressionSyntax invocation))
                return false;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

            return methodSymbol != null
                && !methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.Name == nameof(object.Equals);
        }
    }
}
