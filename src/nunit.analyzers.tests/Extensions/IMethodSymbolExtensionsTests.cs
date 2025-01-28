using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
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
            var (method, _) = await GetMethodSymbolAsync(testCode).ConfigureAwait(false);
            var (requiredParameters, optionalParameters, paramsCount) = method.GetParameterCounts(false, null);

            Assert.Multiple(() =>
            {
                Assert.That(requiredParameters, Is.EqualTo(3), nameof(requiredParameters));
                Assert.That(optionalParameters, Is.EqualTo(2), nameof(optionalParameters));
                Assert.That(paramsCount, Is.EqualTo(1), nameof(paramsCount));
            });
        }

        [Test]
        public async Task GetParameterCountsWithCancellationToken([Values] bool hasCancelAfter)
        {
            var testCode = @"
using System.Threading;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IMethodSymbolExtensionsTestsGetParameterCounts
    {
        public void Foo(int a1, int a2, int a3, CancellationToken cancellationToken) { }
    }
}";
            var (method, compilation) = await GetMethodSymbolAsync(testCode).ConfigureAwait(false);
            INamedTypeSymbol? cancellationTokenType = compilation.GetTypeByMetadataName(NUnitV4FrameworkConstants.FullNameOfCancellationToken);

            var (requiredParameters, optionalParameters, paramsCount) = method.GetParameterCounts(hasCancelAfter, cancellationTokenType);
            int adjustment = hasCancelAfter ? 0 : 1;

            Assert.Multiple(() =>
            {
                Assert.That(requiredParameters, Is.EqualTo(3 + adjustment), nameof(requiredParameters));
                Assert.That(optionalParameters, Is.EqualTo(1 - adjustment), nameof(optionalParameters));
                Assert.That(paramsCount, Is.EqualTo(0), nameof(paramsCount));
            });
        }

        private static async Task<(IMethodSymbol MethodSymbol, Compilation Compilation)> GetMethodSymbolAsync(string code)
        {
            var rootCompilationAndModel = await TestHelpers.GetRootCompilationAndModel(code).ConfigureAwait(false);

            MethodDeclarationSyntax methodDeclaration = rootCompilationAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>().Single()
                .DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            IMethodSymbol? methodSymbol = rootCompilationAndModel.Model.GetDeclaredSymbol(methodDeclaration);

            Assert.That(methodSymbol, Is.Not.Null, $"Cannot find symbol for {methodDeclaration.Identifier}");

            return (methodSymbol!, rootCompilationAndModel.Compilation);
        }
    }
}
