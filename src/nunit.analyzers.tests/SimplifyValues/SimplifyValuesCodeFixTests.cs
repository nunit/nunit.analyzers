using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.SimplifyValues;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpCodeFixVerifier<NUnit.Analyzers.SimplifyValues.SimplifyValuesAnalyzer, NUnit.Analyzers.SimplifyValues.SimplifyValuesCodeFix>;

namespace NUnit.Analyzers.Tests.SimplifyValues;

public class SimplifyValuesCodeFixTests
{
    [Test]
    public void VerifyGetFixableDiagnosticIds()
    {
        CodeFixProvider fix = new SimplifyValuesCodeFix();
        var ids = fix.FixableDiagnosticIds;

        Assert.That(ids, Is.EquivalentTo(new[] { AnalyzerIdentifiers.SimplifyValues }));
    }

    [Test]
    public async Task SimplifyValuesForEnum()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForEnum
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([[|Values(TestEnum.A, TestEnum.B, TestEnum.C)|]] TestEnum e) { }
            }
        """);

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForEnum
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([Values] TestEnum e) { }
            }
        """);

        await Verify.VerifyCodeFixAsync(code, fixedCode, SimplifyValuesCodeFix.SimplifyValuesTitle);
    }

    [Test]
    public async Task SimplifyValuesForBoolean()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForBoolean
            {
                [Test]
                public void Test([[|Values(true, false)|]] bool b) { }
            }
        """);

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForBoolean
            {
                [Test]
                public void Test([Values] bool b) { }
            }
        """);

        await Verify.VerifyCodeFixAsync(code, fixedCode, SimplifyValuesCodeFix.SimplifyValuesTitle);
    }

    [Test]
    public async Task SimplifyValuesForNullableBoolean()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForBoolean
            {
                [Test]
                public void Test([[|Values(null, true, false)|]] bool? b) { }
            }
        """);

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForBoolean
            {
                [Test]
                public void Test([Values] bool? b) { }
            }
        """);

        await Verify.VerifyCodeFixAsync(code, fixedCode, SimplifyValuesCodeFix.SimplifyValuesTitle);
    }

    [Test]
    public async Task SimplifyValuesForNullableEnum()
    {
        var code = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForNullableEnum
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([[|Values(TestEnum.A, TestEnum.B, TestEnum.C, null)|]] TestEnum? e) { }
            }
        """);

        var fixedCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
            public class SimplifyValuesForNullableEnum
            {
                public enum TestEnum { A, B, C }

                [Test]
                public void Test([Values] TestEnum? e) { }
            }
        """);

        await Verify.VerifyCodeFixAsync(code, fixedCode, SimplifyValuesCodeFix.SimplifyValuesTitle);
    }
}
