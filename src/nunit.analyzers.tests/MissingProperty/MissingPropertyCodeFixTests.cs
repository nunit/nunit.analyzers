using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.MissingProperty;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NUnit.Analyzers.Tests.MissingProperty
{
    public class MissingPropertyCodeFixTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new MissingPropertyAnalyzer();
        private static readonly CodeFixProvider fix = new MissingPropertyCodeFix();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.MissingProperty);

        [Test]
        public void VerifyLengthToCountCodeFix()
        {
            var code = TestUtility.WrapInTestMethod($@"
                var collection = (IReadOnlyCollection<int>)new[] {{ 42 }};
                Assert.That(collection, ↓Has.Length.EqualTo(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod($@"
                var collection = (IReadOnlyCollection<int>)new[] {{ 42 }};
                Assert.That(collection, Has.Count.EqualTo(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: string.Format(CultureInfo.InvariantCulture, CodeFixConstants.UsePropertyDescriptionFormat, "Count"));
        }

        [Test]
        public void VerifyCountToLengthCodeFix()
        {
            var code = TestUtility.WrapInTestMethod($@"
                var collection = new[] {{ 42 }};
                Assert.That(collection, ↓Has.Count.EqualTo(1));",
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod($@"
                var collection = new[] {{ 42 }};
                Assert.That(collection, Has.Length.EqualTo(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode,
                fixTitle: string.Format(CultureInfo.InvariantCulture, CodeFixConstants.UsePropertyDescriptionFormat, "Length"));
        }

        [Test]
        public void NoCodeFixForLengthWhenCountPropertyDoesNotExist()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new [] {1, 2, 3}.Where(i => i > 1);
                Assert.That(actual, ↓Has.Length.EqualTo(2));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.NoFix(analyzer, fix, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoCodeFixForCountWhenLengthPropertyDoesNotExist()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new [] {1, 2, 3}.Where(i => i > 1);
                Assert.That(actual, ↓Has.Count.EqualTo(2));",
                additionalUsings: "using System.Linq;");

            AnalyzerAssert.NoFix(analyzer, fix, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoCodeFixForHasMessage()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = ""Can we consider string as message?..."";
                Assert.That(actual, ↓Has.Message.EqualTo(2);");

            AnalyzerAssert.NoFix(analyzer, fix, expectedDiagnostic, testCode);
        }

        [Test]
        public void NoCodeFixForRandomMissingProperty()
        {
            var testCode = TestUtility.WrapInTestMethod(@"
                var actual = new List<int> { 1, 2, 3 };
                Assert.That(actual, ↓Has.Property(""Whatever"").EqualTo(1));",
                additionalUsings: "using System.Collections.Generic;");

            AnalyzerAssert.NoFix(analyzer, fix, expectedDiagnostic, testCode);
        }
    }
}
