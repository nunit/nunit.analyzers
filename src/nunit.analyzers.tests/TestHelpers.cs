using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests
{
    internal static class TestHelpers
    {
        internal static async Task<(SyntaxNode Node, SemanticModel Model)> GetRootAndModel(string file)
        {
            var code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);

            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
                syntaxTrees: new[] { tree },
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location)
                });

            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync().ConfigureAwait(false);

            return (root, model);
        }
    }
}
