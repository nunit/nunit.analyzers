using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsImplicitlyTypedArrayAndNotAssignable
    {
        [Arguments(new[] { "a", "b", "c" })]
        public void Foo(int[] inputs) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute
          : Attribute
        {
            public ArgumentsAttribute(string[] x) { }
        }
    }
}
