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
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\Extensions\{nameof(IMethodSymbolExtensionsTests)}";

        [Test]
        public async Task GetParameterCounts()
        {
            var method = await this.GetMethodSymbolAsync(
                $"{IMethodSymbolExtensionsTests.BasePath}{(nameof(this.GetParameterCounts))}.cs",
                $"{nameof(IMethodSymbolExtensionsTests)}{nameof(GetParameterCounts)}");
            var counts = method.GetParameterCounts();

            Assert.That(counts.Item1, Is.EqualTo(3), nameof(counts.Item1));
            Assert.That(counts.Item2, Is.EqualTo(2), nameof(counts.Item2));
            Assert.That(counts.Item3, Is.EqualTo(1), nameof(counts.Item3));
        }

        private async Task<IMethodSymbol> GetMethodSymbolAsync(string file, string typeName)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(file);

            return rootAndModel.Item2.GetDeclaredSymbol(rootAndModel.Item1
                .DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Where(_ => _.Identifier.ValueText == typeName).Single()
                .DescendantNodes().OfType<MethodDeclarationSyntax>().Single()) as IMethodSymbol;
        }
    }
}
