using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeSyntaxExtensionsTestsGetArgumentsWhenNoneExist
    {
        [NoArguments]
        public void Foo() { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class NoArgumentsAttribute
        : Attribute
    { }
}
