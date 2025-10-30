using System;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.NullConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.NullConstraintUsage
{
    public class NullConstraintUsageAnalyzerTests
    {
        private static readonly DiagnosticAnalyzer analyzer = new NullConstraintUsageAnalyzer();
        private static readonly ExpectedDiagnostic expectedDiagnostic =
            ExpectedDiagnostic.Create(AnalyzerIdentifiers.NullConstraintUsage);

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void AnalyzeWhenActualIsNonNullableValueType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = default(int);
                Assert.That(actual, ↓{constraint});");
            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void AnalyzeWhenActualIsNonNullableValueTypeDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = default(int);
                Assert.That(() => actual, ↓{constraint});");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void AnalyzeWhenActualIsTaskDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Assert.That(() => Task.FromResult(3), ↓{constraint});");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsNullableValueType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                bool? actual = false;
                Assert.That(actual, {constraint});");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsReferenceType(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new string[] {{ ""TestString"" }};
                Assert.That(actual, {constraint});");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsReferenceTypeDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var actual = new string[] {{ ""TestString"" }};
                Assert.That(() => actual, {constraint});");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [Test]
        public void ValidWhenUsedWithPropertyOperator()
        {
            var testCode = TestUtility.WrapMethodInClassNamespaceAndAddUsings(@"
                public struct TestStruct
                {
                    public string RefTypeProp { get; set; }
                }
                
                [Test]
                public void TestMethod()
                {
                    var actual = new TestStruct();
                    Assert.That(actual, Has.Property(""RefTypeProp"").Null);
                }");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsTask(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                var task = Task.CompletedTask;
                Assert.That(task, {constraint});");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsAction(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Action<bool> action = b => {{ }};
                Assert.That(action, {constraint});");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void ValidWhenActualIsFunc(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Func<bool, bool> function = b => b;
                Assert.That(function, {constraint});");

            RoslynAssert.Valid(analyzer, testCode);
        }

        [TestCase("Is.Null")]
        [TestCase("Is.Not.Null")]
        public void AnalyzeWhenActualIsTestDelegate(string constraint)
        {
            var testCode = TestUtility.WrapInTestMethod($@"
                Action<bool> action = b => {{ }};
                Assert.That(() => action(true), ↓{constraint});");

            RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void ActualNUnitFunction()
        {
            bool functionCalled = false;

#pragma warning disable IDE0039 // Use local function
            Func<bool> function = () =>
            {
                functionCalled = true;
                return false;
            };
#pragma warning restore IDE0039 // Use local function

            Assert.Multiple(() =>
            {
                Assert.That(function, Is.Not.Null);
                Assert.That(functionCalled, Is.False);

#pragma warning disable NUnit2057 // Unnecessary DelegateCreation
                Assert.That(() => function, Is.Not.Null);
#pragma warning restore NUnit2057 // Unnecessary DelegateCreation
                Assert.That(functionCalled, Is.False);

#pragma warning disable NUnit2057 // Unnecessary DelegateCreation
#pragma warning disable NUnit2023 // Invalid NullConstraint usage
                Assert.That(() => function(), Is.Not.Null);
#pragma warning restore NUnit2023 // Invalid NullConstraint usage
#pragma warning restore NUnit2057 // Unnecessary DelegateCreation
                Assert.That(functionCalled, Is.True);
            });
        }

        [Test]
        public void ActualNUnitAsyncFunction()
        {
            bool functionCalled = false;

#pragma warning disable IDE0039 // Use local function
            Func<Task<bool>> asyncFunction = () =>
            {
                functionCalled = true;
                return Task.FromResult(false);
            };
#pragma warning restore IDE0039 // Use local function

            Assert.Multiple(() =>
            {
                Assert.That(asyncFunction, Is.Not.Null);
                Assert.That(functionCalled, Is.False);

#pragma warning disable NUnit2057 // Unnecessary DelegateCreation
                Assert.That(() => asyncFunction, Is.Not.Null);
#pragma warning restore NUnit2057 // Unnecessary DelegateCreation
                Assert.That(functionCalled, Is.False);

#pragma warning disable NUnit2057 // Unnecessary DelegateCreation
#pragma warning disable NUnit2023 // Invalid NullConstraint usage
                Assert.That(() => asyncFunction(), Is.Not.Null);
#pragma warning restore NUnit2023 // Invalid NullConstraint usage
#pragma warning restore NUnit2057 // Unnecessary DelegateCreation
                Assert.That(functionCalled, Is.True);
            });
        }

        [Test]
        public void ActualNUnitForAction()
        {
            bool actionCalled = false;

#pragma warning disable IDE0039 // Use local function
            Action action = () => actionCalled = true;
#pragma warning restore IDE0039 // Use local function

            Assert.Multiple(() =>
            {
                Assert.That(action, Is.Not.Null);
                Assert.That(actionCalled, Is.False);

#pragma warning disable NUnit2057 // Unnecessary DelegateCreation
                Assert.That(() => action, Is.Not.Null);
#pragma warning restore NUnit2057 // Unnecessary DelegateCreation
                Assert.That(actionCalled, Is.False);

#pragma warning disable NUnit2057 // Unnecessary DelegateCreation
#pragma warning disable NUnit2023 // Invalid NullConstraint usage
                Assert.That(() => action(), Is.Not.Null);
#pragma warning restore NUnit2023 // Invalid NullConstraint usage
#pragma warning restore NUnit2057 // Unnecessary DelegateCreation

#if EXPECT_TEST_DELEGATE_TO_BE_CALLED
            // The above test succeeds, but does not call the action!
            Assert.That(actionCalled, Is.True);
#endif
            });
        }
    }
}
