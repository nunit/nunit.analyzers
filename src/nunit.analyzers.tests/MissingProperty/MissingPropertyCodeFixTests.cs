using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.MissingProperty;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.MissingProperty.MissingPropertyAnalyzer, NUnit.Analyzers.MissingProperty.MissingPropertyCodeFix>;

namespace NUnit.Analyzers.Tests.MissingProperty
{
    public class MissingPropertyCodeFixTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.MissingProperty);

        [Test]
        public async Task VerifyLengthToCountCodeFix()
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = (IReadOnlyCollection<int>)new[] { 42 };
                        Assert.That(collection, {|#0:Has.Length|}.EqualTo(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = (IReadOnlyCollection<int>)new[] { 42 };
                        Assert.That(collection, Has.Count.EqualTo(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("IReadOnlyCollection<int>", "Length"), fixedCode, string.Format(CultureInfo.InvariantCulture, MissingPropertyCodeFix.UsePropertyDescriptionFormat, "Count"));
        }

        [Test]
        public async Task VerifyCountToLengthCodeFix()
        {
            var code = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 42 };
                        Assert.That(collection, {|#0:Has.Count|}.EqualTo(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            var fixedCode = TestUtility.WrapInTestMethod($$"""
                        var collection = new[] { 42 };
                        Assert.That(collection, Has.Length.EqualTo(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyCodeFixAsync(code, expectedDiagnostic.WithLocation(0).WithArguments("int[]", "Count"), fixedCode, string.Format(CultureInfo.InvariantCulture, MissingPropertyCodeFix.UsePropertyDescriptionFormat, "Length"));
        }

        [Test]
        public async Task NoCodeFixForLengthWhenCountPropertyDoesNotExist()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new [] {1, 2, 3}.Where(i => i > 1);
                        Assert.That(actual, {|#0:Has.Length|}.EqualTo(2));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("IEnumerable<int>", "Length"), testCode);
        }

        [Test]
        public async Task NoCodeFixForCountWhenLengthPropertyDoesNotExist()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new [] {1, 2, 3}.Where(i => i > 1);
                        Assert.That(actual, {|#0:Has.Count|}.EqualTo(2));
            """,
                additionalUsings: "using System.Linq;");

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("IEnumerable<int>", "Count"), testCode);
        }

        [Test]
        public async Task NoCodeFixForHasMessage()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = "Can we consider string as message?...";
                        Assert.That(actual, {|#0:Has.Message|}.EqualTo(2));
            """);

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("string", "Message"), testCode);
        }

        [Test]
        public async Task NoCodeFixForRandomMissingProperty()
        {
            var testCode = TestUtility.WrapInTestMethod("""
                        var actual = new List<int> { 1, 2, 3 };
                        Assert.That(actual, {|#0:Has.Property("Whatever")|}.EqualTo(1));
            """,
                additionalUsings: "using System.Collections.Generic;");

            await Verify.VerifyCodeFixAsync(testCode, expectedDiagnostic.WithLocation(0).WithArguments("List<int>", "Whatever"), testCode);
        }
    }
}
