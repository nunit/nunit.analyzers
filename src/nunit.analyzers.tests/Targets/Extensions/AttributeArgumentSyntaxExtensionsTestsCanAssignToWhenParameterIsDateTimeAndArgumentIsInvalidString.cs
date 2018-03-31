using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
  public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenParameterIsDateTimeAndArgumentIsInvalidString
  {
    [Arguments("x")]
    public void Foo(DateTime a) { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ArgumentsAttribute
      : Attribute
    {
      public ArgumentsAttribute(string a) { }
    }
  }
}
