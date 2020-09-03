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
            var method = await GetMethodSymbolAsync(testCode).ConfigureAwait(false);
            var (requiredParameters, optionalParameters, paramsCount) = method.GetParameterCounts();

            Assert.That(requiredParameters, Is.EqualTo(3), nameof(requiredParameters));
            Assert.That(optionalParameters, Is.EqualTo(2), nameof(optionalParameters));
            Assert.That(paramsCount, Is.EqualTo(1), nameof(paramsCount));
        }

        private static async Task<IMethodSymbol> GetMethodSymbolAsync(string code)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(code).ConfigureAwait(false);

            return rootAndModel.Model.GetDeclaredSymbol(rootAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>().Single()
                .DescendantNodes().OfType<MethodDeclarationSyntax>().Single()) as IMethodSymbol;
        }
    }
}
