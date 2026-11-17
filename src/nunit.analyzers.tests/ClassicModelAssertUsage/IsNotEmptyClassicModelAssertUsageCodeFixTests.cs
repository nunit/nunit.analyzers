using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsNotEmptyClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsNotEmptyClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.IsNotEmptyUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsNotEmptyClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsNotEmptyUsage }));
        }

        [Test]
        public async Task VerifyIsNotEmptyFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(collection)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Not.Empty);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotEmptyFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(collection, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Not.Empty, "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotEmptyFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(collection, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Not.Empty, $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotEmptyFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(collection, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Not.Empty, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotEmptyFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(args: new[] { "first", "second" }, collection: collection, message: "{0}, {1}")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Not.Empty, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotEmptyWithImplicitTypeConversionFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private struct MyString
                    {
                        private readonly string _value;

                        public MyString(string value) => _value = value;

                        public static implicit operator string(MyString value) => value._value;
                        public static implicit operator MyString(string value) => new MyString(value);
                    }
                    public void TestMethod()
                    {
                        MyString s = "Hello NUnit";
                        {|#0:ClassicAssert.IsNotEmpty(s)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private struct MyString
                    {
                        private readonly string _value;

                        public MyString(string value) => _value = value;

                        public static implicit operator string(MyString value) => value._value;
                        public static implicit operator MyString(string value) => new MyString(value);
                    }
                    public void TestMethod()
                    {
                        MyString s = "Hello NUnit";
                        Assert.That((string)s, Is.Not.Empty);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsNotEmptyWithImplicitTypeConversionFixInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private struct MyString
                    {
                        private readonly string _value;

                        public MyString(string value) => _value = value;

                        public static implicit operator string(MyString value) => value._value;
                        public static implicit operator MyString(string value) => new MyString(value);
                    }
                    public void TestMethod()
                    {
                        MyString s = "Hello NUnit";
                        {|#0:ClassicAssert.IsNotEmpty(args: new[] { "first", "second" }, aString: s, message: "{0}, {1}")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    private struct MyString
                    {
                        private readonly string _value;

                        public MyString(string value) => _value = value;

                        public static implicit operator string(MyString value) => value._value;
                        public static implicit operator MyString(string value) => new MyString(value);
                    }
                    public void TestMethod()
                    {
                        MyString s = "Hello NUnit";
                        Assert.That((string)s, Is.Not.Empty, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithEndOfLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(
                            collection{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection,
                            Is.Not.Empty{{commaAndMessage}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithNewLineClosingParen([Values] bool hasMessage)
        {
            var commaAndMessage = hasMessage
                ? $",{Environment.NewLine}                \"message\""
                : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(
                            collection{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection,
                            Is.Not.Empty{{commaAndMessage}}
                        );
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsNotEmpty(
                            collection, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection, Is.Not.Empty, "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
