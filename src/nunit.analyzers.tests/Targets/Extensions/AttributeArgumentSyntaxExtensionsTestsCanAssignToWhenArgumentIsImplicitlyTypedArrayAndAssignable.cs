using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsImplicitlyTypedArrayAndAssignable
    {
        [Arguments(new[] { "a", "b", "c" })]
        public void Foo(string[] inputs) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute
            : Attribute
        {
            public ArgumentsAttribute(string[] x) { }
        }
    }
}
