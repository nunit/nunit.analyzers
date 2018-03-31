using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
  public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsNotAssignable
  {
    [Arguments("x")]
    public void Foo(Guid a) { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ArgumentsAttribute
      : Attribute
    {
      public ArgumentsAttribute(string a) { }
    }
  }
}
