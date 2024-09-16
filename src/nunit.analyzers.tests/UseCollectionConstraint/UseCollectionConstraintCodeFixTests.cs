using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseCollectionConstraint;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseCollectionConstraint
{
    [TestFixture]
    public sealed class UseCollectionConstraintCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new UseCollectionConstraintAnalyzer();
        private static readonly CodeFixProvider fix = new UseCollectionConstraintCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.UsePropertyConstraint);

        private static readonly string[] NumericContraints =
        {
            "EqualTo(3)",
            "GreaterThan(1)",
            "LessThan(5)",
            "GreaterThanOrEqualTo(3).And.LessThanOrEqualTo(9)",
            "Not.LessThan(5)",
        };

        [Test]
        public void VerifyGetFixableDiagnosticIds()
        {
            var fix = new UseCollectionConstraintCodeFix();
            var ids = fix.FixableDiagnosticIds;

            Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.UsePropertyConstraint }));
        }

        [TestCaseSource(nameof(NumericContraints))]
        public void VerifyLength(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var array = new int[] {{ 1 }};
            Assert.That(↓array.Length, Is.{constraint});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var array = new int[] {{ 1 }};
            Assert.That(array, Has.Length.{constraint});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(NumericContraints))]
        public void VerifyLengthWithNewLine(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var array = new int[] {{ 1 }};
            Assert.That(↓array.Length,
                Is.{constraint});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var array = new int[] {{ 1 }};
            Assert.That(array,
                Has.Length.{constraint});
        }}");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCaseSource(nameof(NumericContraints))]
        public void VerifyCount(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var list = new List<int>() {{ 1 }};
            Assert.That(↓list.Count, Is.Not.{constraint}, ""Number of Members"");
        }}", "using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var list = new List<int>() {{ 1 }};
            Assert.That(list, Has.Count.Not.{constraint}, ""Number of Members"");
        }}", "using System.Collections.Generic;");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase("Is.Zero")]
        [TestCase("Is.Not.Positive")]
        [TestCase("Is.EqualTo(0)")]
        [TestCase("Is.LessThan(1)")]
        [TestCase("Is.Not.GreaterThan(0)")]
        [TestCase("Is.Not.GreaterThanOrEqualTo(1)")]
        public void VerifyIsEmpty(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@$"
        public void TestMethod()
        {{
            Assert.That(↓Array.Empty<int>().Length, {constraint});
        }}");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            Assert.That(Array.Empty<int>(), Is.Empty);
        }");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [TestCase("Is.Not.Zero")]
        [TestCase("Is.Positive")]
        [TestCase("Is.Not.EqualTo(0)")]
        [TestCase("Is.Not.LessThan(1)")]
        [TestCase("Is.GreaterThan(0)")]
        [TestCase("Is.GreaterThanOrEqualTo(1)")]
        public void VerifyNotEmpty(string constraint)
        {
            var code = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        public void TestMethod()
        {{
            var list = new List<int>() {{ 1 }};
            Assert.That(↓list.Count, {constraint});
        }}", "using System.Collections.Generic;");
            var fixedCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void TestMethod()
        {
            var list = new List<int>() { 1 };
            Assert.That(list, Is.Not.Empty);
        }", "using System.Collections.Generic;");
            RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }

        [Test]
        public void VerifyNotSuggestingHasPropertyOnNonIntegralTypes()
        {
            const string pipeClassCode = @"

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
";
            var code = TestUtility.WrapClassInNamespaceAndAddUsing(pipeClassCode + @"
        [TestFixture]
        public class TestClass
        {
            public void TestMethod()
            {
                PipeSegment[] segments = { new PipeSegment(10.0), new PipeSegment(5.0) };
                var pipe = new Pipe(2.54, segments);
                Assert.That(pipe.Diameter, Is.EqualTo(2.54));
                Assert.That(↓pipe.Count, Is.EqualTo(2));
                Assert.That(pipe.Length, Is.EqualTo(15.0));
            }
        }", "using System.Collections; using System.Collections.Generic; using System.Linq;");

            var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(pipeClassCode + @"
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
        }", "using System.Collections; using System.Collections.Generic; using System.Linq;");

            RoslynAssert.FixAll(analyzer, fix, expectedDiagnostic, code, fixedCode);
        }
    }
}
