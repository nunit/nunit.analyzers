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
            var attribute = await AttributeSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode, "GetArguments");
            var arguments = attribute.GetArguments();

            Assert.That(arguments.Item1.Length, Is.EqualTo(1), nameof(arguments.Item1));
            Assert.That(arguments.Item2.Length, Is.EqualTo(2), nameof(arguments.Item2));
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
            var attribute = await AttributeSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode, "GetArgumentsWhenNoneExist");
            var arguments = attribute.GetArguments();

            Assert.That(arguments.Item1.Length, Is.EqualTo(0), nameof(arguments.Item1));
            Assert.That(arguments.Item2.Length, Is.EqualTo(0), nameof(arguments.Item2));
        }

        private async static Task<AttributeSyntax> GetAttributeSyntaxAsync(string code, string typeName)
        {
            var rootAndModel = await TestHelpers.GetRootAndModelFromString(code);

            return rootAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Where(_ => _.Identifier.ValueText == typeName).Single()
                .DescendantNodes().OfType<AttributeSyntax>().Single();
        }
    }
}
