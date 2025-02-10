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
        [Test]
        public async Task IsAssignableFromWhenThisIsNull()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IsAssignableFromWhenThisIsNull { }
}";
            var types = await GetTypeSymbolAsync(
                testCode,
                ["IsAssignableFromWhenThisIsNull"]).ConfigureAwait(false);
            var other = types[0];

            Assert.That(default(ITypeSymbol).IsAssignableFrom(other), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsNull()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IsAssignableFromWhenOtherIsNull { }
}";
            var types = await GetTypeSymbolAsync(
                testCode,
                ["IsAssignableFromWhenOtherIsNull"]).ConfigureAwait(false);
            var instance = types[0];

            Assert.That(instance.IsAssignableFrom(default(ITypeSymbol)), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsSameTypeAsThis()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IsAssignableFromWhenOtherIsSameTypeAsThis { }
}";
            var types = await GetTypeSymbolAsync(
                testCode,
                ["IsAssignableFromWhenOtherIsSameTypeAsThis"]).ConfigureAwait(false);
            var instance = types[0];
            var other = instance;

            Assert.That(instance.IsAssignableFrom(other), Is.True);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsInDifferentAssembly()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IsAssignableFromWhenOtherIsInDifferentAssembly { }
}";
            var types1 = await GetTypeSymbolAsync(
                testCode,
                ["IsAssignableFromWhenOtherIsInDifferentAssembly"]).ConfigureAwait(false);
            var instance = types1[0];
            var types2 = await GetTypeSymbolAsync(
                testCode,
                ["IsAssignableFromWhenOtherIsInDifferentAssembly"]).ConfigureAwait(false);
            var other = types2[0];

            Assert.That(instance.IsAssignableFrom(other), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsASubclass()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public class IsAssignableFromWhenOtherIsASubclassBase { }

    public class IsAssignableFromWhenOtherIsASubclassSub
       : IsAssignableFromWhenOtherIsASubclassBase
    { }
}";
            var types = await GetTypeSymbolAsync(
                testCode,
                [
                    "IsAssignableFromWhenOtherIsASubclassBase",
                    "IsAssignableFromWhenOtherIsASubclassSub"
                ]).ConfigureAwait(false);

            Assert.That(types[0].IsAssignableFrom(types[1]), Is.True);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherIsNotASubclass()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public class IsAssignableFromWhenOtherIsNotASubclassA { }

    public class IsAssignableFromWhenOtherIsNotASubclassB { }
}";
            var types = await GetTypeSymbolAsync(
                testCode,
                [
                    "IsAssignableFromWhenOtherIsNotASubclassA",
                    "IsAssignableFromWhenOtherIsNotASubclassB"
                ]).ConfigureAwait(false);

            Assert.That(types[0].IsAssignableFrom(types[1]), Is.False);
        }

        [Test]
        public async Task IsAssignableFromWhenOtherImplementsInterface()
        {
            var testCode = @"
namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public interface IsAssignableFromWhenOtherImplementsInterface { }

    public class IsAssignableFromWhenOtherImplementsInterfaceType
        : IsAssignableFromWhenOtherImplementsInterface
    { }
}";
            var types = await GetTypeSymbolAsync(
                testCode,
                [
                    "IsAssignableFromWhenOtherImplementsInterface",
                    "IsAssignableFromWhenOtherImplementsInterfaceType"
                ]).ConfigureAwait(false);

            Assert.That(types[0].IsAssignableFrom(types[1]), Is.True);
        }

        [Test]
        public async Task IsAssertWhenSymbolIsNotInNUnitAssembly()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IsAssertWhenSymbolIsNotInNUnitAssembly
    {
        public Guid x;
    }
}";
            var typeSymbol = await GetTypeSymbolFromFieldAsync(testCode, "IsAssertWhenSymbolIsNotInNUnitAssembly").ConfigureAwait(false);
            Assert.That(typeSymbol.IsAssert(), Is.False);
        }

        [Test]
        public async Task IsAssertWhenSymbolIsInNUnitAssemblyAndNotAssertType()
        {
            var testCode = @"
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IsAssertWhenSymbolIsInNUnitAssemblyAndNotAssertType
    {
        public Is x;
    }
}";
            var typeSymbol = await GetTypeSymbolFromFieldAsync(testCode, "IsAssertWhenSymbolIsInNUnitAssemblyAndNotAssertType").ConfigureAwait(false);
            Assert.That(typeSymbol.IsAssert(), Is.False);
        }

        [Test]
        public async Task IsAssertWhenSymbolIsAssertType()
        {
            var testCode = @"
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class IsAssertWhenSymbolIsAssertType
    {
        public Assert x;
    }
}";
            var typeSymbol = await GetTypeSymbolFromFieldAsync(testCode, "IsAssertWhenSymbolIsAssertType").ConfigureAwait(false);
            Assert.That(typeSymbol.IsAssert(), Is.True);
        }

        private static async Task<ImmutableArray<ITypeSymbol>> GetTypeSymbolAsync(string code, string[] typeNames)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(code).ConfigureAwait(false);

            var types = new List<ITypeSymbol>();

            foreach (var typeName in typeNames)
            {
                TypeDeclarationSyntax typeDeclaration = rootAndModel.Node
                    .DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .Where(t => t.Identifier.ValueText == typeName)
                    .Single();

                INamedTypeSymbol? item = rootAndModel.Model.GetDeclaredSymbol(typeDeclaration);
                if (item is not null)
                    types.Add(item);
            }

            return types.ToImmutableArray();
        }

        private static async Task<ITypeSymbol> GetTypeSymbolFromFieldAsync(string code, string typeName)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(code).ConfigureAwait(false);

            var fieldNode = rootAndModel.Node
                .DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Where(_ => _.Identifier.ValueText == typeName).Single()
                .DescendantNodes().OfType<FieldDeclarationSyntax>().Single();

            VariableDeclaratorSyntax variableDeclaration = fieldNode.Declaration.Variables[0];
            IFieldSymbol? symbol = rootAndModel.Model.GetDeclaredSymbol(variableDeclaration) as IFieldSymbol;
            Assert.That(symbol, Is.Not.Null, $"Cannot find symbol for {variableDeclaration.Identifier}");
            Assert.That(symbol.Type.Kind, Is.Not.EqualTo(SymbolKind.ErrorType), $"Cannot find type for {fieldNode}");

            return symbol.Type;
        }
    }
}
