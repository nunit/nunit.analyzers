using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsNullAndTargetIsNullableType
    {
        [Arguments(null)]
        public void Foo(int? a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute
          : Attribute
        {
            public ArgumentsAttribute(object x) { }
        }
    }
}
