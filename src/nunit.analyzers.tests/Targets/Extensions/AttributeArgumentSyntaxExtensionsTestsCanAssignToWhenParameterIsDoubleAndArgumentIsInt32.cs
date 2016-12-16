using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
	public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenParameterIsDoubleAndArgumentIsInt32
	{
		[Arguments(3)]
		public void Foo(double a) { }

		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		public sealed class ArgumentsAttribute
			: Attribute
		{
			public ArgumentsAttribute(int a) { }
		}
	}
}
