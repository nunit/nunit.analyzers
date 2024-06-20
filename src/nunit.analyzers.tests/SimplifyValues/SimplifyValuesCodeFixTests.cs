using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SimplifyValues;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.SimplifyValues;

public class SimplifyValuesCodeFixTests
{
    private static readonly DiagnosticAnalyzer analyzer = new SimplifyValuesAnalyzer();
    private static readonly CodeFixProvider fix = new SimplifyValuesCodeFix();
    private static readonly ExpectedDiagnostic expectedDiagnostic =
        ExpectedDiagnostic.Create(AnalyzerIdentifiers.SimplifyValues);

    [Test]
    public void VerifyGetFixableDiagnosticIds()
    {
        var ids = fix.FixableDiagnosticIds;

        Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.SimplifyValues }));
    }

    [Test]
    public void SimplifyValuesForEnum()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForEnum
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([↓Values(TestEnum.A, TestEnum.B, TestEnum.C)] TestEnum e) { }
    }");

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForEnum
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([Values] TestEnum e) { }
    }");

        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: SimplifyValuesCodeFix.SimplifyValuesTitle);
    }

    [Test]
    public void SimplifyValuesForBoolean()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForBoolean
    {
        [Test]
        public void Test([↓Values(true, false)] bool b) { }
    }");

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForBoolean
    {
        [Test]
        public void Test([Values] bool b) { }
    }");

        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: SimplifyValuesCodeFix.SimplifyValuesTitle);
    }

    [Test]
    public void SimplifyValuesForNullableBoolean()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForBoolean
    {
        [Test]
        public void Test([↓Values(null, true, false)] bool? b) { }
    }");

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForBoolean
    {
        [Test]
        public void Test([Values] bool? b) { }
    }");

        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: SimplifyValuesCodeFix.SimplifyValuesTitle);
    }

    [Test]
    public void SimplifyValuesForNullableEnum()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForNullableEnum
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([↓Values(TestEnum.A, TestEnum.B, TestEnum.C, null)] TestEnum? e) { }
    }");

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public class SimplifyValuesForNullableEnum
    {
        public enum TestEnum { A, B, C }

        [Test]
        public void Test([Values] TestEnum? e) { }
    }");

        RoslynAssert.CodeFix(analyzer, fix, expectedDiagnostic, code, fixedCode, fixTitle: SimplifyValuesCodeFix.SimplifyValuesTitle);
    }
}
