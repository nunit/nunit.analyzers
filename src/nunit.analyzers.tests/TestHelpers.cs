using System;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NUnit.Analyzers.Tests
{
    internal static class TestHelpers
    {
        internal static Compilation CreateCompilation(string code = null)
        {
            var syntaxTrees = code is null
                ? null
                : new[] { CSharpSyntaxTree.ParseText(code) };

            return CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees,
                references: AnalyzerAssert.MetadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        internal static async Task<(SyntaxNode Node, SemanticModel Model)> GetRootAndModel(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees: new[] { tree },
                references: AnalyzerAssert.MetadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync().ConfigureAwait(false);

            return (root, model);
        }
    }
}
