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
        [Test]
        public async Task GetArguments()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class GetArguments
    {
        [Arguments(34, AProperty = 22d, BProperty = 33d)]
        public void Foo() { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ArgumentsAttribute : Attribute
    {
        public ArgumentsAttribute(int x) { }

        public double AProperty { get; set; }

        public double BProperty { get; set; }
    }
}";
            var attribute = await GetAttributeSyntaxAsync(testCode, "GetArguments").ConfigureAwait(false);
            var (positionalArguments, namedArguments) = attribute.GetArguments();

            Assert.That(positionalArguments.Length, Is.EqualTo(1), nameof(positionalArguments));
            Assert.That(namedArguments.Length, Is.EqualTo(2), nameof(namedArguments));
        }

        [Test]
        public async Task GetArgumentsWhenNoneExist()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class GetArgumentsWhenNoneExist
    {
        [NoArguments]
        public void Foo() { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class NoArgumentsAttribute : Attribute
    { }
}";
            var attribute = await GetAttributeSyntaxAsync(testCode, "GetArgumentsWhenNoneExist").ConfigureAwait(false);
            var (positionalArguments, namedArguments) = attribute.GetArguments();

            Assert.That(positionalArguments.Length, Is.EqualTo(0), nameof(positionalArguments));
            Assert.That(namedArguments.Length, Is.EqualTo(0), nameof(namedArguments));
        }

        private static async Task<AttributeSyntax> GetAttributeSyntaxAsync(string code, string typeName)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(code).ConfigureAwait(false);

            return rootAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Where(_ => _.Identifier.ValueText == typeName).Single()
                .DescendantNodes().OfType<AttributeSyntax>().Single();
        }
    }
}
