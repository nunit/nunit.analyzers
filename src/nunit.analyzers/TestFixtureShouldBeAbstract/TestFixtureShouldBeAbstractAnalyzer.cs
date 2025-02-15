using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.TestFixtureShouldBeAbstract
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestFixtureShouldBeAbstractAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor testFixtureIsNotAbstract = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.BaseTestFixtureIsNotAbstract,
            title: TestFixtureShouldBeAbstractConstants.Title,
            messageFormat: TestFixtureShouldBeAbstractConstants.Message,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: TestFixtureShouldBeAbstractConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(testFixtureIsNotAbstract);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationAction(DerivedFixturesShouldBeAbstract);
        }

        private static void DerivedFixturesShouldBeAbstract(CompilationAnalysisContext context)
        {
            IEnumerable<INamespaceSymbol> namespaces = context.Compilation.GlobalNamespace.GetNamespaceMembers();

            // Determine all classes that are defined in this assembly and are used as a base class.
            HashSet<INamedTypeSymbol> baseClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            CheckNamespaces(baseClasses, context.Compilation.Assembly, namespaces);

            // Now check if any of these are actually TestFixtures and defined in this assembly.
            foreach (var baseClass in baseClasses)
            {
                if (!baseClass.IsAbstract &&
                    SymbolEqualityComparer.Default.Equals(baseClass.ContainingAssembly, context.Compilation.Assembly) &&
                    baseClass.IsTestFixture(context.Compilation))
                {
                    // Class is defined in this assembly, has tests, used as a base class and not abstract
                    context.ReportDiagnostic(Diagnostic.Create(
                        testFixtureIsNotAbstract,
                        baseClass.Locations[0],
                        baseClass.Name));
                }
            }
        }

        private static void CheckNamespaces(HashSet<INamedTypeSymbol> baseClasses, IAssemblySymbol assembly, IEnumerable<INamespaceSymbol> namespaces)
        {
            foreach (var @namespace in namespaces)
            {
                CheckNamespace(baseClasses, assembly, @namespace);
            }
        }

        private static void CheckNamespace(HashSet<INamedTypeSymbol> baseClasses, IAssemblySymbol assembly, INamespaceSymbol @namespace)
        {
            // Check child namespaces
            CheckNamespaces(baseClasses, assembly, @namespace.GetNamespaceMembers());

            if (!SymbolEqualityComparer.Default.Equals(@namespace.ContainingAssembly, assembly))
            {
                // Namespace is not defined in my assembly.
                return;
            }

            ImmutableArray<INamedTypeSymbol> namedTypeSymbols = @namespace.GetTypeMembers();

            if (namedTypeSymbols.Length == 0)
            {
                return;
            }

            IEnumerable<INamedTypeSymbol> classDefinitions = namedTypeSymbols
                .Where(t => t.IsReferenceType);

            IEnumerable<INamedTypeSymbol> baseClassDefinitions = classDefinitions
                .Where(t => t.BaseType is not null && t.BaseType.SpecialType != SpecialType.System_Object)
                .Select(t => t.BaseType!)
                .Select(t => t.IsGenericType ? t.ConstructedFrom : t);
            baseClasses.UnionWith(baseClassDefinitions);
        }
    }
}
