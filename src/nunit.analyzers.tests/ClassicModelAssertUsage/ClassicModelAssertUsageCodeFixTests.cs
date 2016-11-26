using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
	[TestFixture]
	public sealed class ClassicModelAssertUsageCodeFixTests
	{
		[Test]
		public void GetFixAllProvider()
		{
			var codeFix = new TestableClassicModelAssertUsageCodeFix();
			Assert.That(codeFix.GetFixAllProvider(), Is.EqualTo(WellKnownFixAllProviders.BatchFixer));
		}

		private sealed class TestableClassicModelAssertUsageCodeFix
			: ClassicModelAssertUsageCodeFix
		{
			public override ImmutableArray<string> FixableDiagnosticIds
			{
				get
				{
					return new ImmutableArray<string>();
				}
			}

			protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments) { }
		}
	}
}