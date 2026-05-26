#if NUNIT4
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TaskReturnShouldBeUsed;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TaskReturnShouldBeUsed
{
    [TestFixture]
    internal sealed class TaskReturnShouldBeUsedAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TaskReturnShouldBeUsedAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.TaskReturnShouldBeUsed);

        [Test]
        public void NoDiagnosticsWhenTaskIsAwaited()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public async Task TestMethod()
        {
            await Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
        }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticsWhenTaskIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1)).Wait();
        }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticsWhenTaskIsAssigned()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            Task task = Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
            task.Wait();
        }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenReturnIgnored()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            ↓Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
        }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

#if NUNIT5
        [Test]
        public void NoDiagnosticsWhenTaskResultIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            Exception? exception = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0)).Result;
        }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticsWhenReturnAssignedToTaskVariableDeclaration()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            Task<Exception?> task = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void NoDiagnosticsWhenReturnAssignedToTaskVariable()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            Task<Exception?> task;
            task = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Exception?")]
        [TestCase("var")]
        public void AnalyzeWhenReturnAssignedToVariableDeclaration(string declaredType)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{
            {declaredType} exception = ↓Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }}");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode,
                Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
        }

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        [TestCase("ex", null, null)]            // Default
        [TestCase("exception", "ex", "task")]   // Explicit specified default include and exclude
        [TestCase("invalid", "", null)]         // Raise for any variable name
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
        public void AnalyzeWhenReturnAssignedToIncludedVarDeclaration(string variableName, string? include, string? exclude)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{
            var {variableName} = ↓Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }}");

            Settings settings = Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors);

            if (include is not null || exclude is not null)
            {
                var analyzerConfig = new System.Text.StringBuilder();
                if (include is not null)
                    analyzerConfig.AppendLine($"dotnet_diagnostic.{AnalyzerIdentifiers.TaskReturnShouldBeUsed}.raise_for_var_declaration_containing = {include}");
                if (exclude is not null)
                    analyzerConfig.AppendLine($"dotnet_diagnostic.{AnalyzerIdentifiers.TaskReturnShouldBeUsed}.do_not_raise_for_var_declaration_containing = {exclude}");

                settings = Settings.Default.WithAnalyzerConfig(analyzerConfig.ToString());
            }

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode, settings);
        }

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
        [TestCase("task", null, null)]              // Default
        [TestCase("task", "ex", "task")]            // Explicit specified default include and exclude
        [TestCase("exceptionTask", null, null)]     // Default
        [TestCase("invalid", null, "")]             // Don't raise for any variable name
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
        public void NoDiagnosticsWhenReturnAssignedToExcludedVarDeclaration(string variableName, string? include, string? exclude)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{
            var {variableName} = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }}");

            Settings settings = Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors);

            if (include is not null || exclude is not null)
            {
                var analyzerConfig = new System.Text.StringBuilder();
                if (include is not null)
                    analyzerConfig.AppendLine($"dotnet_diagnostic.{AnalyzerIdentifiers.TaskReturnShouldBeUsed}.raise_for_var_declaration_containing = {include}");
                if (exclude is not null)
                    analyzerConfig.AppendLine($"dotnet_diagnostic.{AnalyzerIdentifiers.TaskReturnShouldBeUsed}.do_not_raise_for_var_declaration_containing = {exclude}");

                settings = Settings.Default.WithAnalyzerConfig(analyzerConfig.ToString());
            }

            RoslynAssert.Valid(analyzer, testCode, settings);
        }

        [Test]
        public void AnalyzeWhenReturnAssignedToVariable()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            Exception? exception;
            exception = ↓Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode,
                Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
        }
#endif
    }
}
#endif
