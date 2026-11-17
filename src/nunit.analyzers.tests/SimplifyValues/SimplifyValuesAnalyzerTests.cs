using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SimplifyValues;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.SimplifyValues.SimplifyValuesAnalyzer>;

namespace NUnit.Analyzers.Tests.SimplifyValues;

public class SimplifyValuesAnalyzerTests
{
    [Test]
    public void VerifySupportedDiagnostics()
    {
        var diagnostics = new SimplifyValuesAnalyzer().SupportedDiagnostics;

        Assert.That(diagnostics, Has.Length.EqualTo(1));
        var diagnostic = diagnostics[0];
        Assert.Multiple(() =>
        {
            Assert.That(diagnostic.Id, Is.EqualTo(AnalyzerIdentifiers.SimplifyValues));
            Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty);
            Assert.That(diagnostic.Category, Is.EqualTo(Categories.Style));
            Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Info));
        });
    }

    [Test]
    public async Task AnalyzeWhenAttributeIsNotInNUnit()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenAttributeIsNotInNUnit
            {
                [Test]
                public void ATest([Values] bool b) { }

                private sealed class ValuesAttribute : Attribute
                { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenCombinatorialStrategyIsNotUsed(
        [Values("Sequential", "Pairwise")] string nonCombinatorialAttribute,
        [Values] bool fullyQualify,
        [Values] bool omitAttribute)
    {
        var prefix = fullyQualify ? "NUnit.Framework." : string.Empty;
        var suffix = omitAttribute ? string.Empty : "Attribute";
        var attribute = $"{prefix}{nonCombinatorialAttribute}{suffix}";

        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
            public class AnalyzeWhenCombinatorialStrategyIsNotUsed
            {
                public enum TestEnum { A, B, C }

                [Test]
                [{{attribute}}]
                public void Test([Values(TestEnum.A, TestEnum.B, TestEnum.C)] TestEnum e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenAttributeHasNoArguments()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenAttributeHasNoArguments
            {
                [Test]
                public void ATest([Values] bool b) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenOneBooleanWasUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenOneBooleanWasUsed
            {
                [Test]
                public void ATest([Values(true)] bool b) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenTwoBooleanWereUsedForNullableBoolean()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenTwoBooleanWereUsedForNullableBoolean
            {
                [Test]
                public void ATest([Values(true, false)] bool? b) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenNotAllEnumValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenNotAllEnumValuesWereUsed
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([Values(TestEnum.A, TestEnum.B)] TestEnum e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenNotAllEnumValuesWereUsedForNullableBoolean()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenNotAllEnumValuesWereUsedForNullableBoolean
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([Values(TestEnum.A, TestEnum.B, TestEnum.C)] TestEnum? e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeNotAllBooleanValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeNotAllBooleanValuesAreInParameters
            {
                public void Test([Values(new object[] { true })] bool b) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeNotAllNullableBooleanValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeNotAllNullableBooleanValuesAreInParameters
            {
                public void Test([Values(new object[] { true, false })] bool? b) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeNotAllEnumValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeNotAllEnumValuesAreInParameters
            {
                public enum TestEnum { A, B, C }

                public void Test([Values(new object[] { TestEnum.A, TestEnum.B })] TestEnum e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeNotAllNullableEnumValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeNotAllNullableEnumValuesAreInParameters
            {
                public enum TestEnum { A, B, C }

                public void Test([Values(new object[] { TestEnum.A, TestEnum.B, TestEnum.C })] TestEnum? e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenAllBooleanValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenAllBooleanValuesWereUsed
            {
                [Test]
                public void Test([[|Values(true, false)|]] bool b) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenAllNullableBooleanValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenAllNullableBooleanValuesWereUsed
            {
                [Test]
                public void Test([[|Values(true, false, null)|]] bool? b) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenAllEnumValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenAllEnumValuesWereUsed
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([[|Values(TestEnum.A, TestEnum.B, TestEnum.C)|]] TestEnum e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeWhenAllNullableEnumValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeWhenAllNullableEnumValuesWereUsed
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([[|Values(TestEnum.A, TestEnum.B, TestEnum.C, null)|]] TestEnum? e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }

    [Test]
    public async Task AnalyzeAllEnumValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class AnalyzeAllEnumValuesAreInParameters
            {
                public enum TestEnum { A, B, C }

                public void Test([[|Values(new object[] { TestEnum.A, TestEnum.B, TestEnum.C })|]] TestEnum e) { }
            }
        """);

        await Verify.VerifyAnalyzerAsync(testCode);
    }
}
