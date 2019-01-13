using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Extensions;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Extensions
{
    [TestFixture]
    public sealed class IMethodSymbolExtensionsTests
    {
        [Test]
        public async Task GetParameterCounts()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IMethodSymbolExtensionsTestsGetParameterCounts
    {
        public void Foo(int a1, int a2, int a3, string b1 = ""b1"", string b2 = ""b2"", params char[] c) { }
    }
}";
            var method = await this.GetMethodSymbolAsync(testCode);
            var counts = method.GetParameterCounts();

            Assert.That(counts.Item1, Is.EqualTo(3), nameof(counts.Item1));
            Assert.That(counts.Item2, Is.EqualTo(2), nameof(counts.Item2));
            Assert.That(counts.Item3, Is.EqualTo(1), nameof(counts.Item3));
        }

        private async Task<IMethodSymbol> GetMethodSymbolAsync(string code)
        {
            var rootAndModel = await TestHelpers.GetRootAndModelFromString(code);

            return rootAndModel.Model.GetDeclaredSymbol(rootAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>().Single()
                .DescendantNodes().OfType<MethodDeclarationSyntax>().Single()) as IMethodSymbol;
        }
    }
}
