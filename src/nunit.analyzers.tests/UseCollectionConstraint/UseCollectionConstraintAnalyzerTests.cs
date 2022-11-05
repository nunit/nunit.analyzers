using System;
using System.Collections.Generic;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.UseCollectionConstraint;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.UseCollectionConstraint
{
    [TestFixture]
    public sealed class UseCollectionConstraintAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new UseCollectionConstraintAnalyzer();
        private readonly ExpectedDiagnostic diagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.UsePropertyConstraint);

        [Test]
        public void AnalyzeWhenHasLengthIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var array = new int[] { 1 };
            Assert.That(array, Has.Length.EqualTo(1));
        }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenHasCountIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var list = new List<int>() { 1 };
            Assert.That(list, Has.Count.EqualTo(1));
        }", "using System.Collections.Generic;");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertyLengthIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var array = new int[] { 1 };
            Assert.That(↓array.Length, Is.EqualTo(1));
        }");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenPropertyCountIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var list = new List<int>() { 1 };
            Assert.That(↓list.Count, Is.Not.Zero);
        }", "using System.Collections.Generic;");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeComplex()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            var array = new int[] { 1 };
            Assert.That(↓array.Length, Is.GreaterThan(1).And.LessThan(9));
        }");
            RoslynAssert.Diagnostics(this.analyzer, this.diagnostic, testCode);
        }

        [Test]
        public void AnalyzeOnNonEnumerable()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
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
        }");

            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenRefStructIsUsed()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
        public void Test()
        {
            const int Length = 8;
            var span = new byte[Length].AsSpan();
            Assert.That(span.Length, Is.EqualTo(Length));
        }");

            IEnumerable<MetadataReference> spanMetadata = MetadataReferences.Transitive(typeof(Span<>));
            IEnumerable<MetadataReference> metadataReferences = (Settings.Default.MetadataReferences ?? Enumerable.Empty<MetadataReference>()).Concat(spanMetadata);

            RoslynAssert.Valid(this.analyzer, testCode, Settings.Default.WithMetadataReferences(metadataReferences));
        }
    }
}
