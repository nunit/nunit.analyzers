using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Extensions;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Extensions
{
    [TestFixture]
    public sealed class AttributeSyntaxExtensionsTests
    {
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\Extensions\{nameof(AttributeSyntaxExtensionsTests)}";

        [Test]
        public async Task GetArguments()
        {
            var attribute = await AttributeSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeSyntaxExtensionsTests.BasePath}{(nameof(this.GetArguments))}.cs",
                $"{nameof(AttributeSyntaxExtensionsTests)}{nameof(this.GetArguments)}");
            var arguments = attribute.GetArguments();

            Assert.That(arguments.Item1.Length, Is.EqualTo(1), nameof(arguments.Item1));
            Assert.That(arguments.Item2.Length, Is.EqualTo(2), nameof(arguments.Item2));
        }

        [Test]
        public async Task GetArgumentsWhenNoneExist()
        {
            var attribute = await AttributeSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeSyntaxExtensionsTests.BasePath}{(nameof(this.GetArgumentsWhenNoneExist))}.cs",
                $"{nameof(AttributeSyntaxExtensionsTests)}{nameof(this.GetArgumentsWhenNoneExist)}");
            var arguments = attribute.GetArguments();

            Assert.That(arguments.Item1.Length, Is.EqualTo(0), nameof(arguments.Item1));
            Assert.That(arguments.Item2.Length, Is.EqualTo(0), nameof(arguments.Item2));
        }

        private async static Task<AttributeSyntax> GetAttributeSyntaxAsync(string file, string typeName)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(file);

            return rootAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Where(_ => _.Identifier.ValueText == typeName).Single()
                .DescendantNodes().OfType<AttributeSyntax>().Single();
        }
    }
}
