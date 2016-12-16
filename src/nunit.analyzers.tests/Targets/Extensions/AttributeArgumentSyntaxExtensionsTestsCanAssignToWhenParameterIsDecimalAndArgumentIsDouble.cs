using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
	public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenParameterIsDecimalAndArgumentIsDouble
	{
		[Arguments(3d)]
		public void Foo(decimal a) { }

		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		public sealed class ArgumentsAttribute
			: Attribute
		{
			public ArgumentsAttribute(double a) { }
		}
	}
}
