using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ConstActualValueUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstActualValueUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ConstActualValueUsage,
            title: ConstActualValueUsageAnalyzerConstants.Title,
            messageFormat: ConstActualValueUsageAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ConstActualValueUsageAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            var actualExpression = assertExpression.GetArgumentExpression(methodSymbol, NunitFrameworkConstants.NameOfActualParameter);

            if (actualExpression == null)
                return;

            var argumentSymbol = context.SemanticModel.GetSymbolInfo(actualExpression).Symbol;

            if (actualExpression is LiteralExpressionSyntax
                || (argumentSymbol is ILocalSymbol localSymbol && localSymbol.IsConst)
                || (argumentSymbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    actualExpression.GetLocation()));
            }
        }
    }
}
