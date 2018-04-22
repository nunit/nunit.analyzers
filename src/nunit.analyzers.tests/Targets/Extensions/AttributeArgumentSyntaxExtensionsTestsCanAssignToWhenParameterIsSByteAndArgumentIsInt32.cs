using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenParameterIsSByteAndArgumentIsInt32
    {
        [Arguments(3)]
        public void Foo(sbyte a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute
            : Attribute
        {
            public ArgumentsAttribute(int a) { }
        }
    }
}
