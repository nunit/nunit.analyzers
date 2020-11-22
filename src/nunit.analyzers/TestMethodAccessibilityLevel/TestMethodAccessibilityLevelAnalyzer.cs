using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.TestMethodAccessibilityLevel
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestMethodAccessibilityLevelAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor testMethodIsNotPublic = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodIsNotPublic,
            title: TestMethodAccessibilityLevelConstants.TestMethodIsNotPublicTitle,
            messageFormat: TestMethodAccessibilityLevelConstants.TestMethodIsNotPublicMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestMethodAccessibilityLevelConstants.TestMethodIsNotPublicDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(testMethodIsNotPublic);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (IsTestMethod(context.Compilation, methodSymbol))
            {
                if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        testMethodIsNotPublic,
                        methodSymbol.Locations[0]));
                }
            }
            else if (IsSetUpTearDownMethod(context.Compilation, methodSymbol))
            {
                if (methodSymbol.DeclaredAccessibility != Accessibility.Public
                    && methodSymbol.DeclaredAccessibility != Accessibility.Protected)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        testMethodIsNotPublic,
                        methodSymbol.Locations[0]));
                }
            }
        }

        private static bool IsTestMethod(Compilation compilation, IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Any(a => a.IsTestMethodAttribute(compilation));
        }

        private static bool IsSetUpTearDownMethod(Compilation compilation, IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Any(a => a.IsSetUpOrTearDownMethodAttribute(compilation));
        }
    }
}
