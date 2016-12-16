using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

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
	}
}
