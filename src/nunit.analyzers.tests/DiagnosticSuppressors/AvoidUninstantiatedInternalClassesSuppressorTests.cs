using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.DiagnosticSuppressors;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    internal class AvoidUninstantiatedInternalClassesSuppressorTests
    {
        private const string CalculatorClass = @"
            internal sealed class Calculator
            {
                private readonly Calculation _calculation;

                public Calculator(Calculation calculation) => _calculation = calculation;

                public int Calculate(int op1, int op2)
                {
                    return _calculation == Calculation.Add ? op1 + op2 : op1 - op2;
                }

                public enum Calculation
                {
                    Add,
                    Subtract,
                }
            }
        ";

        private const string CalculatorTest = @"
            internal sealed class CalculatorTests
            {
                [Test]
                public void TestAdd()
                {
                    var instance = new Calculator(Calculator.Calculation.Add);
                    var result = instance.Calculate(3, 4);
                    Assert.That(result, Is.EqualTo(7));
                }

                [Test]
                public void TestSubtract()
                {
                    var instance = new Calculator(Calculator.Calculation.Subtract);
                    var result = instance.Calculate(3, 4);
                    Assert.That(result, Is.EqualTo(-1));
                }
            }
        ";

        private const string TestDataClass = @"
            internal sealed class TestData : IEnumerable
            {
                public IEnumerator GetEnumerator()
                {
                    yield return ""Hello"";
                    yield return ""World"";
                }
            }
        ";

        private const string TestFixtureWithParameterizedTestMethod = @"
            [TestFixture]
            public sealed class TestFixture
            {
                [TestCaseSource(typeof(TestData))]
                public void ParameterizedTestMethod(string value)
                {
                    Assert.That(value, Is.Not.Null);
                    Assert.That(value, Has.Length.EqualTo(5));
                }
            }
         ";

        private const string ParameterizedTestFixture = @"
            [TestFixtureSource(typeof(TestData))]
            public sealed class ParameterizedTestFixture
            {
                private readonly string value;

                public ParameterizedTestFixture(string value)
                {
                    this.value = value;
                }

                [Test]
                public void MustBeNotNull()
                {
                    Assert.That(value, Is.Not.Null);
                }

                [Test]
                public void MustBeFiveCharactersLong()
                {
                    Assert.That(value, Has.Length.EqualTo(5));
                }
            }
        ";

        private static readonly Settings settingSearchingAllSourcesForTestCaseUsages =
            Settings.Default.WithAnalyzerConfig("dotnet_diagnostic.NUnit3003.search_all_code_for_use_as_data_source = true");

        private static readonly DiagnosticSuppressor suppressor = new AvoidUninstantiatedInternalClassSuppressor();
        private DiagnosticAnalyzer analyzer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Find the NetAnalyzers assembly (note version should match the one referenced)
            string netAnalyzersPath = Path.Combine(PathHelper.GetNuGetPackageDirectory(),
                "microsoft.codeanalysis.netanalyzers/8.0.0/analyzers/dotnet/cs/Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll");
            Assembly netAnalyzerAssembly = Assembly.LoadFrom(netAnalyzersPath);
            Type analyzerType = netAnalyzerAssembly.GetType("Microsoft.CodeQuality.CSharp.Analyzers.Maintainability.CSharpAvoidUninstantiatedInternalClasses", true)!;
            this.analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType)!;
        }

        [Test]
        public async Task NonTestClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(CalculatorClass);

            await TestHelpers.NotSuppressed(this.analyzer, suppressor, testCode).ConfigureAwait(true);
        }

        [Test]
        public async Task TestClass()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($@"
                {CalculatorClass}
                {CalculatorTest}
            ");

            await TestHelpers.Suppressed(this.analyzer, suppressor, testCode).ConfigureAwait(true);
        }

        [Test]
        public async Task TestClassNotUsedAsTestSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing(TestDataClass, "using System.Collections;");

            await TestHelpers.NotSuppressed(this.analyzer, suppressor, testCode, settingSearchingAllSourcesForTestCaseUsages).ConfigureAwait(true);
        }

        [Test]
        public async Task TestClassUsedAsTestCaseSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                {{TestDataClass}}
                {{TestFixtureWithParameterizedTestMethod}}
                """, additionalUsings: "using System.Collections;");

            await TestHelpers.Suppressed(this.analyzer, suppressor, testCode, settingSearchingAllSourcesForTestCaseUsages).ConfigureAwait(true);
        }

        [Test]
        public async Task TestClassUsedAsTestFixtureSource()
        {
            var testCode = TestUtility.WrapClassInNamespaceAndAddUsing($$"""
                {{TestDataClass}}
                {{ParameterizedTestFixture}}
                """, additionalUsings: "using System.Collections;");

            await TestHelpers.Suppressed(this.analyzer, suppressor, testCode, settingSearchingAllSourcesForTestCaseUsages).ConfigureAwait(true);
        }

        [Test]
        public async Task TestClassUsedInMultipleSourceFiles()
        {
            var testDataCode = TestUtility.WrapClassInNamespaceAndAddUsing(TestDataClass, additionalUsings: "using System.Collections;");
            var testFixtureCode = TestUtility.WrapClassInNamespaceAndAddUsing(TestFixtureWithParameterizedTestMethod);
            var parameterizedTestFixtureCode = TestUtility.WrapClassInNamespaceAndAddUsing(ParameterizedTestFixture);

            await TestHelpers.Suppressed(this.analyzer, suppressor, [testDataCode, testFixtureCode, parameterizedTestFixtureCode], settingSearchingAllSourcesForTestCaseUsages).ConfigureAwait(true);

            // Test that the suppressor does not suppress when the setting is not set
            await TestHelpers.NotSuppressed(this.analyzer, suppressor, [testDataCode, testFixtureCode, parameterizedTestFixtureCode],
                Settings.Default).ConfigureAwait(true);

            // Test that the suppressor does not suppress when the setting is explicitly disabled
            await TestHelpers.NotSuppressed(this.analyzer, suppressor, [testDataCode, testFixtureCode, parameterizedTestFixtureCode],
                Settings.Default.WithAnalyzerConfig("dotnet_diagnostic.NUnit3003.search_all_code_for_use_as_data_source = false")).ConfigureAwait(true);
        }
    }
}
