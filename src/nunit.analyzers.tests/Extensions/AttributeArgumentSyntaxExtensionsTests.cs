using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Extensions;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Extensions
{
    [TestFixture]
    public sealed class AttributeArgumentSyntaxExtensionsTests
    {
        [Test]
        public async Task CanAssignToWhenArgumentIsNullAndTargetIsReferenceType()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenArgumentIsNullAndTargetIsReferenceType
    {
        [Arguments(null)]
        public void Foo(object a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(object x) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNullAndTargetIsNullableType()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenArgumentIsNullAndTargetIsNullableType
    {
        [Arguments(null)]
        public void Foo(int? a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(object x) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNullAndTargetIsValueType()
        {
            var testCode = @"
using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenArgumentIsNullAndTargetIsValueType
    {
        [Arguments(null)]
        public void Foo(int a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(object x) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNotNullableAndAssignable()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenArgumentIsNotNullableAndAssignable
    {
        [Arguments(""x"")]
        public void Foo(object a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string x) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNullableAndAssignable()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenArgumentIsNullableAndAssignable
    {
        [Arguments(3)]
        public void Foo(int a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(int? x) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsNotAssignable()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenArgumentIsNotAssignable
    {
        [Arguments(""x"")]
        public void Foo(Guid a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsInt16AndArgumentIsInt32()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsInt16AndArgumentIsInt32
    {
        [Arguments(3)]
        public void Foo(short a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(int a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsByteAndArgumentIsInt32()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsByteAndArgumentIsInt32
    {
        [Arguments(3)]
        public void Foo(byte a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(int a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsSByteAndArgumentIsInt32()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsSByteAndArgumentIsInt32
    {
        [Arguments(3)]
        public void Foo(sbyte a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(int a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDoubleAndArgumentIsInt32()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsDoubleAndArgumentIsInt32
    {
        [Arguments(3)]
        public void Foo(double a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(int a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsDouble()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsDecimalAndArgumentIsDouble
    {
        [Arguments(3d)]
        public void Foo(decimal a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(double a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsValidString()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsDecimalAndArgumentIsValidString
    {
        [Arguments(""3"")]
        public void Foo(decimal a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsInvalidString()
        {
            var testCode = @"
using NUnit.Framework;
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsDecimalAndArgumentIsInvalidString
    {
        [Arguments(""x"")]
        public void Foo(decimal a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDecimalAndArgumentIsInt32()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsDecimalAndArgumentIsInt32
    {
        [Arguments(3)]
        public void Foo(decimal a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(int a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsNullableInt64AndArgumentIsInt32()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsNullableInt64AndArgumentIsInt32
    {
        [Arguments(3)]
        public void Foo(long? a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(int a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDateTimeAndArgumentIsValidString()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsDateTimeAndArgumentIsValidString
    {
        [Arguments(""1/1/2000"")]
        public void Foo(DateTime a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute
            : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsDateTimeAndArgumentIsInvalidString()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsDateTimeAndArgumentIsInvalidString
    {
        [Arguments(""x"")]
        public void Foo(DateTime a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsTimeSpanAndArgumentIsValidString()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsTimeSpanAndArgumentIsValidString
    {
        [Arguments(""00:03:00"")]
        public void Foo(TimeSpan a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenParameterIsTimeSpanAndArgumentIsInvalidString()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenParameterIsTimeSpanAndArgumentIsInvalidString
    {
        [Arguments(""x"")]
        public void Foo(TimeSpan a) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string a) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.False);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsImplicitlyTypedArrayAndAssignable()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class CanAssignToWhenArgumentIsImplicitlyTypedArrayAndAssignable
    {
        [Arguments(new[] { ""a"", ""b"", ""c"" })]
        public void Foo(string[] inputs) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string[] x) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.True);
        }

        [Test]
        public async Task CanAssignToWhenArgumentIsImplicitlyTypedArrayAndNotAssignable()
        {
            var testCode = @"
using System;

namespace NUnit.Analyzers.Tests.Targets.Extensions
{
    public sealed class AttributeArgumentSyntaxExtensionsTestsCanAssignToWhenArgumentIsImplicitlyTypedArrayAndNotAssignable
    {
        [Arguments(new[] { ""a"", ""b"", ""c"" })]
        public void Foo(int[] inputs) { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class ArgumentsAttribute : Attribute
        {
            public ArgumentsAttribute(string[] x) { }
        }
    }
}";
            var values = await AttributeArgumentSyntaxExtensionsTests.GetAttributeSyntaxAsync(testCode);

            Assert.That(values.Syntax.CanAssignTo(values.TypeSymbol, values.Model), Is.False);
        }

        private async static Task<(AttributeArgumentSyntax Syntax, ITypeSymbol TypeSymbol, SemanticModel Model)> GetAttributeSyntaxAsync(string code)
        {
            var rootAndModel = await TestHelpers.GetRootAndModelFromString(code);

            // It's assumed the code will have one attribute with one argument,
            // along with one method with one parameter
            var attributeArgumentSyntax = rootAndModel.Node.DescendantNodes().OfType<AttributeSyntax>().Single(
                    _ => _.Name.ToFullString() == "Arguments")
                    .DescendantNodes().OfType<AttributeArgumentSyntax>().Single();
            var typeSymbol = rootAndModel.Model.GetDeclaredSymbol(
                    rootAndModel.Node.DescendantNodes().OfType<MethodDeclarationSyntax>().Single()).Parameters[0].Type;

            return (attributeArgumentSyntax, typeSymbol, rootAndModel.Model);
        }
    }
}
