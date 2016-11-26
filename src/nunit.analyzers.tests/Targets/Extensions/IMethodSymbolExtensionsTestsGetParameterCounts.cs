using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
	public sealed class IMethodSymbolExtensionsTestsGetParameterCounts
	{
		public void Foo(int a1, int a2, int a3, string b1 = "b1", string b2 = "b2", params char[] c) { }
	}
}
