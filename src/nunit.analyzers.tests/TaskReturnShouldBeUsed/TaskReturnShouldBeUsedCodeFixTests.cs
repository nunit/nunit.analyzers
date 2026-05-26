#if NUNIT4
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TaskReturnShouldBeUsed;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TaskReturnShouldBeUsed
{
    [TestFixture]
    internal sealed class TaskReturnShouldBeUsedCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new TaskReturnShouldBeUsedAnalyzer();
        private static readonly CodeFixProvider fix = new TaskReturnShouldBeUsedCodeFix();

        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.TaskReturnShouldBeUsed);

        [Test]
        public void AnalyzeWhenReturnIgnoredInVoidMethod()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            ↓Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
        }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public async Task TestMethod()
        {
            await Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void AnalyzeWhenReturnIgnoredInOverriddenVoidMethod()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public abstract class BaseClass
    {
        [Test]
        public abstract void TestMethod();
    }

    [TestFixture]
    public class TestClass : BaseClass
    {
        [Test]
        public override void TestMethod()
        {
            ↓Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
        }
    }");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public abstract class BaseClass
    {
        [Test]
        public abstract void TestMethod();
    }

    [TestFixture]
    public class TestClass : BaseClass
    {
        [Test]
        public override void TestMethod()
        {
            Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1)).Wait();
        }
    }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void AnalyzeWhenReturnIgnoredInTaskMethod()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public Task TestMethod()
        {
            ↓Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
            return Task.CompletedTask;
        }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public Task TestMethod()
        {
            Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1)).Wait();
            return Task.CompletedTask;
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void AnalyzeWhenReturnIgnoredInAsyncTaskMethod()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public async Task TestMethod()
        {
            await Task.Yield();
            ↓Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
        }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public async Task TestMethod()
        {
            await Task.Yield();
            await Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

#if NUNIT5
        [TestCase("Exception?")]
        [TestCase("var")]
        public void AnalyzeWhenReturnAssignedToVariableDeclaration(string declaredType)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public void TestMethod()
        {{
            {declaredType} exception = ↓Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }}");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        [Test]
        public async Task TestMethod()
        {{
            {declaredType} exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }}");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                settings: Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
        }

        [Test]
        public void AnalyzeWhenReturnAssignedToVariable()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void TestMethod()
        {
            Exception? exception;
            exception = ↓Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public async Task TestMethod()
        {
            Exception? exception;
            exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void AnalyzeWhenReturnAssignedInOverriddenVoidMethod()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public abstract class BaseClass
    {
        [Test]
        public abstract void TestMethod();
    }

    [TestFixture]
    public class TestClass : BaseClass
    {
        [Test]
        public override void TestMethod()
        {
            Exception? exception = ↓Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
        }
    }");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public abstract class BaseClass
    {
        [Test]
        public abstract void TestMethod();
    }

    [TestFixture]
    public class TestClass : BaseClass
    {
        [Test]
        public override void TestMethod()
        {
            Exception? exception = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0)).Result;
        }
    }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
#endif
    }
}
#endif
