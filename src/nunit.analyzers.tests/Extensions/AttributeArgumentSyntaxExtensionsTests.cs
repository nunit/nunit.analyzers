using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Tests.Targets.Extensions;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Extensions
{
    [TestFixture]
    public sealed class AttributeArgumentSyntaxExtensionsTests
    {
        private static readonly string BasePath =
            $@"{TestContext.CurrentContext.TestDirectory}\Targets\Extensions\{nameof(AttributeArgumentSyntaxExtensionsTests)}";

        [Test]
        public async Task CanAssignToWhenArgumentIsNullAndTargetIsReferenceType()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenArgumentIsNullAndTargetIsReferenceType))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNullAndTargetIsNullableType()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenArgumentIsNullAndTargetIsNullableType))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNullAndTargetIsValueType()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenArgumentIsNullAndTargetIsValueType))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNotNullableAndAssignable()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenArgumentIsNotNullableAndAssignable))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNullableAndAssignable()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenArgumentIsNullableAndAssignable))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNotAssignable()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenArgumentIsNotAssignable))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsInt16AndArgumentIsInt32()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsInt16AndArgumentIsInt32))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsByteAndArgumentIsInt32()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsByteAndArgumentIsInt32))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsSByteAndArgumentIsInt32()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsSByteAndArgumentIsInt32))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDoubleAndArgumentIsInt32()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsDoubleAndArgumentIsInt32))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsDouble()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsDecimalAndArgumentIsDouble))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsValidString()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsDecimalAndArgumentIsValidString))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsInvalidString()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsDecimalAndArgumentIsInvalidString))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsInt32()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsDecimalAndArgumentIsInt32))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsNullableInt64AndArgumentIsInt32()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsNullableInt64AndArgumentIsInt32))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDateTimeAndArgumentIsValidString()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsDateTimeAndArgumentIsValidString))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDateTimeAndArgumentIsInvalidString()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsDateTimeAndArgumentIsInvalidString))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsTimeSpanAndArgumentIsValidString()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsTimeSpanAndArgumentIsValidString))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsTimeSpanAndArgumentIsInvalidString()
        {
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(
                $"{AttributeArgumentSyntaxExtensionsTests.BasePath}{(nameof(this.CanAssignToWhenParameterIsTimeSpanAndArgumentIsInvalidString))}.cs");

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.False);
        }

        private async static Task<Tuple<AttributeArgumentSyntax, ITypeSymbol, SemanticModel>> GetAttributeSyntaxAsync(string file)
        {
            var rootAndModel = await TestHelpers.GetRootAndModel(file);

            // It's assumed the code will have one attribute with one argument,
            // along with one method with one parameter
            return new Tuple<AttributeArgumentSyntax, ITypeSymbol, SemanticModel>(
                rootAndModel.Item1.DescendantNodes().OfType<AttributeSyntax>().Single(
                    _ => _.Name.ToFullString() == "Arguments")
                    .DescendantNodes().OfType<AttributeArgumentSyntax>().Single(),
                rootAndModel.Item2.GetDeclaredSymbol(
                    rootAndModel.Item1.DescendantNodes().OfType<MethodDeclarationSyntax>().Single()).Parameters[0].Type,
                rootAndModel.Item2);
        }

        private async static Task<(AttributeArgumentSyntax Syntax, ITypeSymbol Symbol, SemanticModel Model)> GetAttributeSyntaxAsync<T>()
        {
            var file = GetTestFileName<T>();
            var rootAndModel = await TestHelpers.GetRootAndModel(file);

            // It's assumed the code will have one attribute with one argument,
            // along with one method with one parameter
            return (rootAndModel.Item1.DescendantNodes().OfType<AttributeSyntax>().Single(
                    _ => _.Name.ToFullString() == "Arguments")
                    .DescendantNodes().OfType<AttributeArgumentSyntax>().Single(),
                rootAndModel.Item2.GetDeclaredSymbol(
                    rootAndModel.Item1.DescendantNodes().OfType<MethodDeclarationSyntax>().Single()).Parameters[0].Type,
                rootAndModel.Item2);
        }


        [Test]
        public async Task CanAssignToWhenArgumentIsImplicitlyTypedArrayAndAssignable()
        {
            //option 1
            var fileName = GetTestFileName<CanAssignToWhenArgumentIsImplicitlyTypedArrayAndAssignable>();
            var values = await GetAttributeSyntaxAsync(fileName);

            Assert.That(values.Item1.CanAssignTo(values.Item2, values.Item3), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsImplicitlyTypedArrayAndNotAssignable()
        {
            // option 2
            var values = await GetAttributeSyntaxAsync<CanAssignToWhenArgumentIsImplicitlyTypedArrayAndNotAssignable>();
            Assert.That(values.Syntax.CanAssignTo(values.Symbol, values.Model), Is.False);
        }

        private static string GetTestFileName<T>()
        {
            return Path.Combine(BasePath, typeof(T).Name.ToString() + ".cs");
        }
    }
}
