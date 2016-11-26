using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
	public sealed class AttributeSyntaxExtensionsTestsGetArguments
	{
		[An(34, AProperty = 22d, BProperty = 33d)]
		public void Foo() { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class AnAttribute
		: Attribute
	{
		public AnAttribute(int x) { }

		public double AProperty { get; set; }

		public double BProperty { get; set; }
	}
}
