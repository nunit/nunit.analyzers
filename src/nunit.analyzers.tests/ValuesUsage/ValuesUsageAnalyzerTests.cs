using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ValuesUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.ValuesUsage.ValuesUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.ValuesUsage
{
    public class ValuesUsageAnalyzerTests
    {
        private static IEnumerable<TestCaseData> SpecialConversions
        {
            get
            {
                yield return new TestCaseData("2019-10-10", typeof(DateTime));
                yield return new TestCaseData("23:59:59", typeof(TimeSpan));
                yield return new TestCaseData("2019-10-10", typeof(DateTimeOffset));
                yield return new TestCaseData("2019-10-14T19:15:25+00:00", typeof(DateTimeOffset));
                yield return new TestCaseData("https://github.com/nunit/", typeof(Uri));
            }
        }

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var diagnostics = new ValuesUsageAnalyzer().SupportedDiagnostics;

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            var diagnostic = diagnostics[0];
            Assert.Multiple(() =>
                            {
                                Assert.That(diagnostic.Id, Is.EqualTo(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage));
                                Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty);
                                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Structure));
                                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error));
                            });
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsNotInNUnit()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAttributeIsNotInNUnit
                {
                    [Test]
                    public void ATest([Values] bool value) { }

                    private sealed class ValuesAttribute : Attribute
                    { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAttributeHasNoArguments()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAttributeHasNoArguments
                {
                    [Test]
                    public void ATest([Values] bool value) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentTypeIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentTypeIsCorrect
                {
                    [Test]
                    public void ATest([Values(true, false)] bool blah) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCaseSource(nameof(SpecialConversions))]
        public async Task AnalyzeWhenArgumentIsSpecialConversion(string value, Type targetType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public sealed class AnalyzeWhenArgumentIsSpecialConversion
                {
                    [Test]
                    public void Test([Values("{{value}}")] {{targetType.Name}} a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsACast()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentIsACast
                {
                    [Test]
                    public void Test([Values((byte)2)] byte a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsAPrefixedValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentIsAPrefixedValue
                {
                    [Test]
                    public void Test([Values(-2)] int a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentIsEnum
                {
                    public enum TestEnum { A,B,C }

                    [Test]
                    public void Test([Values(TestEnum.A, TestEnum.B)] TestEnum e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsIntConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentIsIntConvertedToEnum
                {
                    public enum TestEnum { A,B,C }

                    [Test]
                    public void Test([Values(0, 1)] TestEnum e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsEnumConvertedToInt()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentIsEnumConvertedToInt
                {
                    public enum TestEnum { A,B,C }

                    [Test]
                    public void Test([Values(TestEnum.A, TestEnum.B)] int e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentTypeGeneric()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentTypeIsCorrect
                {
                    [Test]
                    public void ATest<T>([Values(5, 5.0)] T blah) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("notnull")]
        [TestCase("class")]
        public async Task AnalyzeWhenArgumentTypeGenericWithConstraintsAndNonCompatible(string constraint)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public sealed class AnalyzeWhenArgumentTypeIsNotCorrect
                {
                    [Test]
                    public void ATest<T>([Values({|#0:null|}, "5.0")] T blah) where T : {{constraint}} { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "object", "blah", "T"));
        }

        [TestCase("null")]
        [TestCase("\"5.0\"")]
        public async Task AnalyzeWhenArgumentTypeGenericWithStructConstraintsAndNonCompatible(string value)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public sealed class AnalyzeWhenArgumentTypeIsNotCorrect
                {
                    [Test]
                    public void ATest<T>([Values({|#0:{{value}}|})] T blah) where T : struct { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", value == "null" ? "<null>" : "string", "blah", "T"));
        }

        [TestCase("", "class?")]
        [TestCase("?", "class")]
        public async Task AnalyzeWhenArgumentTypeGenericWithConstraintsAndCompatible(string nullableAnnotation, string constraint)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public sealed class AnalyzeWhenArgumentTypeIsNotCorrect
                {
                    [Test]
                    public void ATest<T>([Values(null, "5.0")] T{{nullableAnnotation}} blah) where T : {{constraint}} { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentTypeGenericAndDifferentTypes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentTypeIsNotCorrect
                {
                    [Test]
                    public void ATest<T>([Values(5, {|#0:"5.0"|})] T blah) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("1", "string", "blah", "T"));
        }

        [Test]
        public async Task AnalyzeWhenArgumentTypeGenericResolvesNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentTypeIsNotCorrect
                {
                    [Test]
                    public void ATest<T>([Values("5.0", null)] T blah) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeParameterIsArray()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentIsArray
                {
                    [Test]
                    public void Test([Values(new byte[] { }, new byte[] { 1 }, new byte[] { 1, 2, 3 })] byte[] buffer) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeParameterIsArrayOfObjectWithGoodArguments()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsArrayOfObject
                {
                    public void Test([Values(new object[] { 1, 2.0 })] double d) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeParameterIsArrayOfObjectWithBadArguments()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsArrayOfObject
                {
                    public void Test([Values(new object[] { 1, {|#0:2.0|} })] int i) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("1", "double", "i", "int"));
        }

        [Test]
        public async Task AnalyzeArgumentIsStringConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentIsStringConvertedToEnum
                {
                    public enum TestEnum { A,B,C }

                    [Test]
                    public void Test([Values({|#0:"A"|}, {|#1:"B"|})] TestEnum e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "string", "e", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsStringConvertedToEnum.TestEnum"), Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(1).WithArguments("1", "string", "e", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsStringConvertedToEnum.TestEnum"));
        }

        [Test]
        public async Task AnalyzeArgumentIsEnumConvertedToString()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentIsEnumConvertedToString
                {
                    public enum TestEnum { A,B,C }

                    [Test]
                    public void Test([Values({|#0:TestEnum.A|}, {|#1:TestEnum.B|})] string e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsEnumConvertedToString.TestEnum", "e", "string"), Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(1).WithArguments("1", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsEnumConvertedToString.TestEnum", "e", "string"));
        }

        [Test]
        public async Task AnalyzeArgumentHasImplicitConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentHasImplicitConversion
                {
                    [Test]
                    public void Test([Values(uint.MaxValue)] long e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsStringConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                public sealed class AnalyzeArgumentIsStringConvertedToTypeWithCustomConverter
                {
                    [Test]
                    public void Test([Values("A")] CustomType p) { }
                }

                [TypeConverter(typeof(CustomTypeConverter))]
                public struct CustomType { }
                public class CustomTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsNullConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                public sealed class AnalyzeArgumentIsNullConvertedToTypeWithCustomConverter
                {
                    [Test]
                    public void Test([Values(null)] CustomType p) { }
                }

                [TypeConverter(typeof(CustomTypeConverter))]
                public struct CustomType { }
                public class CustomTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsStringConvertedToTypeWithCustomConverterOnBaseType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                public sealed class AnalyzeArgumentIsStringConvertedToTypeWithCustomConverterOnBaseType
                {
                    [Test]
                    public void Test([Values("A")] CustomType p) { }
                }

                public class CustomType : BaseType { }
                [TypeConverter(typeof(BaseTypeConverter))]
                public class BaseType { }
                public class BaseTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsIntConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                public sealed class AnalyzeArgumentIsIntConvertedToTypeWithCustomConverter
                {
                    [Test]
                    public void Test([Values(2)] CustomType p) { }
                }

                [TypeConverter(typeof(CustomTypeConverter))]
                public struct CustomType { }
                public class CustomTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                                                                             ValuesUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                                                                             1, "object", "blah", "bool"));
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class Foo
                {
                    [Test]
                    public void ATest([Values(true, {|#0:null|})] bool blah) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeArgumentHasTypeNotAllowedInAttributes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeArgumentHasTypeNotAllowedInAttributes
                {
                    [Test]
                    public void Test([Values(1.0m)] decimal d) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToValueType()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                                                                             ValuesUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                                                                             0, "<null>", "a", "char"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToValueType
                {
                    [Test]
                    public void Test([Values({|#0:null|})] char a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNullableValueType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToNullableType
                {
                    [Test]
                    public void Test([Values(null)] int? a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNullableEnumType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToNullableEnumType
                {
                    public enum TestEnum { A,B,C }

                    [Test]
                    public void Test([Values(null)] TestEnum? e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNullableReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToNullableType
                {
                    public void Test([Values(null)] object? a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToUnspecifiedReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                #nullable disable
                public sealed class AnalyzeWhenArgumentPassesNullToNullableType
                {
                    public void Test([Values(null)] object a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNonNullableReferenceType()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                                                                             ValuesUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                                                                             0, "<null>", "a", "object"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToNonNullableType
                {
                    public void Test([Values({|#0:null|})] object a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenSuppressedNullIsPassedToAReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenSuppressedNullIsPassedToAReferenceType
                {
                    public void Test([Values(null!)] string s) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSuppressedNullIsPassedToANonReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenSuppressedNullIsPassedToAReferenceType
                {
                    public void Test([Values({|#0:null!|})] int i) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "<null>", "i", "int"));
        }

        [Test]
        public async Task AnalyzeWhenSuppressedConstantIsPassedToANonNullableReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenSuppressedNullIsPassedToAReferenceType
                {
                    public void Test([Values({|#0:2!|})] string s) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "int", "s", "string"));
        }
    }
}
