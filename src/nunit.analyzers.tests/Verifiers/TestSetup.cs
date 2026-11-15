using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace NUnit.Analyzers.Tests.Verifiers
{
    /// <summary>
    /// Common configuration shared by all analyzer/code fix tests.
    /// </summary>
    internal static class TestSetup
    {
        private static readonly Lazy<Task<ImmutableArray<MetadataReference>>> metadataReferences =
            new(ResolveMetadataReferencesAsync, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets the reference assemblies for the framework the test host is running on.
        /// This matches the NUnit assembly that is referenced and keeps the
        /// <c>#if NETFRAMEWORK</c> conditions in the tests meaningful.
        /// </summary>
        public static ReferenceAssemblies ReferenceAssemblies { get; } =
#if NETFRAMEWORK
            ReferenceAssemblies.NetFramework.Net472.Default;
#else
            ReferenceAssemblies.Net.Net80;
#endif

        /// <summary>
        /// Gets the assemblies referenced by the test code on top of <see cref="ReferenceAssemblies"/>.
        /// </summary>
        public static IReadOnlyList<Assembly> AdditionalAssemblies { get; } =
        [
            typeof(NUnit.Framework.Assert).Assembly,
#if NUNIT4
            typeof(NUnit.Framework.Legacy.ClassicAssert).Assembly,
#endif
#if NETFRAMEWORK
            // These types are part of the .NET 8 reference assemblies, but come from packages on .NET Framework.
            typeof(IAsyncDisposable).Assembly, // Microsoft.Bcl.AsyncInterfaces, also IAsyncEnumerable<T>
            typeof(ValueTask<>).Assembly, // System.Threading.Tasks.Extensions, needed by IAsyncEnumerable<T>
            typeof(Span<>).Assembly, // System.Memory
            typeof(ImmutableArray<>).Assembly, // System.Collections.Immutable
#endif
        ];

        public static void Configure(SolutionState state)
        {
            state.AdditionalReferences.AddRange(AdditionalAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location)));
        }

        public static ParseOptions AugmentParseOptions(ParseOptions parseOptions)
        {
            return ((CSharpParseOptions)parseOptions)
                .WithLanguageVersion(LanguageVersion.Preview)
#if NUNIT4
                .WithPreprocessorSymbols("NUNIT4");
#else
                .WithPreprocessorSymbols("NUNIT3");
#endif
        }

        public static CompilationOptions AugmentCompilationOptions(CompilationOptions compilationOptions)
        {
            // The testing framework parses documentation comments in 'Diagnose' mode,
            // which would require every public member in the test code to be documented.
            var specificDiagnosticOptions = compilationOptions.SpecificDiagnosticOptions
                .SetItem("CS1591", ReportDiagnostic.Suppress);

            return ((CSharpCompilationOptions)compilationOptions)
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }

        /// <summary>
        /// Creates a compilation of <paramref name="sources"/> using the same references as the analyzer tests,
        /// for tests that inspect symbols or syntax directly instead of running an analyzer.
        /// </summary>
        public static async Task<Compilation> CreateCompilationAsync(params string[] sources)
        {
            var parseOptions = AugmentParseOptions(new CSharpParseOptions());
            IEnumerable<SyntaxTree> syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source, (CSharpParseOptions)parseOptions));

            return CSharpCompilation.Create(
                Guid.NewGuid().ToString("N"),
                syntaxTrees,
                await metadataReferences.Value,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        public static async Task<(SyntaxNode Node, Compilation Compilation, SemanticModel Model)> GetRootCompilationAndModelAsync(string code)
        {
            Compilation compilation = await CreateCompilationAsync(code);
            SyntaxTree tree = compilation.SyntaxTrees.Single();
            SyntaxNode root = await tree.GetRootAsync();

            return (root, compilation, compilation.GetSemanticModel(tree));
        }

        public static async Task<(SyntaxNode Node, SemanticModel Model)> GetRootAndModelAsync(string code)
        {
            (SyntaxNode root, _, SemanticModel model) = await GetRootCompilationAndModelAsync(code);

            return (root, model);
        }

        private static async Task<ImmutableArray<MetadataReference>> ResolveMetadataReferencesAsync()
        {
            ImmutableArray<MetadataReference> frameworkReferences =
                await ReferenceAssemblies.ResolveAsync(LanguageNames.CSharp, CancellationToken.None);

            return frameworkReferences.AddRange(AdditionalAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location)));
        }
    }
}
