using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.IsEmptyClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class IsEmptyClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.IsEmptyUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new IsEmptyClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.IsEmptyUsage }));
        }

        [Test]
        public async Task VerifyIsEmptyFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsEmpty(collection)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Empty);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsEmptyFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsEmpty(collection, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Empty, "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsEmptyFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsEmpty(collection, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Empty, $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsEmptyFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsEmpty(collection, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Empty, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsEmptyFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.IsEmpty(args: new[] { "first", "second" }, message: "{0}, {1}", collection: collection)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Is.Empty, $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyIsEmptyWithImplicitTypeConversionFix()
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
                        MyString s = string.Empty;
                        {|#0:ClassicAssert.IsEmpty(s)|};
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
                        MyString s = string.Empty;
                        Assert.That((string)s, Is.Empty);
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

                        {|#0:ClassicAssert.IsEmpty(
                            collection{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection,
                            Is.Empty{{commaAndMessage}});
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

                        {|#0:ClassicAssert.IsEmpty(
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
                            Is.Empty{{commaAndMessage}}
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

                        {|#0:ClassicAssert.IsEmpty(
                            collection, "message"{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection, Is.Empty, "message"{{optionalNewline}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
