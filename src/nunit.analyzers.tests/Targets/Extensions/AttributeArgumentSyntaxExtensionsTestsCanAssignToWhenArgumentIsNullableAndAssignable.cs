using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsNullableAndAssignable
    {
        [Arguments(3)]
        public void Foo(int a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute
          : Attribute
        {
            public ArgumentsAttribute(int? x) { }
        }
    }
}
