#if NUNIT4
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseAssertEnterMultipleScope;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseAssertEnterMultipleScope
{
    [TestFixture]
    public sealed class UseAssertEnterMultipleScopeCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UseAssertEnterMultipleScopeAnalyzer();
        private static readonly CodeFixProvider fix = new UseAssertEnterMultipleScopeCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.UseAssertEnterMultipleScope);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new UseAssertEnterMultipleScopeCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UseAssertEnterMultipleScope }));
        }

        [Test]
        public void VerifyAssertMultiple()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMulti()
        {
            ↓Assert.Multiple(() =>
            {
                var i = 4;
                var j = 67;
                Assert.That(i, Is.EqualTo(j));
            });
        }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMulti()
        {
            using (Assert.EnterMultipleScope())
            {
                var i = 4;
                var j = 67;
                Assert.That(i, Is.EqualTo(j));
            }
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyAssertMultipleWithAsyncDelegate()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMulti()
        {
            ↓Assert.Multiple(async () =>
            {
                var i = 4;
                var j = 67;
                await Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            });
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task TestMulti()
        {
            using (Assert.EnterMultipleScope())
            {
                var i = 4;
                var j = 67;
                await Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            }
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyAssertMultipleWithAsyncDelegateAndUsingSystemThreadingInsideNamespace()
        {
            var code = @"
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
{
    using System.Threading.Tasks;

    public class TestClass
    {
        [Test]
        public void TestMulti()
        {
            ↓Assert.Multiple(async () =>
            {
                var i = 4;
                var j = 67;
                await Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            });
        }
    }
}";
            var fixedCode = @"
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
{
    using System.Threading.Tasks;

    public class TestClass
    {
        [Test]
        public async Task TestMulti()
        {
            using (Assert.EnterMultipleScope())
            {
                var i = 4;
                var j = 67;
                await Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            }
        }
    }
}";

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyAssertMultipleWithAsyncDelegateAndUsingSystemThreadingOutNamespaceButOtherUsingsInside()
        {
            var code = @"
using System.Threading.Tasks;

namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
{
    using NUnit.Framework;

    public class TestClass
    {
        [Test]
        public void TestMulti()
        {
            ↓Assert.Multiple(async () =>
            {
                var i = 4;
                var j = 67;
                await Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            });
        }
    }
}";
            var fixedCode = @"
using System.Threading.Tasks;

namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
{
    using NUnit.Framework;

    public class TestClass
    {
        [Test]
        public async Task TestMulti()
        {
            using (Assert.EnterMultipleScope())
            {
                var i = 4;
                var j = 67;
                await Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            }
        }
    }
}";

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyAssertMultipleWithAsyncDelegateAndNoSystemThreading()
        {
            var code = @"
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
{
    public class TestClass
    {
        [Test]
        public void TestMulti()
        {
            ↓Assert.Multiple(async () =>
            {
                var i = 4;
                var j = 67;
                await System.Threading.Tasks.Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            });
        }
    }
}";
            var fixedCode = @"
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
{
    public class TestClass
    {
        [Test]
        public async System.Threading.Tasks.Task TestMulti()
        {
            using (Assert.EnterMultipleScope())
            {
                var i = 4;
                var j = 67;
                await System.Threading.Tasks.Task.Yield();
                Assert.That(i, Is.EqualTo(j));
            }
        }
    }
}";

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyAssertMultipleAsync()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task TestMultiAsync()
        {
            await ↓Assert.MultipleAsync(async () =>
            {
                var i = await GetInt();
                var j = await GetInt();
                Assert.That(i, Is.EqualTo(j));
            });
        }

        private static async Task<int> GetInt() { await Task.Delay(1000); return 1; }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async Task TestMultiAsync()
        {
            using (Assert.EnterMultipleScope())
            {
                var i = await GetInt();
                var j = await GetInt();
                Assert.That(i, Is.EqualTo(j));
            }
        }

        private static async Task<int> GetInt() { await Task.Delay(1000); return 1; }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyMultipleAsyncAndValueTaskIsUsed()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async ValueTask Test()
        {
            await ↓Assert.MultipleAsync(async () =>
            {
                Assert.That(await Get1(), Is.Not.Null);
                Assert.That(await Get2(), Is.Not.Null);
            });

            static Task<string?> Get1() => Task.FromResult(default(string));
            static Task<string?> Get2() => Task.FromResult(default(string));
        }");

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public async ValueTask Test()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(await Get1(), Is.Not.Null);
                Assert.That(await Get2(), Is.Not.Null);
            }

            static Task<string?> Get1() => Task.FromResult(default(string));
            static Task<string?> Get2() => Task.FromResult(default(string));
        }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyMultipleAsyncAndGenericTaskIsUsed()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class TestClass
    {
        [Test(ExpectedResult = 4)]
        public async Task<int> Test()
        {
            await ↓Assert.MultipleAsync(async () =>
            {
                Assert.That(await Get1(), Is.Not.Null);
                Assert.That(await Get2(), Is.Not.Null);
            });

            return 4;

            static Task<string?> Get1() => Task.FromResult(default(string));
            static Task<string?> Get2() => Task.FromResult(default(string));
        }
    }");
            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class TestClass
    {
        [Test(ExpectedResult = 4)]
        public async Task<int> Test()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(await Get1(), Is.Not.Null);
                Assert.That(await Get2(), Is.Not.Null);
            }

            return 4;

            static Task<string?> Get1() => Task.FromResult(default(string));
            static Task<string?> Get2() => Task.FromResult(default(string));
        }
    }");

            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyKeepsLeadingTrivia()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void Test1()
        {
            // Arrange
            const int expected = 4;

            // Act
            int? actual = 2 + 2;

            // Assert
            ↓Assert.Multiple(() => {
                Assert.That(actual, Is.Not.Null);
                Assert.That(actual, Is.EqualTo(expected));
            });
        }");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        [Test]
        public void Test1()
        {
            // Arrange
            const int expected = 4;

            // Act
            int? actual = 2 + 2;

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual, Is.Not.Null);
                Assert.That(actual, Is.EqualTo(expected));
            }
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
    }
}

#endif
