using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.UseCollectionConstraint.UseCollectionConstraintAnalyzer>;

namespace NUnit.Analyzers.Tests.UseCollectionConstraint
{
    [TestFixture]
    public sealed class UseCollectionConstraintAnalyzerTests
    {
        private readonly DiagnosticResult diagnostic = Verify.Diagnostic(AnalyzerIdentifiers.UsePropertyConstraint);

        [Test]
        public async Task AnalyzeWhenHasLengthIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        var array = new int[] { 1 };
                        Assert.That(array, Has.Length.EqualTo(1));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenHasCountIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        var list = new List<int>() { 1 };
                        Assert.That(list, Has.Count.EqualTo(1));
                    }
            """, "using System.Collections.Generic;");
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenPropertyLengthIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        var array = new int[] { 1 };
                        Assert.That({|#0:array.Length|}, Is.EqualTo(1));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0).WithArguments("Length"));
        }

        [Test]
        public async Task AnalyzeWhenPropertyCountIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        var list = new List<int>() { 1 };
                        Assert.That({|#0:list.Count|}, Is.Not.Zero);
                    }
            """, "using System.Collections.Generic;");
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0).WithArguments("Count"));
        }

        [Test]
        public async Task AnalyzeComplex()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        var array = new int[] { 1 };
                        Assert.That({|#0:array.Length|}, Is.GreaterThan(1).And.LessThan(9));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, this.diagnostic.WithLocation(0).WithArguments("Length"));
        }

        [Test]
        public async Task AnalyzeOnNonEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    public void TestMethod()
                    {
                        var pipe = new Pipe
                        {
                            Diameter = 2.54,
                            Length = 10,
                        };
                        Assert.That(pipe.Diameter, Is.EqualTo(2.54));
                        Assert.That(pipe.Length, Is.EqualTo(10));
                    }

                    private sealed class Pipe
                    {
                        public double Diameter { get; set; }
                        public int Length { get; set; }
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenRefStructIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void Test()
                    {
                        const int Length = 8;
                        var span = new byte[Length].AsSpan();
                        Assert.That(span.Length, Is.EqualTo(Length));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
