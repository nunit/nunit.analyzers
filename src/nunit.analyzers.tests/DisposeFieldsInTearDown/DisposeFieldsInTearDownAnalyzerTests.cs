using System.Collections.Generic;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DisposeFieldsInTearDown;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DisposeFieldsInTearDown
{
    public class DisposeFieldsInTearDownAnalyzerTests
    {
        private const string DummyDisposable = @"
        private sealed class DummyDisposable : IDisposable
        {
            public void Dispose() {}
        }";

        // CA2000 allows transfer of ownership using ICollection<IDisposable>.Add
        private const string Disposer = @"
        private sealed class Disposer : IDisposable
        {
            public void Dispose() {}

            public T? Add<T>(T? resource) => resource;
        }
        ";

        private static readonly DiagnosticAnalyzer analyzer = new DisposeFieldsInTearDownAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.FieldIsNotDisposedInTearDown);

        [Test]
        public void AnalyzeWhenFieldIsDisposedInCorrectMethod(
            [Values("", "OneTime")] string attributePrefix,
            [Values("", "this.")] string prefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? field;

        [{attributePrefix}SetUp]
        public void SetUpMethod()
        {{
            {prefix}field = new DummyDisposable();
        }}

        [{attributePrefix}TearDown]
        public void TearDownMethod()
        {{
            {prefix}field?.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("", "OneTime")]
        [TestCase("OneTime", "")]
        public void AnalyzeWhenFieldIsDisposedInWrongMethod(string attributePrefix1, string attributePrefix2)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? ↓field;

        [{attributePrefix1}SetUp]
        public void SetUpMethod()
        {{
            field = new DummyDisposable();
        }}

        [{attributePrefix2}TearDown]
        public void TearDownMethod()
        {{
            field?.Dispose();
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("SetUp")]
        [TestCase("Test")]
        public void AnalyzeWhenFieldIsNotDisposed(string attribute)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private object? ↓field;

        [{attribute}]
        public void SomeMethod()
        {{
            field = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            field = null;
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("")]
        [TestCase("OneTime")]
        public void AnalyzeWhenFieldIsDisposedUsingDisposer(string attributePrefix)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private Disposer? disposer;
        private object? field;

        [{attributePrefix}SetUp]
        public void SetUpMethod()
        {{
            disposer = new Disposer();
            field = disposer.Add(new DummyDisposable());
        }}

        [{attributePrefix}TearDown]
        public void TearDownMethod()
        {{
            disposer?.Dispose();
        }}

        {DummyDisposable}

        {Disposer}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
