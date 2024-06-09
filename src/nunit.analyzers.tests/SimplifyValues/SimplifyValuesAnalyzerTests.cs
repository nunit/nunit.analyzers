using System.Globalization;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SimplifyValues;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.SimplifyValues;

public class SimplifyValuesAnalyzerTests
{
    private readonly SimplifyValuesAnalyzer analyzer = new();

    [Test]
    public void VerifySupportedDiagnostics()
    {
        var diagnostics = this.analyzer.SupportedDiagnostics;

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
    public void AnalyzeWhenAttributeIsNotInNUnit()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAttributeIsNotInNUnit
    {
        [Test]
        public void ATest([Values] bool b) { }

        private sealed class ValuesAttribute : Attribute
        { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenCombinatorialStrategyIsNotUsed(
        [Values("Sequential", "Pairwise")] string nonCombinatorialAttribute,
        [Values] bool fullyQualify,
        [Values] bool omitAttribute)
    {
        var prefix = fullyQualify ? "NUnit.Framework." : string.Empty;
        var suffix = omitAttribute ? string.Empty : "Attribute";
        var attribute = $"{prefix}{nonCombinatorialAttribute}{suffix}";

        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public class AnalyzeWhenCombinatorialStrategyIsNotUsed
    {{
        public enum TestEnum {{ A, B, C }}

        [Test]
        [{attribute}]
        public void Test([Values(TestEnum.A, TestEnum.B, TestEnum.C)] TestEnum e) {{ }}
    }}");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenAttributeHasNoArguments()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAttributeHasNoArguments
    {
        [Test]
        public void ATest([Values] bool b) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenOneBooleanWasUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenOneBooleanWasUsed
    {
        [Test]
        public void ATest([Values(true)] bool b) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenTwoBooleanWereUsedForNullableBoolean()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenTwoBooleanWereUsedForNullableBoolean
    {
        [Test]
        public void ATest([Values(true, false)] bool? b) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenNotAllEnumValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenNotAllEnumValuesWereUsed
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([Values(TestEnum.A, TestEnum.B)] TestEnum e) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenNotAllEnumValuesWereUsedForNullableBoolean()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenNotAllEnumValuesWereUsedForNullableBoolean
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([Values(TestEnum.A, TestEnum.B, TestEnum.C)] TestEnum? e) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeNotAllBooleanValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeNotAllBooleanValuesAreInParameters
    {
        public void Test([Values(new object[] { true })] bool b) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeNotAllNullableBooleanValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeNotAllNullableBooleanValuesAreInParameters
    {
        public void Test([Values(new object[] { true, false })] bool? b) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeNotAllEnumValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeNotAllEnumValuesAreInParameters
    {
        public enum TestEnum { A, B, C }

        public void Test([Values(new object[] { TestEnum.A, TestEnum.B })] TestEnum e) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeNotAllNullableEnumValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeNotAllNullableEnumValuesAreInParameters
    {
        public enum TestEnum { A, B, C }

        public void Test([Values(new object[] { TestEnum.A, TestEnum.B, TestEnum.C })] TestEnum? e) { }
    }");
        RoslynAssert.Valid(this.analyzer, testCode);
    }

    [Test]
    public void AnalyzeWhenAllBooleanValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAllBooleanValuesWereUsed
    {
        [Test]
        public void Test([↓Values(true, false)] bool b) { }
    }");
        RoslynAssert.Diagnostics(this.analyzer,
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SimplifyValues),
            testCode);
    }

    [Test]
    public void AnalyzeWhenAllNullableBooleanValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAllNullableBooleanValuesWereUsed
    {
        [Test]
        public void Test([↓Values(true, false, null)] bool? b) { }
    }");
        RoslynAssert.Diagnostics(this.analyzer,
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SimplifyValues),
            testCode);
    }

    [Test]
    public void AnalyzeWhenAllEnumValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAllEnumValuesWereUsed
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([↓Values(TestEnum.A, TestEnum.B, TestEnum.C)] TestEnum e) { }
    }");
        RoslynAssert.Diagnostics(this.analyzer,
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SimplifyValues),
            testCode);
    }

    [Test]
    public void AnalyzeWhenAllNullableEnumValuesWereUsed()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeWhenAllNullableEnumValuesWereUsed
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([↓Values(TestEnum.A, TestEnum.B, TestEnum.C, null)] TestEnum? e) { }
    }");
        RoslynAssert.Diagnostics(this.analyzer,
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SimplifyValues),
            testCode);
    }

    [Test]
    public void AnalyzeAllEnumValuesAreInParameters()
    {
        var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class AnalyzeAllEnumValuesAreInParameters
    {
        public enum TestEnum { A, B, C }

        public void Test([↓Values(new object[] { TestEnum.A, TestEnum.B, TestEnum.C })] TestEnum e) { }
    }");
        RoslynAssert.Diagnostics(this.analyzer,
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.SimplifyValues),
            testCode);
    }
}
