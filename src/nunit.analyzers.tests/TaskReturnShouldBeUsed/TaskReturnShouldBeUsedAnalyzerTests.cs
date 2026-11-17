#if NUNIT4
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TaskReturnShouldBeUsed;
using NUnit.Analyzers.Tests.Verifiers;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TaskReturnShouldBeUsed.TaskReturnShouldBeUsedAnalyzer>;

namespace NUnit.Analyzers.Tests.TaskReturnShouldBeUsed
{
    [TestFixture]
    internal sealed class TaskReturnShouldBeUsedAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.TaskReturnShouldBeUsed);

        [Test]
        public async Task NoDiagnosticsWhenTaskIsAwaited()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public async Task TestMethod()
                    {
                        await Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticsWhenTaskIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1)).Wait();
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticsWhenTaskIsAssigned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        Task task = Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
                        task.Wait();
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticsWhenTaskIsReturned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public Task AssertAsync()
                    {
                        return Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenReturnIgnored()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        {|#0:Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1))|};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("ThatAsync"));
        }

#if NUNIT5
        [Test]
        public async Task NoDiagnosticsWhenTaskResultIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        Exception? exception = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0)).Result;
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticsWhenReturnAssignedToTaskVariableDeclaration()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        Task<Exception?> task = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task NoDiagnosticsWhenReturnAssignedToTaskVariable()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        Task<Exception?> task;
                        task = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("Exception?")]
        [TestCase("var")]
        public async Task AnalyzeWhenReturnAssignedToVariableDeclaration(string declaredType)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod()
                    {
                        {{declaredType}} exception = {|#0:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None, expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"));
        }

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        [TestCase("ex", null, null)]            // Default
        [TestCase("exception", "ex", "task")]   // Explicit specified default include and exclude
        [TestCase("invalid", "", null)]         // Raise for any variable name
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
        public async Task AnalyzeWhenReturnAssignedToIncludedVarDeclaration(string variableName, string? include, string? exclude)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod()
                    {
                        var {{variableName}} = {|#0:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => AddAnalyzerConfig(test, include, exclude), expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"));
        }

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        [TestCase("task", null, null)]              // Default
        [TestCase("task", "ex", "task")]            // Explicit specified default include and exclude
        [TestCase("exceptionTask", null, null)]     // Default
        [TestCase("invalid", null, "")]             // Don't raise for any variable name
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
        public async Task NoDiagnosticsWhenReturnAssignedToExcludedVarDeclaration(string variableName, string? include, string? exclude)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod()
                    {
                        var {{variableName}} = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => AddAnalyzerConfig(test, include, exclude));
        }

        [Test]
        public async Task AnalyzeWhenReturnAssignedToVariable()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        Exception? exception;
                        exception = {|#0:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None, expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"));
        }

        [Test]
        public async Task AnalyzeWhenReturned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public Exception? AssertAsync()
                    {
                        return {|#0:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|};
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None, expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"));
        }
#endif

        private static void AddAnalyzerConfig(NUnitCSharpAnalyzerTest<TaskReturnShouldBeUsedAnalyzer> test, string? include, string? exclude)
        {
            var analyzerConfig = new StringBuilder();
            if (include is not null)
            {
                analyzerConfig.AppendLine(FormattableString.Invariant(
                    $"dotnet_diagnostic.{AnalyzerIdentifiers.TaskReturnShouldBeUsed}.raise_for_var_declaration_containing = {include}"));
            }

            if (exclude is not null)
            {
                analyzerConfig.AppendLine(FormattableString.Invariant(
                    $"dotnet_diagnostic.{AnalyzerIdentifiers.TaskReturnShouldBeUsed}.do_not_raise_for_var_declaration_containing = {exclude}"));
            }

            if (analyzerConfig.Length > 0)
            {
                test.TestState.AddAnalyzerConfig(analyzerConfig.ToString());
            }
        }
    }
}
#endif
