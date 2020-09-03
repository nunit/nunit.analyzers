using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.TestCaseUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.TestCaseUsage
{
    [TestFixture]
    public sealed class TestCaseUsageAnalyzerTests
    {
        private readonly DiagnosticAnalyzer analyzer = new TestCaseUsageAnalyzer();

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
            var analyzer = new TestCaseUsageAnalyzer();
            var diagnostics = analyzer.SupportedDiagnostics;

            var expectedIdentifiers = new List<string>
            {
                AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage,
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage
            };
            CollectionAssert.AreEquivalent(expectedIdentifiers, diagnostics.Select(d => d.Id));

            foreach (var diagnostic in diagnostics)
            {
                Assert.That(diagnostic.Title.ToString(CultureInfo.InvariantCulture), Is.Not.Empty);
                Assert.That(diagnostic.Category, Is.EqualTo(Categories.Structure),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.Category)}");
                Assert.That(diagnostic.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Error),
                    $"{diagnostic.Id} : {nameof(DiagnosticDescriptor.DefaultSeverity)}");
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
        public void AnalyzeWhenAttributeIsNotInNUnit()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeIsNotInNUnit
    {
        [TestCase]
        public void ATest() { }

        private sealed class TestCaseAttribute : Attribute
        { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeIsTestAttribute()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeIsTestAttribute
    {
        [Test]
        public void ATest() { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeHasNoArguments()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeHasNoArguments
    {
        [TestCase]
        public void ATest() { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentIsCorrect
    {
        [TestCase(2)]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [TestCaseSource(nameof(SpecialConversions))]
        public void AnalyzeWhenArgumentIsSpecialConversion(string value, Type targetType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public sealed class AnalyzeWhenArgumentIsSpecialConversion
    {{
        [TestCase(""{value}"")]
        public void Test({targetType.Name} a) {{ }}
    }}");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsACast()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenArgumentIsACast
    {
        [TestCase((byte)2)]
        public void Test(byte a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsAPrefixedValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenArgumentIsAPrefixedValue
    {
        [TestCase(-2)]
        public void Test(int a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeArgumentIsEnum
    {
        public enum TestEnum { A,B,C }

        [TestCase(TestEnum.A)]
        [TestCase(TestEnum.B)]
        public void Test(TestEnum e) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsIntConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeArgumentIsIntConvertedToEnum
    {
        public enum TestEnum { A,B,C }

        [TestCase(0)]
        [TestCase(1)]
        public void Test(TestEnum e) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsEnumConvertedToInt()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeArgumentIsEnumConvertedToInt
    {
        public enum TestEnum { A,B,C }

        [TestCase(TestEnum.A)]
        [TestCase(TestEnum.B)]
        public void Test(int e) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsStringConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeArgumentIsStringConvertedToEnum
    {
        public enum TestEnum { A,B,C }

        [TestCase(↓""A"")]
        [TestCase(↓""B"")]
        public void Test(TestEnum e) { }
    }");
            AnalyzerAssert.Diagnostics<TestCaseUsageAnalyzer>(
                ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage),
                testCode);
        }

        [Test]
        public void AnalyzeArgumentIsEnumConvertedToString()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeArgumentIsEnumConvertedToString
    {
        public enum TestEnum { A,B,C }

        [TestCase(↓TestEnum.A)]
        [TestCase(↓TestEnum.B)]
        public void Test(string e) { }
    }");
            AnalyzerAssert.Diagnostics<TestCaseUsageAnalyzer>(
                ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage),
                testCode);
        }

        [Test]
        public void AnalyzeArgumentHasImplicitConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeArgumentHasImplicitConversion
    {
        [TestCase(uint.MaxValue)]
        public void Test(long e) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsStringConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    class C
    {
        [TestCase(""A"")]
        public void Test(CustomType p) { }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    struct CustomType { }
    class CustomTypeConverter : TypeConverter { }");

            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsNullConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    class C
    {
        [TestCase(null)]
        public void Test(CustomType p) { }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    struct CustomType { }
    class CustomTypeConverter : TypeConverter { }");

            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsStringConvertedToTypeWithCustomConverterOnBaseType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    class C
    {
        [TestCase(""A"")]
        public void Test(CustomType p) { }
    }

    class CustomType : BaseType { }
    [TypeConverter(typeof(BaseTypeConverter))]
    class BaseType { }
    class BaseTypeConverter : TypeConverter { }");

            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeArgumentIsIntConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    class C
    {
        [TestCase(↓2)]
        public void Test(CustomType p) { }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    struct CustomType { }
    class CustomTypeConverter : TypeConverter { }");

            AnalyzerAssert.Diagnostics<TestCaseUsageAnalyzer>(
                ExpectedDiagnostic.Create(AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage),
                testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture,
                    TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "int", "a", "char"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentTypeIsIncorrect
    {
        [TestCase(↓2)]
        public void Test(char a) { }
    }");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentPassesNullToValueType()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture,
                    TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage, 0, "<null>", "a", "char"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesNullToValueType
    {
        [TestCase(↓null)]
        public void Test(char a) { }
    }");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentPassesNullToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesNullToNullableType
    {
        [TestCase(null)]
        public void Test(int? a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentPassesValueToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesValueToNullableType
    {
        [TestCase(2)]
        public void Test(int? a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenNotEnoughRequiredArgumentsAreProvided()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage,
                TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenNotEnoughRequiredArgumentsAreProvided
    {
        [↓TestCase(2)]
        public void Test(int a, char b) { }
    }");
            var message = "The TestCaseAttribute provided too few arguments. Expected '2', but got '1'.";
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenTooManyRequiredArgumentsAreProvided()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage,
                TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTooManyRequiredArgumentsAreProvided
    {
        [↓TestCase(2, 'b')]
        public void Test(int a) { }
    }");
            var message = "The TestCaseAttribute provided too many arguments. Expected '1', but got '2'.";
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage,
                TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage);

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTooManyRequiredAndOptionalArgumentsAreProvided
    {
        [↓TestCase(2, 'b', 2d)]
        public void Test(int a, char b = 'c') { }
    }");
            var message = "The TestCaseAttribute provided too many arguments. Expected '2', but got '3'.";
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic.WithMessage(message), testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasRequiredAndParamsAndMoreArgumentsThanParametersAreProvided
    {
        [TestCase(1, 2, 3, 4)]
        public void Test(int a, int b, params int[] c) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndNoArgumentsAreProvided
    {
        [TestCase]
        public void Test(params object[] a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsCorrect
    {
        [TestCase(""a"")]
        public void Test(params string[] a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture, TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                              0, "int", "a", "string[]"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentTypeIsIncorrect
    {
        [TestCase(↓2)]
        public void Test(params string[] a) { }
    }");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
                string.Format(CultureInfo.InvariantCulture, TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                              0, "<null>", "a", "int[]"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToValueType
    {
        [TestCase(↓null)]
        public void Test(params int[] a) { }
    }");
            AnalyzerAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToReferenceType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesNullToReferenceType
    {
        [TestCase(null)]
        public void Test(params string[] a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesExactType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentPassesExactType
    {
        [TestCase(new int[0])]
        public void Test(params int[] a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenMethodHasOnlyParamsAndArgumentImplicitlyConvertedToType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenMethodHasOnlyParamsAndArgumentImplicitlyConvertedToType
    {
        [TestCase(byte.MaxValue)]
        public void Test(params int[] a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsAReferenceToConstantThatNeedsConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenArgumentIsAReferenceToConstantThatNeedsConversion
    {
        const int value = 42;

        [TestCase(value)]
        public void Test(decimal a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsANegativeNumberThatNeedsConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenArgumentIsANegativeNumberThatNeedsConversion
    {
        [TestCase(-600)]
        public void Test(decimal a) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSoleParameterIsObjectArrayAndProvidedOneArg()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedOneArg
    {
        [TestCase(""a"")]
        public void Test(object[] array) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSoleParameterIsObjectArrayAndProvidedTwoArgs()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedTwoArgs
    {
        [TestCase(""a"", ""b"")]
        public void Test(object[] array) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSoleParameterIsObjectArrayAndProvidedThreeArgs()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedThreeArgs
    {
        [TestCase(""a"", ""b"", ""c"")]
        public void Test(object[] array) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenSoleParameterIsObjectArrayAndProvidedDifferentTypes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    class AnalyzeWhenSoleParameterIsObjectArrayAndProvidedDifferentTypes
    {
        [TestCase(1, ""b"")]
        public void Test(object[] array) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }

        [Test]
        public void AnalyzeWhenTestMethodHasTypeParameterArgumentType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenTestMethodHasTypeParameterArgumentType
    {
        [TestCase(1)]
        public void TestWithGenericParameter<T>(T arg1) { }
    }");
            AnalyzerAssert.Valid<TestCaseUsageAnalyzer>(testCode);
        }
    }
}
