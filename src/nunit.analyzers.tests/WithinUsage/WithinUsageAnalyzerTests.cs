using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using Verify = NUnit.Analyzers.Tests.Verifiers.NUnitCSharpAnalyzerVerifier<NUnit.Analyzers.WithinUsage.WithinUsageAnalyzer>;

namespace NUnit.Analyzers.Tests.WithinUsage
{
    public class WithinUsageAnalyzerTests
    {
        private static readonly DiagnosticResult expectedDiagnostic =
            Verify.Diagnostic(AnalyzerIdentifiers.WithinIncompatibleTypes);

#if !NUNIT4
        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
#endif
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForStrings(string constraintName)
        {
            string testCode = TestUtility.WrapInTestMethod(
                $$"""Assert.That("1", Is.Not.Empty.And.{{constraintName}}("1").{|#0:Within|}(0.1));""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

#if !NUNIT4
        [TestCase(NUnitFrameworkConstants.NameOfIsEqualTo)]
#endif
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThan)]
        [TestCase(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo)]
        public async Task AnalyzeWhenAppliedToInvertedEqualityConstraintForStrings(string constraintName)
        {
            string testCode = TestUtility.WrapInTestMethod(
                $$"""Assert.That("1", Is.Not.{{constraintName}}("1").{|#0:Within|}(0.1));""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

#if !NUNIT4
        [Test]
        public async Task AnalyzeWhenAppliedToNotEqualConstraintForStrings()
        {
            string testCode = TestUtility.WrapInTestMethod(
                """Assert.That("1", Is.Not.EqualTo("1").{|#0:Within|}(0.1));""");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
#endif

        [Test]
        public async Task AnalyzeWhenUsingAllGenericParameters()
        {
            string testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(1.0, 1.05)]
                    public void Test<T>(T a, T b)
                    {
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenUsingSomeGenericParameters()
        {
            string testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings("""
                    [TestCase(1.0, 1.05)]
                    public void Test<T>(T a, T b)
                        where T : struct
                    {
                        Assert.That(a, Is.EqualTo(b).Within(0.1).And.Not.LessThan("0.0").{|#0:Within|}("5"));
                    }
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

#if NUNIT5

#if !NETFRAMEWORK
        [TestCase("Half")]
        [TestCase("Int128")]
        [TestCase("UInt128")]
#endif
        [TestCase("nint")]
        [TestCase("nuint")]
        public async Task AnalyzeWhenUsingNewType(string typeName)
        {
            string testCode = TestUtility.WrapInTestMethod($$"""
                        {{typeName}} a = ({{typeName}})30;
                        {{typeName}} b = ({{typeName}})50;
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
            """);
            await Verify.VerifyAnalyzerAsync(testCode);
        }
#endif

        [Test]
        public async Task AnalyzeWhenUsingIntPtr()
        {
            string testCode = TestUtility.WrapInTestMethod($$"""
                        IntPtr a = (IntPtr)30;
                        IntPtr b = (IntPtr)50;
                        Assert.That(a, Is.EqualTo(b).{|#0:Within|}(0.1));
            """);
            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForArraysOfValidTypes()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = new[] {1, 2};
                        var b = new[] {1.1, 2.1};
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForArraysOfInvalidTypes()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = new[] {"Hello", "World"};
                        var b = new[] {"Hi", "There"};
                        Assert.That(a, Is.EqualTo(b).{|#0:Within|}(0.1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForIncompatibleTuples()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = ("1", "1");
                        var b = ("1.1", "1");
                        Assert.That(a, Is.EqualTo(b).{|#0:Within|}(0.1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForMixedCompatibleTuples()
        {
#if NUNIT5 && !NETFRAMEWORK
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = (1, "1");
                        var b = ((Half)1.01, "1");
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
            """);
#else
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = (1, "1");
                        var b = (1.01, "1");
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
            """);
#endif
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForNamedTuples()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = (value: 1, name: "1");
                        var b = (value: 1.01, name: "1");
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("1", "1")]
        [TestCase("0.1234", "0.2134")]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForNumericTypes(string a, string b)
        {
            string testCode = TestUtility.WrapInTestMethod(
                $"Assert.That({a}, Is.EqualTo({b}).Within(0.1));");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForNumericTuples()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = (1, 1);
                        var b = (1.1, 1);
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForMixedValidTypeTuples()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = (1, DateTime.Now);
                        var b = (1.1, DateTime.Now);
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase(nameof(DateTimeOffset))]
        [TestCase(nameof(DateTime))]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForDateTypes(string typeName)
        {
            string testCode = TestUtility.WrapInTestMethod($$"""
                        var a = {{typeName}}.Now;
                        var b = {{typeName}}.Now;
                        Assert.That(a, Is.EqualTo(b).Within(TimeSpan.Zero));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForDateTypeKind()
        {
            string testCode = TestUtility.WrapInTestMethod($$"""
                        var a = DateTimeKind.Local;
                        var b = DateTimeKind.Local;
                        Assert.That(a, Is.EqualTo(b).{|#0:Within|}(1));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAppliedToEqualityConstraintForTimeSpanTypes()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        var a = TimeSpan.Zero;
                        var b = TimeSpan.Zero;
                        Assert.That(a, Is.EqualTo(b).Within(TimeSpan.Zero));
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToCollectionsOfValidTypes()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        double value = 5.5;
                        int value2 = 6;
                        Assert.That(new List<double>() { value }, Is.EqualTo(new List<double> { value2 }).Within(2));
            """,
                "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToCollectionsOfInValidTypes()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        string value = "Value";
                        string value2 = "Value2";
                        Assert.That(new List<string>() { value }, Is.EqualTo(new List<string> { value2 }).{|#0:Within|}(2));
            """,
                "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAppliedToImmutableArrayOfValidTypes()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        double value = 5.5;
                        int value2 = 6;
                        Assert.That(ImmutableArray.Create(value), Is.EqualTo(ImmutableArray.Create(value2)).Within(2));
            """,
                "using System.Collections.Immutable;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeWhenAppliedToImmutableArrayOfInValidTypes()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        string value = "Value";
                        string value2 = "Value2";
                        Assert.That(ImmutableArray.Create(value), Is.EqualTo(ImmutableArray.Create(value2)).{|#0:Within|}(2));
            """,
                "using System.Collections.Immutable;");

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAppliedToChar()
        {
            string testCode = TestUtility.WrapInTestMethod($$"""
                        char a = 'a';
                        char b = 'b';
                        Assert.That(a, Is.EqualTo(b).{|#0:Within|}(2));
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task NestedCase()
        {
            string testCode = TestUtility.WrapInTestMethod($$"""
                        const string Key1 = "key1";
                        const string Key2 = "Key2";
                        const string Key3 = "Key3";
                        var a = new KeyValuePair<string, KeyValuePair<string, KeyValuePair<string, int>>>(
                            Key1, new KeyValuePair<string, KeyValuePair<string, int>>(Key2, new KeyValuePair<string, int>(Key3, 3)));
                        var b = new KeyValuePair<string, KeyValuePair<string, KeyValuePair<string, int>>>(
                            Key1, new KeyValuePair<string, KeyValuePair<string, int>>(Key2, new KeyValuePair<string, int>(Key3, 4)));
                        Assert.That(a, Is.EqualTo(b).Within(2));
            """,
            "using System.Collections.Generic;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeNonGenericDictionaryEntry()
        {
            string testCode = TestUtility.WrapInTestMethod($$"""
                        const string Value = "key";
                        var a = new DictionaryEntry(1, Value);
                        var b = new DictionaryEntry(2, Value);
                        Assert.That(a, Is.EqualTo(b).Within(2));
            """,
                "using System.Collections;");

            // By the time we see the '.Within' we have no idea about the types used.
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeOnNonGenericCollection()
        {
            string testCode = TestUtility.WrapInTestMethod("""
                        double value = 5.5;
                        int value2 = 6;
                        Assert.That(new ArrayList { value }, Is.EqualTo(new ArrayList { value2 }).Within(2));
            """,
                "using System.Collections;");

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task AnalyzeNonGenericSubclassOfGenericIEnumerable()
        {
            string testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public class TestClass
                {
                    public void TestMethod()
                    {
                        Assert.That(new DoubleCollection(1), Is.EqualTo(new DoubleCollection(2)).Within(0.001));
                    }

                    private sealed class DoubleCollection : IEnumerable<double>
                    {
                        public DoubleCollection(params double[] numbers) { }
                        public IEnumerator<double> GetEnumerator() => throw new NotImplementedException();
                        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                    }
                }
            """,
                additionalUsings: """
                    using System.Collections;
                     using System.Collections.Generic;
                """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("double")]
        [TestCase("int")]
        [TestCase("TimeSpan")]
        [TestCase("nint")]
#if NUNIT5 && !NETFRAMEWORK
        [TestCase("Half")]
        [TestCase("Int128")]
        [TestCase("UInt128")]
#endif
        public async Task AnalyzeWhenAppliedToEqualityConstraintForNullableValidTypes(string type)
        {
            string testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings($$"""
                    [TestCase(null, null)]
                    public void Test({{type}}? a, {{type}}? b)
                    {
                        Assert.That(a, Is.EqualTo(b).Within(0.1));
                    }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("class")]
        [TestCase("struct")]
        public async Task AnalyzeWhenAppliedToUserDefinedType(string kind)
        {
            string testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public sealed class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var instance = new UserDefinedType(1, 1.1, "1.1");

                        Assert.That(new UserDefinedType(1, 1.2, "1.1"), Is.EqualTo(instance).Within(0.1));
                    }

                    private {{kind}} UserDefinedType
                    {
                        public UserDefinedType(int valueA, double valueB, string valueC)
                        {
                            ValueA = valueA;
                            ValueB = valueB;
                            ValueC = valueC;
                        }

                        public int ValueA { get; }
                        public double ValueB { get; }
                        public string ValueC { get; }
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [TestCase("class")]
        [TestCase("struct")]
        public async Task AnalyzeWhenAppliedToUserDefinedTypeOverridingEquals(string kind)
        {
            string testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                [TestFixture]
                public sealed class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var instance = new UserDefinedType(1, 1.1, "1.1");

                        Assert.That(new UserDefinedType(1, 1.2, "1.1"), Is.EqualTo(instance).{|#0:Within|}(0.1));
                    }

                    private {{kind}} UserDefinedType
                    {
                        public UserDefinedType(int valueA, double valueB, string valueC)
                        {
                            ValueA = valueA;
                            ValueB = valueB;
                            ValueC = valueC;
                        }

                        public int ValueA { get; }
                        public double ValueB { get; }
                        public string ValueC { get; }

                        public override bool Equals(object? obj)
                        {
                            return obj is UserDefinedType other &&
                                other.ValueA == ValueA &&
                                other.ValueB == ValueB &&
                                other.ValueC == ValueC;
                        }

                        public override int GetHashCode()
                        {
                            return ValueA.GetHashCode() ^ ValueB.GetHashCode() ^ ValueC.GetHashCode();
                        }
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }

        [Test]
        public async Task AnalyzeWhenAppliedToRecord()
        {
            string testCode = TestUtility.WrapClassInNamespaceAndAddUsing("""
                [TestFixture]
                public sealed class TestClass
                {
                    [Test]
                    public void TestMethod()
                    {
                        var instance = new SomeRecord(1, 1.1, "1.1");

                        Assert.That(new SomeRecord(1, 1.2, "1.1"), Is.EqualTo(instance).{|#0:Within|}(0.1));
                    }

                    private sealed record SomeRecord
                    {
                        public SomeRecord(int valueA, double valueB, string valueC)
                        {
                            ValueA = valueA;
                            ValueB = valueB;
                            ValueC = valueC;
                        }

                        public int ValueA { get; }
                        public double ValueB { get; }
                        public string ValueC { get; }
                    }
                }
            """);

            await Verify.VerifyAnalyzerAsync(testCode, expectedDiagnostic.WithLocation(0));
        }
    }
}
