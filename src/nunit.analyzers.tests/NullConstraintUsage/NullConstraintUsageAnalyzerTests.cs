using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.NullConstraintUsage.NullConstraintUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.NullConstraintUsage
{
    public class NullConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.NullConstraintUsage);

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task AnalyzeWhenActualIsNonNullableValueType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = default(int);
                        Assert.That(actual, {|#0:{{constraint}}|});
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("int"));
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task AnalyzeWhenActualIsNonNullableValueTypeDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = default(int);
                        Assert.That(() => actual, {|#0:{{constraint}}|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("int"));
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task AnalyzeWhenActualIsTaskDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Assert.That(() => Task.FromResult(3), {|#0:{{constraint}}|});
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("int"));
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task ValidWhenActualIsNullableValueType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        bool? actual = false;
                        Assert.That(actual, {{constraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task ValidWhenActualIsReferenceType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new string[] { "TestString" };
                        Assert.That(actual, {{constraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task ValidWhenActualIsReferenceTypeDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var actual = new string[] { "TestString" };
                        Assert.That(() => actual, {{constraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task ValidWhenUsedWithPropertyOperator()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public struct TestStruct
                    {
                        public string RefTypeProp { get; set; }
                    }

                    [Test]
                    public void TestMethod()
                    {
                        var actual = new TestStruct();
                        Assert.That(actual, Has.Property("RefTypeProp").Null);
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task ValidWhenActualIsTask(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        var task = Task.CompletedTask;
                        Assert.That(task, {{constraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task ValidWhenActualIsAction(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Action<bool> action = b => { };
                        Assert.That(action, {{constraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task ValidWhenActualIsFunc(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Func<bool, bool> function = b => b;
                        Assert.That(function, {{constraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public async Task ValidWhenActualIsTestDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($$"""
                        Action<bool> action = b => { };
                        Assert.That(() => action(true), {{constraint}});
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
