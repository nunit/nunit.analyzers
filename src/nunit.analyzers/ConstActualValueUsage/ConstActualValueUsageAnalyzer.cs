using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ConstActualValueUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstActualValueUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            AnalyzerIdentifiers.ConstActualValueUsage,
            ConstActualValueUsageAnalyzerConstants.Title,
            ConstActualValueUsageAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.Argument);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var argumentSyntax = context.Node as ArgumentSyntax;
            var argumentListSyntax = argumentSyntax?.Parent as ArgumentListSyntax;
            var invocationSyntax = argumentSyntax?.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();

            if (argumentSyntax == null || argumentListSyntax == null || invocationSyntax == null)
                return;

            var symbol = context.SemanticModel.GetSymbolInfo(invocationSyntax.Expression).Symbol;

            if (!(symbol is IMethodSymbol methodSymbol) || !methodSymbol.ContainingType.IsAssert())
                return;

            var parameterSymbol = argumentSyntax.NameColon != null
                ? methodSymbol.Parameters.FirstOrDefault(p => p.Name == argumentSyntax.NameColon.Name.Identifier.Text)
                : methodSymbol.Parameters.ElementAtOrDefault(argumentListSyntax.Arguments.IndexOf(argumentSyntax));

            if (parameterSymbol?.Name == NunitFrameworkConstants.NameOfActualParameter)
            {
                var argumentSymbol = context.SemanticModel.GetSymbolInfo(argumentSyntax.Expression).Symbol;

                if (argumentSyntax.Expression is LiteralExpressionSyntax
                    || (argumentSymbol is ILocalSymbol localSymbol && localSymbol.IsConst)
                    || (argumentSymbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        argumentSyntax.GetLocation()));
                }
            }
        }
    }
}
