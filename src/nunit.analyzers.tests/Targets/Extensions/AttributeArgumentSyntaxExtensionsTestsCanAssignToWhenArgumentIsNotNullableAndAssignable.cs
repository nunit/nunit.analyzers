using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
	public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsNotNullableAndAssignable
	{
		[Arguments("x")]
		public void Foo(object a) { }

		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		public sealed class ArgumentsAttribute
			: Attribute
		{
			public ArgumentsAttribute(string x) { }
		}
	}
}
