using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.NonTestMethodAccessibilityLevel
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NonTestMethodAccessibilityLevelAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor nonTestMethodIsPublic = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.NonTestMethodIsPublic,
            title: NonTestMethodAccessibilityLevelConstants.NonTestMethodIsPublicTitle,
            messageFormat: NonTestMethodAccessibilityLevelConstants.NonTestMethodIsPublicMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: false,
            description: NonTestMethodAccessibilityLevelConstants.NonTestMethodIsPublicDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(nonTestMethodIsPublic);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            // Is this even a TestFixture, i.e.: Does it has any test methods.
            // If not public methods are fine.
            bool hasTestMethods = false;

            var publicNonTestMethods = new List<IMethodSymbol>();

            var methods = typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary);
            foreach (var method in methods)
            {
                if (IsTestRelatedMethod(context.Compilation, method))
                    hasTestMethods = true;
                else if (IsPublicOrInternalMethod(method))
                    publicNonTestMethods.Add(method);
            }

            if (hasTestMethods)
            {
                foreach (var method in publicNonTestMethods)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        nonTestMethodIsPublic,
                        method.Locations[0]));
                }
            }
        }

        private static bool IsTestRelatedMethod(Compilation compilation, IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Any(
                a => a.IsSetUpOrTearDownMethodAttribute(compilation) || a.IsTestMethodAttribute(compilation));
        }

        private static bool IsPublicOrInternalMethod(IMethodSymbol method)
        {
            switch(method.DeclaredAccessibility)
            {
                case Accessibility.Public:
                case Accessibility.Internal:
                case Accessibility.ProtectedOrInternal:
                    return true;

                default:
                    return false;
            }
        }
    }
}
