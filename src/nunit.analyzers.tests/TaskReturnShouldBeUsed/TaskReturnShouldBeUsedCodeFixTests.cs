#if NUNIT4
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.TaskReturnShouldBeUsed.TaskReturnShouldBeUsedAnalyzer, NUnit.Analyzers.TaskReturnShouldBeUsed.TaskReturnShouldBeUsedCodeFix>;

namespace NUnit.Analyzers.Tests.TaskReturnShouldBeUsed
{
    [TestFixture]
    internal sealed class TaskReturnShouldBeUsedCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.TaskReturnShouldBeUsed);

        [Test]
        public async Task AnalyzeWhenReturnIgnoredInVoidMethod()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        {|#0:Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1))|};
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public async Task TestMethod()
                    {
                        await Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThatAsync"), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenReturnIgnoredInOverriddenVoidMethod()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                        {|#0:Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1))|};
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThatAsync"), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenReturnIgnoredInTaskMethod()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public Task TestMethod()
                    {
                        {|#0:Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1))|};
                        return Task.CompletedTask;
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public Task TestMethod()
                    {
                        Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1)).Wait();
                        return Task.CompletedTask;
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThatAsync"), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenReturnIgnoredInAsyncTaskMethod()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public async Task TestMethod()
                    {
                        await Task.Yield();
                        {|#0:Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1))|};
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public async Task TestMethod()
                    {
                        await Task.Yield();
                        await Assert.ThatAsync(() => Task.FromResult(0), Is.LessThan(1));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThatAsync"), fixedCode);
        }

#if NUNIT5
        [TestCase("Exception?")]
        [TestCase("var")]
        public async Task AnalyzeWhenReturnAssignedToVariableDeclaration(string declaredType)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public void TestMethod()
                    {
                        {{declaredType}} exception = {|#0:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|};
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [Test]
                    public async Task TestMethod()
                    {
                        {{declaredType}} exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"), fixedCode, fixTitle: null, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
        }

        [Test]
        public async Task AnalyzeWhenReturnAssignedToVariable()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void TestMethod()
                    {
                        Exception? exception;
                        exception = {|#0:{|CS0029:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|}|};
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public async Task TestMethod()
                    {
                        Exception? exception;
                        exception = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
                    }
            """);

            // The code to fix does not compile (CS0029), as the Task is assigned to an Exception.
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenReturnAssignedInOverriddenVoidMethod()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                        Exception? exception = {|#0:{|CS0029:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|}|};
                    }
                }
            """);

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            // The code to fix does not compile (CS0029), as the Task is assigned to an Exception.
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"), fixedCode);
        }

        [Test]
        public async Task AnalyzeWhenReturned()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public Exception? AssertAsync()
                    {
                        return {|#0:{|CS0029:Assert.ThrowsAsync<Exception>(() => Task.FromResult(0))|}|};
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async Task<Exception?> AssertAsync()
                    {
                        return await Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
                    }
            """);

            // The code to fix does not compile (CS0029), as the Task is assigned to an Exception.
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("ThrowsAsync"), fixedCode);
        }
#endif
    }
}
#endif
