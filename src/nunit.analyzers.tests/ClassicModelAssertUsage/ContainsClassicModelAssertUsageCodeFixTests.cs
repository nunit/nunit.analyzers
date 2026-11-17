using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.ClassicModelAssertUsage.ClassicModelAssertUsageAnalyzer, NUnit.Analyzers.ClassicModelAssertUsage.ContainsClassicModelAssertUsageCodeFix>;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class ContainsClassicModelAssertUsageCodeFixTests
    {
        private static readonly DiagnosticResult instanceDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.ContainsUsage);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new ContainsClassicModelAssertUsageCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.ContainsUsage }));
        }

        [Test]
        public async Task VerifyContainsFix()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(instance, collection)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Does.Contain(instance));
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyContainsFixWithMessage()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(instance, collection, "message")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Does.Contain(instance), "message");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyContainsFixWithMessageAndOneArgumentForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(instance, collection, "message-id: {0}", Guid.NewGuid())|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Does.Contain(instance), $"message-id: {Guid.NewGuid()}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyContainsFixWithMessageAndTwoArgumentsForParams()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(instance, collection, "{0}, {1}", "first", "second")|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Does.Contain(instance), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task VerifyContainsFixWithMessageAndArrayParamsInNonstandardOrder()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(args: new[] { "first", "second" }, actual: collection, message: "{0}, {1}", expected: instance)|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(collection, Does.Contain(instance), $"{"first"}, {"second"}");
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
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
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(
                            instance,
                            collection{{commaAndMessage}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection,
                            Does.Contain(instance){{commaAndMessage}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
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
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(
                            instance,
                            collection{{commaAndMessage}}
                        )|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection,
                            Does.Contain(instance){{commaAndMessage}}
                        );
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }

        [Test]
        public async Task CodeFixMaintainsReasonableTriviaWithAllArgumentsOnSameLine([Values] bool newlineBeforeClosingParen)
        {
            var optionalNewline = newlineBeforeClosingParen ? $"{Environment.NewLine}            " : string.Empty;
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        {|#0:ClassicAssert.Contains(
                            instance, collection{{optionalNewline}})|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var instance = new object();
                        var collection = Array.Empty<object>();

                        Assert.That(
                            collection, Does.Contain(instance){{optionalNewline}});
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, instanceDiagnostic.WithLocation(0), fixedCode, ClassicModelAssertUsageCodeFix.TransformToConstraintModelDescription);
        }
    }
}
