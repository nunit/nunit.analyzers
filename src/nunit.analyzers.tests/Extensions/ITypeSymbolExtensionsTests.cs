using System.Collections.Generic;
using System.Collections.Immutable;
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
    public sealed class ITypeSymbolExtensionsTests
    {
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\Extensions\{nameof(ITypeSymbolExtensionsTests)}";

        [Test]
        public async Task IsAssignableFromWhenThisIsNull()
        {
            var other = (await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenThisIsNull))}.cs",
                new[] { $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenThisIsNull)}" }))[0];

            Assert.That((null as ITypeSymbol).IsAssignableFrom(other), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsNull()
        {
            var @this = (await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenOtherIsNull))}.cs",
                new[] { $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsNull)}" }))[0];

            Assert.That(@this.IsAssignableFrom(null as ITypeSymbol), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsSameTypeAsThis()
        {
            var @this = (await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenOtherIsSameTypeAsThis))}.cs",
                new[] { $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsSameTypeAsThis)}" }))[0];
            var other = @this;

            Assert.That(@this.IsAssignableFrom(other), Is.True);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsInDifferentAssembly()
        {
            var @this = (await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenOtherIsInDifferentAssembly))}.cs",
                new[] { $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsInDifferentAssembly)}" }))[0];
            var other = (await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenOtherIsInDifferentAssembly))}.cs",
                new[] { $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsInDifferentAssembly)}" }))[0];

            Assert.That(@this.IsAssignableFrom(other), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsASubclass()
        {
            var types = await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenOtherIsASubclass))}.cs",
                new[]
                {
                    $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsASubclass)}Base",
                    $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsASubclass)}Sub"
                });

            Assert.That(types[0].IsAssignableFrom(types[1]), Is.True);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsNotASubclass()
        {
            var types = await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenOtherIsNotASubclass))}.cs",
                new[]
                {
                    $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsNotASubclass)}A",
                    $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherIsNotASubclass)}B"
              });

            Assert.That(types[0].IsAssignableFrom(types[1]), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherImplementsInterface()
        {
            var types = await this.GetTypeSymbolAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssignableFromWhenOtherImplementsInterface))}.cs",
                new[]
                {
                    $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherImplementsInterface)}",
                    $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssignableFromWhenOtherImplementsInterface)}Type"
                });

            Assert.That(types[0].IsAssignableFrom(types[1]), Is.True);
        }

        [Test]
        public async Task IsAssertWhenSymbolIsNotInNUnitAssembly()
        {
            Assert.That((await this.GetTypeSymbolFromFieldAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssertWhenSymbolIsNotInNUnitAssembly))}.cs",
                $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssertWhenSymbolIsNotInNUnitAssembly)}")).IsAssert(), Is.False);
        }

        [Test]
        public async Task IsAssertWhenSymbolIsInNUnitAssemblyAndNotAssertType()
        {
            Assert.That((await this.GetTypeSymbolFromFieldAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssertWhenSymbolIsInNUnitAssemblyAndNotAssertType))}.cs",
                $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssertWhenSymbolIsInNUnitAssemblyAndNotAssertType)}")).IsAssert(), Is.False);
        }

        [Test]
        public async Task IsAssertWhenSymbolIsAssertType()
        {
            Assert.That((await this.GetTypeSymbolFromFieldAsync(
                $"{ITypeSymbolExtensionsTests.BasePath}{(nameof(this.IsAssertWhenSymbolIsAssertType))}.cs",
                $"{nameof(ITypeSymbolExtensionsTests)}{nameof(IsAssertWhenSymbolIsAssertType)}")).IsAssert(), Is.True);
        }

        private async Task<ImmutableArray<ITypeSymbol>> GetTypeSymbolAsync(string file, string[] typeNames)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(file);

            var types = new List<ITypeSymbol>();

            foreach (var typeName in typeNames)
            {
                types.Add(rootAndModel.Model.GetDeclaredSymbol(rootAndModel.Node
                    .DescendantNodes().OfType<TypeDeclarationSyntax>()
                    .Where(_ => _.Identifier.ValueText == typeName).Single()));
            }

            return types.ToImmutableArray();
        }

        private async Task<ITypeSymbol> GetTypeSymbolFromFieldAsync(string file, string typeName)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(file);

            var fieldNode = rootAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Where(_ => _.Identifier.ValueText == typeName).Single()
                .DescendantNodes().OfType<FieldDeclarationSyntax>().Single();

            return (rootAndModel.Model.GetDeclaredSymbol(fieldNode.Declaration.Variables[0]) as IFieldSymbol).Type;
        }
    }
}
