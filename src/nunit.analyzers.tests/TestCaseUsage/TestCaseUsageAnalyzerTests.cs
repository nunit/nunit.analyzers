using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseUsage;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.TestCaseUsage.TestCaseUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.TestCaseUsage
{
    [TestFixture]
    public sealed class TestCaseUsageAnalyzerTests
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

#if NET6_0_OR_GREATER
        private static IEnumerable<TestCaseData> SpecialConversions_NET6
        {
            get
            {
                yield return new TestCaseData("2019-10-10", typeof(DateOnly));
                yield return new TestCaseData("23:59:59", typeof(TimeOnly));
            }
        }
#endif

        [Test]
        public void VerifySupportedDiagnostics()
        {
            var analyzer = new TestCaseUsageAnalyzer();
            var diagnostics = new TestCaseUsageAnalyzer().SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage,
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage
            };

            Assert.That(diagnostics.Select(d => d.Id), Is.EquivalentTo(expectedIdentifiers));

            foreach (var diagnostic in diagnostics)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty);
                    Assert.That(diagnostic.Category, Is.EqualTo(Categories.Structure),
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                    Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                        $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
                });
            }

            var diagnosticMessage = diagnostics.Select(_ => _.MessageFormat.ToString(CultureInfo.InvariantCulture)).ToImmutableArray();

            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage),
                $"{TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage),
                $"{TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage} is missing.");
            Assert.That(diagnosticMessage, Contains.Item(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage),
                $"{TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage} is missing.");
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsNotInNUnit()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAttributeIsNotInNUnit
                {
                    [TestCase]
                    public void ATest() { }

                    private sealed class TestCaseAttribute : Attribute
                    { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAttributeIsTestAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenAttributeIsTestAttribute
                {
                    [Test]
                    public void ATest() { }
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
                    [TestCase]
                    public void ATest() { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentIsCorrect
                {
                    [TestCase(2)]
                    public void Test(int a) { }
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
                    [TestCase("{{value}}")]
                    public void Test({{targetType.Name}} a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsACast()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsACast
                {
                    [TestCase((byte)2)]
                    public void Test(byte a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsAPrefixedValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsAPrefixedValue
                {
                    [TestCase(-2)]
                    public void Test(int a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsEnum
                {
                    public enum TestEnum { A,B,C }

                    [TestCase(TestEnum.A)]
                    [TestCase(TestEnum.B)]
                    public void Test(TestEnum e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsIntConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsIntConvertedToEnum
                {
                    public enum TestEnum { A,B,C }

                    [TestCase(0)]
                    [TestCase(1)]
                    public void Test(TestEnum e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsEnumConvertedToInt()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsEnumConvertedToInt
                {
                    public enum TestEnum { A,B,C }

                    [TestCase(TestEnum.A)]
                    [TestCase(TestEnum.B)]
                    public void Test(int e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeParameterIsArray()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsArray
                {
                    [TestCase(new byte[] { })]
                    [TestCase(new byte[] { 1 })]
                    [TestCase(new byte[] { 1, 2 })]
                    [TestCase(new byte[] { 1, 2, 3 })]
                    [TestCase(new byte[] { 1, 2, 3, 4 })]
                    public void Test(byte[] buffer) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeParameterIsArrayOfEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeParameterIsArrayOfEnum
                {
                    public enum TestEnum { A,B,C }

                    [TestCase(new TestEnum[] { })]
                    [TestCase(new TestEnum[] { TestEnum.A })]
                    [TestCase(new TestEnum[] { TestEnum.A, TestEnum.B })]
                    [TestCase(new TestEnum[] { TestEnum.A, TestEnum.B, TestEnum.C })]
                    public void Test(TestEnum[] e) { }
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
                    [TestCase(new object[] { 1, 2.0 })]
                    public void Test(int i, double d) { }
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
                    [TestCase(new object[] { 1, {|#0:2.0|} })]
                    public void Test(int i, int ii) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(0).WithArguments("1", "double", "ii", "int"));
        }

        [Test]
        public async Task AnalyzeArgumentIsStringConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsStringConvertedToEnum
                {
                    public enum TestEnum { A,B,C }

                    [TestCase({|#0:"A"|})]
                    [TestCase({|#1:"B"|})]
                    public void Test(TestEnum e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "string", "e", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsStringConvertedToEnum.TestEnum"), Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(1).WithArguments("0", "string", "e", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsStringConvertedToEnum.TestEnum"));
        }

        [Test]
        public async Task AnalyzeArgumentIsEnumConvertedToString()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentIsEnumConvertedToString
                {
                    public enum TestEnum { A,B,C }

                    [TestCase({|#0:TestEnum.A|})]
                    [TestCase({|#1:TestEnum.B|})]
                    public void Test(string e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsEnumConvertedToString.TestEnum", "e", "string"), Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(1).WithArguments("0", "NUnit.Analyzers.Tests.Targets.TestCaseUsage.AnalyzeArgumentIsEnumConvertedToString.TestEnum", "e", "string"));
        }

        [Test]
        public async Task AnalyzeArgumentHasImplicitConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentHasImplicitConversion
                {
                    [TestCase(uint.MaxValue)]
                    public void Test(long e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsStringConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                class C
                {
                    [TestCase("A")]
                    public void Test(CustomType p) { }
                }

                [TypeConverter(typeof(CustomTypeConverter))]
                struct CustomType { }
                class CustomTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsNullConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                class C
                {
                    [TestCase(null)]
                    public void Test(CustomType p) { }
                }

                [TypeConverter(typeof(CustomTypeConverter))]
                struct CustomType { }
                class CustomTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsStringConvertedToTypeWithCustomConverterOnBaseType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                class C
                {
                    [TestCase("A")]
                    public void Test(CustomType p) { }
                }

                class CustomType : BaseType { }
                [TypeConverter(typeof(BaseTypeConverter))]
                class BaseType { }
                class BaseTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeArgumentIsIntConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                using System.ComponentModel;

                class C
                {
                    [TestCase(2)]
                    public void Test(CustomType p) { }
                }

                [TypeConverter(typeof(CustomTypeConverter))]
                struct CustomType { }
                class CustomTypeConverter : TypeConverter { }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "int", "a", "char"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentTypeIsIncorrect
                {
                    [TestCase({|#0:2|})]
                    public void Test(char a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenOneArgumentTypeOfMultipleIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 1, "int", "expected", "System.Type"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenOneArgumentTypeOfMultipleIsIncorrect
                {
                    [TestCase("string", {|#0:12|}, 27, "other")]
                    public void Test(string actual, Type expected, int value, string other) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenLastArgumentTypeOfTwoIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 1, "int", "expected", "bool"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenLastArgumentTypeOfTwoIsIncorrect
                {
                    [TestCase("string", {|#0:12|})]
                    public void Test(string actual, bool expected) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeArgumentHasTypeNotAllowedInAttributes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeArgumentHasTypeNotAllowedInAttributes
                {
                    [TestCase(1.0m)]
                    public void Test(decimal d) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToValueType()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "<null>", "a", "char"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToValueType
                {
                    [TestCase({|#0:null|})]
                    public void Test(char a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToNullableValueType
                {
                    [TestCase(null)]
                    public void Test(int? a) { }
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

                    [TestCase(null)]
                    public void Test(TestEnum? e) { }
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
                    [TestCase(null)]
                    public void Test(object? a) { }
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
                    [TestCase(null)]
                    public void Test(object a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNonNullableReferenceType()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture,
                    TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "<null>", "a", "object"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesNullToNullableType
                {
                    [TestCase({|#0:null|})]
                    public void Test(object a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesSuppressedNullToNonNullableReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesSuppressedNullToNonNullableType
                {
                    [TestCase(null!)]
                    public void Test(object a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesNullToNonNullableParamsReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesSuppressedNullToNonNullableType
                {
                    [TestCase("Hello", {|#0:null|})]
                    public void Test(params string[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(0).WithArguments("1", "object", "a", "string[]"));
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesSuppressedNullToNonNullableParamsReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesSuppressedNullToNonNullableType
                {
                    [TestCase("Hello", null!)]
                    public void Test(params string[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesValueToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesValueToNullableType
                {
                    [TestCase(2)]
                    public void Test(int? a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentPassesValueToNullableEnumType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenArgumentPassesValueToNullableEnumType
                {
                    public enum TestEnum { A,B,C }

                    [TestCase(TestEnum.A)]
                    [TestCase(TestEnum.B)]
                    public void Test(TestEnum? e) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenNotEnoughRequiredArgumentsAreProvided()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage).WithMessage(TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenNotEnoughRequiredArgumentsAreProvided
                {
                    [{|#0:TestCase(2)|}]
                    public void Test(int a, char b) { }
                }
            """);
            var message = "The TestCaseAttribute provided too few arguments. Expected '2', but got '1'.";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTooManyRequiredArgumentsAreProvided()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage).WithMessage(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTooManyRequiredArgumentsAreProvided
                {
                    [{|#0:TestCase(2, 'b')|}]
                    public void Test(int a) { }
                }
            """);
            var message = "The TestCaseAttribute provided too many arguments. Expected '1', but got '2'.";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage).WithMessage(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided
                {
                    [{|#0:TestCase(2, 'b', 2d)|}]
                    public void Test(int a, char b = 'c') { }
                }
            """);
            var message = "The TestCaseAttribute provided too many arguments. Expected '2', but got '3'.";
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithMessage(message).WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided
                {
                    [TestCase(1, 2, 3, 4)]
                    public void Test(int a, int b, params int[] c) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided
                {
                    [TestCase]
                    public void Test(params object[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect
                {
                    [TestCase("a")]
                    public void Test(params string[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                              0, "int", "a", "string[]"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect
                {
                    [TestCase({|#0:2|})]
                    public void Test(params string[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType()
        {
            var expectedDiagnostic = Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithMessage(string.Format(CultureInfo.InvariantCulture, TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                              0, "<null>", "a", "int[]"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType
                {
                    [TestCase({|#0:null|})]
                    public void Test(params int[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToReferenceType
                {
                    [TestCase(null)]
                    public void Test(params string[]? a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesExactType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesExactType
                {
                    [TestCase(new int[0])]
                    public void Test(params int[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenMethodHasOnlyParamsAndArgumentImplicitlyConvertedToType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentImplicitlyConvertedToType
                {
                    [TestCase(byte.MaxValue)]
                    public void Test(params int[] a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsAReferenceToConstantThatNeedsConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsAReferenceToConstantThatNeedsConversion
                {
                    const int value = 42;

                    [TestCase(value)]
                    public void Test(decimal a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsANegativeNumberThatNeedsConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsANegativeNumberThatNeedsConversion
                {
                    [TestCase(-600)]
                    public void Test(decimal a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSoleParameterIsObjectArrayAndProvidedOneArg()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedOneArg
                {
                    [TestCase("a")]
                    public void Test(object[] array) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSoleParameterIsObjectArrayAndProvidedTwoArgs()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedTwoArgs
                {
                    [TestCase("a", "b")]
                    public void Test(object[] array) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSoleParameterIsObjectArrayAndProvidedThreeArgs()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedThreeArgs
                {
                    [TestCase("a", "b", "c")]
                    public void Test(object[] array) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenSoleParameterIsObjectArrayAndProvidedDifferentTypes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedDifferentTypes
                {
                    [TestCase(1, "b")]
                    public void Test(object[] array) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasTypeParameterArgumentType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasTypeParameterArgumentType
                {
                    [TestCase(1)]
                    public void TestWithGenericParameter<T>(T arg1) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

#if NET6_0_OR_GREATER
        [TestCaseSource(nameof(SpecialConversions_NET6))]
        public async Task AnalyzeWhenArgumentIsSpecialConversionNET6(string value, Type targetType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                public sealed class AnalyzeWhenArgumentIsSpecialConversion
                {
                    [TestCase("{{value}}")]
                    public void Test({{targetType.Name}} a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif

#if NUNIT4
#if NET6_0_OR_GREATER
        [Test]
        public async Task AnalyzeWhenArgumentIsCorrectGenericTypeParameter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsGenericTypeParameter
                {
                    [TestCase<byte>(2)]
                    public void Test(byte a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentsAreCorrectGenericTypeParameter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsGenericTypeParameter
                {
                    [TestCase<byte, uint>(2, 3)]
                    public void Test(byte a, uint b) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenArgumentIsWrongGenericTypeParameter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsGenericTypeParameter
                {
                    [TestCase<double>({|#0:2|})]
                    public void Test(int a) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "double", "a", "int"));
        }

        [Test]
        public async Task AnalyzeWhenArgumentsAreWrongGenericTypeParameter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                class AnalyzeWhenArgumentIsGenericTypeParameter
                {
                    [TestCase<double, int>({|#0:2|}, {|#1:3|})]
                    public void Test(int a, uint b) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(0).WithArguments("0", "double", "a", "int"), Verify.Diagnostic(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage).WithLocation(1).WithArguments("1", "int", "b", "uint"));
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasTypeParameterArgumentTypeAndGenericTestCase()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                public sealed class AnalyzeWhenTestMethodHasTypeParameterArgumentType
                {
                    [TestCase<int>(1)]
                    [TestCase<uint>(1)]
                    [TestCase<float>(1)]
                    [TestCase<double>(1)]
                    public void TestWithGenericParameter<T>(T arg1) { }
                }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif

        [Test]
        public async Task AnalyzeWhenTestMethodHasImplicitlySuppliedCancellationTokenParameterDueToCancelAfterOnMethod()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(100)]
                    [CancelAfter(50)]
                    public async Task InfiniteLoopWithCancelAfter(int delayInMs, CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(delayInMs, cancellationToken).ConfigureAwait(false);
                        }
                        }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasImplicitlySuppliedCancellationTokenParameterDueToCancelAfterOnClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                [CancelAfter(50)]
                public class TestClass
                {
                    [TestCase(100)]
                    public async Task InfiniteLoopWithCancelAfter(int delayInMs, CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(delayInMs, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasImplicitlySuppliedCancellationTokenParameterDueToCancelAfterOnBaseClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [CancelAfter(50)]
                public abstract class BaseClass
                {
                }

                [TestFixture]
                public class TestClass : BaseClass
                {
                    [TestCase(100)]
                    public async Task InfiniteLoopWithCancelAfter(int delayInMs, CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(delayInMs, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenTestMethodHasNoImplicitlySuppliedCancellationTokenParameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(100)]
                    [CancelAfter(50)]
                    public async Task InfiniteLoopWith50msCancelAfter(int delayInMs)
                    {
                        CancellationToken cancellationToken = TestContext.CurrentContext.CancellationToken;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(delayInMs, cancellationToken).ConfigureAwait(false);
                        }
                    }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task WhenTestMethodHasCancellationTokenParameter()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [{|#0:TestCase(100)|}]
                    public async Task InfiniteLoopWith50msCancelAfter(int delayInMs, CancellationToken cancellationToken)
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(delayInMs, cancellationToken).ConfigureAwait(false);
                        }
                    }
            """, "using System.Threading;");

            await Verify.VerifyAnalyzerAsync(testCode, Verify.Diagnostic(AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage).WithLocation(0).WithArguments("2", "1"));
        }
#endif

        [TestCase("arg: new[] { 1, 2, 3, }", "T[]", "")]
        [TestCase("""arg: new[] { "a", "b", "c", }""", "IEnumerable<T>", "using System.Collections.Generic;")]
        [TestCase("arg: new[] { typeof(int), typeof(string), typeof(double), }", "IList<T>", "using System.Collections.Generic;")]
        [TestCase("arg: new[] { 1ul, 2ul, 3ul, }", "ICollection<T>", "using System.Collections.Generic;")]
        public async Task AnalyzeWhenTestMethodHasGenericsEnumerable(string arrayArgument, string parameterType, string additionalUsings)
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase({{arrayArgument}})]
                    public void TestCollectionMethod<T>({{parameterType}} collection)
                    {
                        Assert.That(collection, Is.Not.Empty);
                    }
            """, additionalUsings);

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
