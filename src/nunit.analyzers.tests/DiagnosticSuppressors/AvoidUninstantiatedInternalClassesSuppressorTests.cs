using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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

            this.analyzer = new DefaultEnabledAnalyzer(this.analyzer);
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
    }
}
