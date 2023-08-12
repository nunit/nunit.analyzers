using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ValuesUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ValuesUsage
{
    public class ValuesUsageAnalyzerTests
    {
        private readonly ValuesUsageAnalyzer analyzer = new ValuesUsageAnalyzer();

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
            var diagnostics = this.analyzer.SupportedDiagnostics;

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
        public void AnalyzeWhenAttributeIsNotInNUnit()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeIsNotInNUnit
    {
        [Test]
        public void ATest([Values] bool value) { }

        private sealed class ValuesAttribute : Attribute
        { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenAttributeHasNoArguments()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenAttributeHasNoArguments
    {
        [Test]
        public void ATest([Values] bool value) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentTypeIsCorrect()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentTypeIsCorrect
    {
        [Test]
        public void ATest([Values(true, false)] bool blah) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [TestCaseSource(nameof(SpecialConversions))]
        public void AnalyzeWhenArgumentIsSpecialConversion(string value, Type targetType)
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
    public sealed class AnalyzeWhenArgumentIsSpecialConversion
    {{
        [Test]
        public void Test([Values(""{value}"")] {targetType.Name} a) {{ }}
    }}");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsACast()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentIsACast
    {
        [Test]
        public void Test([Values((byte)2)] byte a) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentIsAPrefixedValue()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentIsAPrefixedValue
    {
        [Test]
        public void Test([Values(-2)] int a) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentIsEnum
    {
        public enum TestEnum { A,B,C }

        [Test]
        public void Test([Values(TestEnum.A, TestEnum.B)] TestEnum e) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsIntConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentIsIntConvertedToEnum
    {
        public enum TestEnum { A,B,C }

        [Test]
        public void Test([Values(0, 1)] TestEnum e) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsEnumConvertedToInt()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentIsEnumConvertedToInt
    {
        public enum TestEnum { A,B,C }

        [Test]
        public void Test([Values(TestEnum.A, TestEnum.B)] int e) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeParameterIsArray()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentIsArray
    {
        [Test]
        public void Test([Values(new byte[] { }, new byte[] { 1 }, new byte[] { 1, 2, 3 })] byte[] buffer) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsStringConvertedToEnum()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentIsStringConvertedToEnum
    {
        public enum TestEnum { A,B,C }

        [Test]
        public void Test([Values(""A"", ""B"")] TestEnum e) { }
    }");
            RoslynAssert.Diagnostics(this.analyzer,
                                     ExpectedDiagnostic.Create(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage),
                                     testCode);
        }

        [Test]
        public void AnalyzeArgumentIsEnumConvertedToString()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentIsEnumConvertedToString
    {
        public enum TestEnum { A,B,C }

        [Test]
        public void Test([Values(TestEnum.A, TestEnum.B)] string e) { }
    }");
            RoslynAssert.Diagnostics(this.analyzer,
                                     ExpectedDiagnostic.Create(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage),
                                     testCode);
        }

        [Test]
        public void AnalyzeArgumentHasImplicitConversion()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentHasImplicitConversion
    {
        [Test]
        public void Test([Values(uint.MaxValue)] long e) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsStringConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    public sealed class AnalyzeArgumentIsStringConvertedToTypeWithCustomConverter
    {
        [Test]
        public void Test([Values(""A"")] CustomType p) { }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    public struct CustomType { }
    public class CustomTypeConverter : TypeConverter { }");

            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsNullConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    public sealed class AnalyzeArgumentIsNullConvertedToTypeWithCustomConverter
    {
        [Test]
        public void Test([Values(null)] CustomType p) { }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    public struct CustomType { }
    public class CustomTypeConverter : TypeConverter { }");

            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsStringConvertedToTypeWithCustomConverterOnBaseType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    public sealed class AnalyzeArgumentIsStringConvertedToTypeWithCustomConverterOnBaseType
    {
        [Test]
        public void Test([Values(""A"")] CustomType p) { }
    }

    public class CustomType : BaseType { }
    [TypeConverter(typeof(BaseTypeConverter))]
    public class BaseType { }
    public class BaseTypeConverter : TypeConverter { }");

            RoslynAssert.Valid(this.analyzer, testCode);
        }

        [Test]
        public void AnalyzeArgumentIsIntConvertedToTypeWithCustomConverter()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    using System.ComponentModel;

    public sealed class AnalyzeArgumentIsIntConvertedToTypeWithCustomConverter
    {
        [Test]
        public void Test([Values(2)] CustomType p) { }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    public struct CustomType { }
    public class CustomTypeConverter : TypeConverter { }");

            RoslynAssert.Diagnostics(this.analyzer,
                                     ExpectedDiagnostic.Create(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage),
                                     testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentTypeIsIncorrect()
        {
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage,
                                                               string.Format(CultureInfo.InvariantCulture,
                                                                             ValuesUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                                                                             1, "object", "blah", "bool"));
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class Foo
    {
        [Test]
        public void ATest([Values(true, null)] bool blah) { }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeArgumentHasTypeNotAllowedInAttributes()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeArgumentHasTypeNotAllowedInAttributes
    {
        [Test]
        public void Test([Values(1.0m)] decimal d) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode, Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
        }

        [Test]
        public void AnalyzeWhenArgumentPassesNullToValueType()
        {
            // TODO: Can we get this to report <null> instead of object?[]
            var expectedDiagnostic = ExpectedDiagnostic.Create(AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage,
                                                               string.Format(CultureInfo.InvariantCulture,
                                                                             ValuesUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                                                                             0, "object?[]", "a", "char"));

            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesNullToValueType
    {
        [Test]
        public void Test([Values(null)] char a) { }
    }");
            RoslynAssert.Diagnostics(this.analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void AnalyzeWhenArgumentPassesNullToNullableType()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(@"
    public sealed class AnalyzeWhenArgumentPassesNullToNullableType
    {
        [Test]
        public void Test([Values(null)] int? a) { }
    }");
            RoslynAssert.Valid(this.analyzer, testCode);
        }
    }
}
