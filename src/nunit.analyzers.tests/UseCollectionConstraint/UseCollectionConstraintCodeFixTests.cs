using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseCollectionConstraint;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.UseCollectionConstraint.UseCollectionConstraintAnalyzer, NUnit.Analyzers.UseCollectionConstraint.UseCollectionConstraintCodeFix>;

namespace NUnit.Analyzers.Tests.UseCollectionConstraint
{
    [TestFixture]
    public sealed class UseCollectionConstraintCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.UsePropertyConstraint);

        private static readonly string[] NumericContraints =
        [
            "EqualTo(3)",
            "GreaterThan(1)",
            "LessThan(5)",
            "GreaterThanOrEqualTo(3).And.LessThanOrEqualTo(9)",
            "Not.LessThan(5)",
        ];

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new UseCollectionConstraintCodeFix();
            var ids = new UseCollectionConstraintCodeFix().FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UsePropertyConstraint }));
        }

        [TestCaseSource(nameof(NumericContraints))]
        public async Task VerifyLength(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var array = new int[] { 1 };
                        Assert.That({|#0:array.Length|}, Is.{{constraint}});
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var array = new int[] { 1 };
                        Assert.That(array, Has.Length.{{constraint}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("Length"), fixedCode);
        }

        [TestCaseSource(nameof(NumericContraints))]
        public async Task VerifyLengthWithNewLine(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var array = new int[] { 1 };
                        Assert.That({|#0:array.Length|},
                            Is.{{constraint}});
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var array = new int[] { 1 };
                        Assert.That(array,
                            Has.Length.{{constraint}});
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("Length"), fixedCode);
        }

        [TestCaseSource(nameof(NumericContraints))]
        public async Task VerifyCount(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var list = new List<int>() { 1 };
                        Assert.That({|#0:list.Count|}, Is.Not.{{constraint}}, "Number of Members");
                    }
            """, "using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var list = new List<int>() { 1 };
                        Assert.That(list, Has.Count.Not.{{constraint}}, "Number of Members");
                    }
            """, "using System.Collections.Generic;");
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("Count"), fixedCode);
        }

        [TestCase("Is.Zero")]
        [TestCase("Is.Not.Positive")]
        [TestCase("Is.EqualTo(0)")]
        [TestCase("Is.LessThan(1)")]
        [TestCase("Is.Not.GreaterThan(0)")]
        [TestCase("Is.Not.GreaterThanOrEqualTo(1)")]
        public async Task VerifyIsEmpty(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        Assert.That({|#0:Array.Empty<int>().Length|}, {{constraint}});
                    }
            """);
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        Assert.That(Array.Empty<int>(), Is.Empty);
                    }
            """);
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("Length"), fixedCode);
        }

        [TestCase("Is.Not.Zero")]
        [TestCase("Is.Positive")]
        [TestCase("Is.Not.EqualTo(0)")]
        [TestCase("Is.Not.LessThan(1)")]
        [TestCase("Is.GreaterThan(0)")]
        [TestCase("Is.GreaterThanOrEqualTo(1)")]
        public async Task VerifyNotEmpty(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    public void TestMethod()
                    {
                        var list = new List<int>() { 1 };
                        Assert.That({|#0:list.Count|}, {{constraint}});
                    }
            """, "using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    public void TestMethod()
                    {
                        var list = new List<int>() { 1 };
                        Assert.That(list, Is.Not.Empty);
                    }
            """, "using System.Collections.Generic;");
            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("Count"), fixedCode);
        }

        [Test]
        public async Task VerifyNotSuggestingHasPropertyOnNonIntegralTypes()
        {
            const string pipeClassCode = """

                internal sealed class PipeSegment
                {
                    public PipeSegment(double length) => Length = length;
                    public double Length { get; }
                }

                internal sealed class Pipe : IEnumerable<PipeSegment>
                {
                    PipeSegment[] _segments;

                    public Pipe(double diameter, PipeSegment[] segments)
                    {
                        Diameter = diameter;
                        _segments = segments;
                    }

                    public double Diameter { get; }

                    public double Length => Enumerable.Sum(_segments, x => x.Length);

                    public int Count => _segments.Length;

                    public IEnumerator<PipeSegment> GetEnumerator() => ((IEnumerable<PipeSegment>)_segments).GetEnumerator();

                    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                }
            """;
            var code = TestUtility.WrapClassInNamespaceAndAddUsing(pipeClassCode + "\n" + """
                [TestFixture]
                public class TestClass
                {
                    public void TestMethod()
                    {
                        PipeSegment[] segments = { new PipeSegment(10.0), new PipeSegment(5.0) };
                        var pipe = new Pipe(2.54, segments);
                        Assert.That(pipe.Diameter, Is.EqualTo(2.54));
                        Assert.That({|#0:pipe.Count|}, Is.EqualTo(2));
                        Assert.That(pipe.Length, Is.EqualTo(15.0));
                    }
                }
            """, "using System.Collections; using System.Collections.Generic; using System.Linq;");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(pipeClassCode + "\n" + """
                [TestFixture]
                public class TestClass
                {
                    public void TestMethod()
                    {
                        PipeSegment[] segments = { new PipeSegment(10.0), new PipeSegment(5.0) };
                        var pipe = new Pipe(2.54, segments);
                        Assert.That(pipe.Diameter, Is.EqualTo(2.54));
                        Assert.That(pipe, Has.Count.EqualTo(2));
                        Assert.That(pipe.Length, Is.EqualTo(15.0));
                    }
                }
            """, "using System.Collections; using System.Collections.Generic; using System.Linq;");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("Count"), fixedCode);
        }
    }
}
