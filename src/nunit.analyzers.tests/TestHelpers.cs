using System;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests
{
    internal static class TestHelpers
    {
        internal static async Task<(SyntaxNode Node, SemanticModel Model)> GetRootAndModel(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees: new[] { tree },
                references: MetadataReferences.Transitive(typeof(Assert)));

            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync().ConfigureAwait(false);

            return (root, model);
        }
    }
}
