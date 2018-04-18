using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenParameterIsTimeSpanAndArgumentIsValidString
    {
        [Arguments("00:03:00")]
        public void Foo(TimeSpan a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute
          : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}
