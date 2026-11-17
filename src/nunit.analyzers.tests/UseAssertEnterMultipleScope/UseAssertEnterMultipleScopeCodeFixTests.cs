#if NUNIT4
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseAssertEnterMultipleScope;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.UseAssertEnterMultipleScope.UseAssertEnterMultipleScopeAnalyzer, NUnit.Analyzers.UseAssertEnterMultipleScope.UseAssertEnterMultipleScopeCodeFix>;

namespace NUnit.Analyzers.Tests.UseAssertEnterMultipleScope
{
    [TestFixture]
    public sealed class UseAssertEnterMultipleScopeCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.UseAssertEnterMultipleScope);

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new UseAssertEnterMultipleScopeCodeFix();
            var ids = new UseAssertEnterMultipleScopeCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UseAssertEnterMultipleScope }));
        }

        [Test]
        public async Task VerifyAssertMultiple()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMulti()
                    {
                        {|#0:Assert.Multiple(() =>
                        {
                            var i = 4;
                            var j = 67;
                            Assert.That(i, Is.EqualTo(j));
                        })|};
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMulti()
                    {
                        using (Assert.EnterMultipleScope())
                        {
                            var i = 4;
                            var j = 67;
                            Assert.That(i, Is.EqualTo(j));
                        }
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyAssertMultipleWithAsyncDelegate()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMulti()
                    {
                        {|#0:Assert.Multiple(async () =>
                        {
                            var i = 4;
                            var j = 67;
                            await Task.Yield();
                            Assert.That(i, Is.EqualTo(j));
                        })|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyAssertMultipleWithAsyncDelegateAndUsingSystemThreadingInsideNamespace()
        {
            var code = """
                using NUnit.Framework;

                namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
                {
                    using System.Threading.Tasks;

                    public class TestClass
                    {
                        [Test]
                        public void TestMulti()
                        {
                            {|#0:Assert.Multiple(async () =>
                            {
                                var i = 4;
                                var j = 67;
                                await Task.Yield();
                                Assert.That(i, Is.EqualTo(j));
                            })|};
                        }
                    }
                }
                """;
            var fixedCode = """
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
                }
                """;

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyAssertMultipleWithAsyncDelegateAndUsingSystemThreadingOutNamespaceButOtherUsingsInside()
        {
            var code = """
                using System.Threading.Tasks;

                namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
                {
                    using NUnit.Framework;

                    public class TestClass
                    {
                        [Test]
                        public void TestMulti()
                        {
                            {|#0:Assert.Multiple(async () =>
                            {
                                var i = 4;
                                var j = 67;
                                await Task.Yield();
                                Assert.That(i, Is.EqualTo(j));
                            })|};
                        }
                    }
                }
                """;
            var fixedCode = """
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
                }
                """;

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyAssertMultipleWithAsyncDelegateAndNoSystemThreading()
        {
            var code = """
                using NUnit.Framework;

                namespace NUnit.Analyzers.Tests.Targets.UseAssertEnterMultipleScope
                {
                    public class TestClass
                    {
                        [Test]
                        public void TestMulti()
                        {
                            {|#0:Assert.Multiple(async () =>
                            {
                                var i = 4;
                                var j = 67;
                                await System.Threading.Tasks.Task.Yield();
                                Assert.That(i, Is.EqualTo(j));
                            })|};
                        }
                    }
                }
                """;
            var fixedCode = """
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
                }
                """;

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyAssertMultipleAsync()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async Task TestMultiAsync()
                    {
                        await {|#0:Assert.MultipleAsync(async () =>
                        {
                            var i = await GetInt();
                            var j = await GetInt();
                            Assert.That(i, Is.EqualTo(j));
                        })|};
                    }

                    private static async Task<int> GetInt() { await Task.Delay(1000); return 1; }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async Task TestMultiAsync()
                    {
                        using (Assert.EnterMultipleScope())
                        {
                            var i = await GetInt();
                            var j = await GetInt();
                            Assert.That(i, Is.EqualTo(j));
                        }
                    }

                    private static async Task<int> GetInt() { await Task.Delay(1000); return 1; }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyMultipleAsyncAndValueTaskIsUsed()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async ValueTask Test()
                    {
                        await {|#0:Assert.MultipleAsync(async () =>
                        {
                            Assert.That(await Get1(), Is.Not.Null);
                            Assert.That(await Get2(), Is.Not.Null);
                        })|};

                        static Task<string?> Get1() => Task.FromResult(default(string));
                        static Task<string?> Get2() => Task.FromResult(default(string));
                    }
            """);

            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public async ValueTask Test()
                    {
                        using (Assert.EnterMultipleScope())
                        {
                            Assert.That(await Get1(), Is.Not.Null);
                            Assert.That(await Get2(), Is.Not.Null);
                        }

                        static Task<string?> Get1() => Task.FromResult(default(string));
                        static Task<string?> Get2() => Task.FromResult(default(string));
                    }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyMultipleAsyncAndGenericTaskIsUsed()
        {
            var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public class TestClass
                {
                    [Test(ExpectedResult = 4)]
                    public async Task<int> Test()
                    {
                        await {|#0:Assert.MultipleAsync(async () =>
                        {
                            Assert.That(await Get1(), Is.Not.Null);
                            Assert.That(await Get2(), Is.Not.Null);
                        })|};

                        return 4;

                        static Task<string?> Get1() => Task.FromResult(default(string));
                        static Task<string?> Get2() => Task.FromResult(default(string));
                    }
                }
            """);
            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
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
                }
            """);

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }

        [Test]
        public async Task VerifyKeepsLeadingTrivia()
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [Test]
                    public void Test1()
                    {
                        // Arrange
                        const int expected = 4;

                        // Act
                        int? actual = 2 + 2;

                        // Assert
                        {|#0:Assert.Multiple(() => {
                            Assert.That(actual, Is.Not.Null);
                            Assert.That(actual, Is.EqualTo(expected));
                        })|};
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
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
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0), fixedCode);
        }
    }
}

#endif
