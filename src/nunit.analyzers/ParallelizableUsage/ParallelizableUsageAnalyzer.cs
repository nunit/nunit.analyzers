using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ParallelizableUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParallelizableUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor scopeSelfNoEffectOnAssemblyUsage = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage,
            title: ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyTitle,
            messageFormat: ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyDescription);

        private static readonly DiagnosticDescriptor scopeChildrenOnNonParameterizedTest = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ParallelScopeChildrenOnNonParameterizedTestMethodUsage,
            title: ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodTitle,
            messageFormat: ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodDescription);

        private static readonly DiagnosticDescriptor scopeFixturesOnTest = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage,
            title: ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodTitle,
            messageFormat: ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            scopeSelfNoEffectOnAssemblyUsage,
            scopeChildrenOnNonParameterizedTest,
            scopeFixturesOnTest);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(AnalyzeCompilation);
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            if (!TryGetAttributeEnumValue(context.Compilation, context.Compilation.Assembly,
                out int enumValue,
                out var attributeData))
            {
                return;
            }

            if (HasExactFlag(enumValue, ParallelizableUsageAnalyzerConstants.ParallelScope.Self))
            {
                // Specifying ParallelScope.Self on an assembly level attribute has no effect
                context.ReportDiagnostic(Diagnostic.Create(
                    scopeSelfNoEffectOnAssemblyUsage,
                    attributeData.ApplicationSyntaxReference.GetLocation()));
            }
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            if (!TryGetAttributeEnumValue(context.Compilation, context.Symbol,
                out int enumValue,
                out var attributeData))
            {
                return;
            }

            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (HasFlag(enumValue, ParallelizableUsageAnalyzerConstants.ParallelScope.Children))
            {
                // One may not specify ParallelScope.Children on a non-parameterized test method
                if (IsNonParameterizedTestMethod(context, methodSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(scopeChildrenOnNonParameterizedTest,
                        attributeData.ApplicationSyntaxReference.GetLocation()));
                }
            }
            else if (HasFlag(enumValue, ParallelizableUsageAnalyzerConstants.ParallelScope.Fixtures))
            {
                // One may not specify ParallelScope.Fixtures on a test method
                context.ReportDiagnostic(Diagnostic.Create(scopeFixturesOnTest,
                    attributeData.ApplicationSyntaxReference.GetLocation()));
            }
        }

        private static bool TryGetAttributeEnumValue(Compilation compilation, ISymbol symbol,
            out int enumValue,
            [NotNullWhen(true)] out AttributeData? attributeData)
        {
            enumValue = 0;
            attributeData = null;

            var parallelizableAttributeType = compilation.GetTypeByMetadataName(
                NUnitFrameworkConstants.FullNameOfTypeParallelizableAttribute);
            if (parallelizableAttributeType == null)
                return false;

            attributeData = symbol.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, parallelizableAttributeType));

            if (attributeData?.ApplicationSyntaxReference is null)
                return false;

            var optionalEnumValue = GetOptionalEnumValue(attributeData);
            if (optionalEnumValue == null)
                return false;

            enumValue = optionalEnumValue.Value;
            return true;
        }

        private static int? GetOptionalEnumValue(AttributeData attributeData)
        {
            var attributePositionalArguments = attributeData.ConstructorArguments;
            var noExplicitEnumArgument = attributePositionalArguments.Length == 0;
            if (noExplicitEnumArgument)
            {
                return ParallelizableUsageAnalyzerConstants.ParallelScope.Self;
            }
            else
            {
                var arg = attributePositionalArguments[0];
                return arg.Value as int?;
            }
        }

        private static bool IsNonParameterizedTestMethod(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
        {
            // The method is only a parametric method if (see DefaultTestCaseBuilder.BuildFrom)
            // * it has parameters
            // * is marked with one or more attributes deriving from ITestBuilder
            // * the attributes defines tests (difficult to access without evaluating the code)
            bool noParameters = methodSymbol.Parameters.IsEmpty;
            bool noITestBuilders = !methodSymbol.GetAttributes().Any(a => a.DerivesFromITestBuilder(context.Compilation));

            return noParameters && noITestBuilders;
        }

        private static bool HasFlag(int enumValue, int flag)
            => (enumValue & flag) == flag;

        private static bool HasExactFlag(int enumValue, int flag)
            => enumValue == flag;
    }
}
