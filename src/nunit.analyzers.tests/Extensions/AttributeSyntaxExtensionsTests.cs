using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

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
			var attribute = await this.GetAttributeSyntaxAsync(
				$"{AttributeSyntaxExtensionsTests.BasePath}{(nameof(this.GetArguments))}.cs",
				$"{nameof(AttributeSyntaxExtensionsTests)}{nameof(GetArguments)}");
			var arguments = attribute.GetArguments();

			Assert.That(arguments.Item1.Length, Is.EqualTo(1), nameof(arguments.Item1));
			Assert.That(arguments.Item2.Length, Is.EqualTo(2), nameof(arguments.Item2));
		}

		private async Task<AttributeSyntax> GetAttributeSyntaxAsync(string file, string typeName)
		{
			var rootAndModel = await TestHelpers.GetRootAndModel(file);

			return rootAndModel.Item1
				.DescendantNodes().OfType<TypeDeclarationSyntax>()
				.Where(_ => _.Identifier.ValueText == typeName).Single()
				.DescendantNodes().OfType<AttributeSyntax>().Single();
		}
	}
}
