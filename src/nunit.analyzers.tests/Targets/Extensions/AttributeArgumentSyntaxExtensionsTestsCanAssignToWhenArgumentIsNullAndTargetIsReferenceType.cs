using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
  public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsNullAndTargetIsReferenceType
  {
    [Arguments(null)]
    public void Foo(object a) { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ArgumentsAttribute
      : Attribute
    {
      public ArgumentsAttribute(object x) { }
    }
  }
}
