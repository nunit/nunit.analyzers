using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.DisposeFieldsInTearDown;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DisposeFieldsInTearDown
{
    public sealed class DisposeFieldsInTearDownAnalyzerTests
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

        [Test]
        public void AnalyzeWhenFieldIsDisposedInTryOrFinallyStatement()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private IDisposable? field1;
        private IDisposable? field2;

        [SetUp]
        public void SetUpMethod()
        {{
            field1 = new DummyDisposable();
            field2 = new DummyDisposable();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            try
            {{
                field1?.Dispose();
            }}
            finally
            {{
                field2?.Dispose();
            }}
        }}

        {DummyDisposable}
        ");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Dispose")]
        [TestCase("Close")]
        public void AnalyzeWhenFieldIsDisposedUsingClose(string method)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private DummyWriter? field;

        [SetUp]
        public void SetUpMethod()
        {{
            field = new DummyWriter();
        }}

        [TearDown]
        public void TearDownMethod()
        {{
            field?.{method}();
        }}

        private sealed class DummyWriter : IDisposable
        {{
            public void Dispose() {{}}

            public void Close() => Dispose();
        }}");

            RoslynAssert.Valid(analyzer, testCode);
        }

#if !NETFRAMEWORK
        [Test]
        public void AnalyzeWhenFieldIsAsyncDisposable(
            [Values("DisposeAsync", "CloseAsync")] string method,
            [Values("", "this.")] string prefix,
            [Values("", ".ConfigureAwait(false)")] string configureAwait)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($@"
        private DummyAsyncWriter? field;

        [SetUp]
        public void SetUpMethod()
        {{
            field = new DummyAsyncWriter();
        }}

        [TearDown]
        public async Task TearDownMethod()
        {{
            if (field is not null)
                await {prefix}field.{method}(){configureAwait};
        }}

        private sealed class DummyAsyncWriter : IAsyncDisposable
        {{
            public ValueTask DisposeAsync() => default(ValueTask);

            public Task CloseAsync() => DisposeAsync().AsTask();
        }}");

            RoslynAssert.Valid(analyzer, testCode);
        }
#endif
    }
}
