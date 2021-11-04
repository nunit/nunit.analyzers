using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;

namespace NUnit.Analyzers.Tests.Constants
{
    /// <summary>
    /// Tests to ensure that the string constants in the analyzer project correspond
    /// to the NUnit concepts that they represent.
    /// </summary>
    [TestFixture]
    public sealed class NUnitFrameworkConstantsTests
    {
        private static readonly (string Constant, string TypeName)[] NameOfSource = new (string Constant, string TypeName)[]
        {
            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintWithin), nameof(EqualConstraint.Within)),
            (nameof(NUnitFrameworkConstants.NameOfIs), nameof(Is)),
            (nameof(NUnitFrameworkConstants.NameOfIsFalse), nameof(Is.False)),
            (nameof(NUnitFrameworkConstants.NameOfIsTrue), nameof(Is.True)),
            (nameof(NUnitFrameworkConstants.NameOfIsEqualTo), nameof(Is.EqualTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsEquivalentTo), nameof(Is.EquivalentTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsSubsetOf), nameof(Is.SubsetOf)),
            (nameof(NUnitFrameworkConstants.NameOfIsSupersetOf), nameof(Is.SupersetOf)),
            (nameof(NUnitFrameworkConstants.NameOfIsNot), nameof(Is.Not)),
            (nameof(NUnitFrameworkConstants.NameOfIsNotEqualTo), nameof(Is.Not.EqualTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsSameAs), nameof(Is.SameAs)),
            (nameof(NUnitFrameworkConstants.NameOfIsSamePath), nameof(Is.SamePath)),
            (nameof(NUnitFrameworkConstants.NameOfNull), nameof(Is.Null)),
            (nameof(NUnitFrameworkConstants.NameOfIsGreaterThan), nameof(Is.GreaterThan)),
            (nameof(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo), nameof(Is.GreaterThanOrEqualTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsLessThan), nameof(Is.LessThan)),
            (nameof(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo), nameof(Is.LessThanOrEqualTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsPositive), nameof(Is.Positive)),
            (nameof(NUnitFrameworkConstants.NameOfIsZero), nameof(Is.Zero)),
            (nameof(NUnitFrameworkConstants.NameOfIsNaN), nameof(Is.NaN)),
            (nameof(NUnitFrameworkConstants.NameOfIsEmpty), nameof(Is.Empty)),
            (nameof(NUnitFrameworkConstants.NameOfIsInstanceOf), nameof(Is.InstanceOf)),

            (nameof(NUnitFrameworkConstants.NameOfContains), nameof(Contains)),
            (nameof(NUnitFrameworkConstants.NameOfContainsItem), nameof(Contains.Item)),

            (nameof(NUnitFrameworkConstants.NameOfDoes), nameof(Does)),
            (nameof(NUnitFrameworkConstants.NameOfDoesNot), nameof(Does.Not)),
            (nameof(NUnitFrameworkConstants.NameOfDoesContain), nameof(Does.Contain)),
            (nameof(NUnitFrameworkConstants.NameOfDoesStartWith), nameof(Does.StartWith)),
            (nameof(NUnitFrameworkConstants.NameOfDoesEndWith), nameof(Does.EndWith)),

            (nameof(NUnitFrameworkConstants.NameOfHas), nameof(Has)),
            (nameof(NUnitFrameworkConstants.NameOfHasProperty), nameof(Has.Property)),
            (nameof(NUnitFrameworkConstants.NameOfHasCount), nameof(Has.Count)),
            (nameof(NUnitFrameworkConstants.NameOfHasLength), nameof(Has.Length)),
            (nameof(NUnitFrameworkConstants.NameOfHasMessage), nameof(Has.Message)),
            (nameof(NUnitFrameworkConstants.NameOfHasInnerException), nameof(Has.InnerException)),
            (nameof(NUnitFrameworkConstants.NameOfHasNo), nameof(Has.No)),

            (nameof(NUnitFrameworkConstants.NameOfMultiple), nameof(Assert.Multiple)),
            (nameof(NUnitFrameworkConstants.NameOfThrows), nameof(Assert.Throws)),

            (nameof(NUnitFrameworkConstants.NameOfAssert), nameof(Assert)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsTrue), nameof(Assert.IsTrue)),
            (nameof(NUnitFrameworkConstants.NameOfAssertTrue), nameof(Assert.True)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsFalse), nameof(Assert.IsFalse)),
            (nameof(NUnitFrameworkConstants.NameOfAssertFalse), nameof(Assert.False)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreEqual), nameof(Assert.AreEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreNotEqual), nameof(Assert.AreNotEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreSame), nameof(Assert.AreSame)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreNotSame), nameof(Assert.AreNotSame)),
            (nameof(NUnitFrameworkConstants.NameOfAssertNull), nameof(Assert.Null)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNull), nameof(Assert.IsNull)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNotNull), nameof(Assert.IsNotNull)),
            (nameof(NUnitFrameworkConstants.NameOfAssertNotNull), nameof(Assert.NotNull)),
            (nameof(NUnitFrameworkConstants.NameOfAssertThat), nameof(Assert.That)),
            (nameof(NUnitFrameworkConstants.NameOfAssertGreater), nameof(Assert.Greater)),
            (nameof(NUnitFrameworkConstants.NameOfAssertGreaterOrEqual), nameof(Assert.GreaterOrEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertLess), nameof(Assert.Less)),
            (nameof(NUnitFrameworkConstants.NameOfAssertLessOrEqual), nameof(Assert.LessOrEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertZero), nameof(Assert.Zero)),
            (nameof(NUnitFrameworkConstants.NameOfAssertNotZero), nameof(Assert.NotZero)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNaN), nameof(Assert.IsNaN)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsEmpty), nameof(Assert.IsEmpty)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNotEmpty), nameof(Assert.IsNotEmpty)),
            (nameof(NUnitFrameworkConstants.NameOfAssertContains), nameof(Assert.Contains)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsInstanceOf), nameof(Assert.IsInstanceOf)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNotInstanceOf), nameof(Assert.IsNotInstanceOf)),

            (nameof(NUnitFrameworkConstants.NameOfAssertCatch), nameof(Assert.Catch)),
            (nameof(NUnitFrameworkConstants.NameOfAssertCatchAsync), nameof(Assert.CatchAsync)),
            (nameof(NUnitFrameworkConstants.NameOfAssertThrows), nameof(Assert.Throws)),
            (nameof(NUnitFrameworkConstants.NameOfAssertThrowsAsync), nameof(Assert.ThrowsAsync)),

            (nameof(NUnitFrameworkConstants.NameOfConstraint), nameof(Constraint)),

            (nameof(NUnitFrameworkConstants.NameOfTestCaseAttribute), nameof(TestCaseAttribute)),
            (nameof(NUnitFrameworkConstants.NameOfTestCaseSourceAttribute), nameof(TestCaseSourceAttribute)),
            (nameof(NUnitFrameworkConstants.NameOfTestAttribute), nameof(TestAttribute)),
            (nameof(NUnitFrameworkConstants.NameOfParallelizableAttribute), nameof(ParallelizableAttribute)),
            (nameof(NUnitFrameworkConstants.NameOfValueSourceAttribute), nameof(ValueSourceAttribute)),

            (nameof(NUnitFrameworkConstants.NameOfExpectedResult), nameof(TestAttribute.ExpectedResult)),

            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionAnd), nameof(EqualConstraint.And)),
            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionOr), nameof(EqualConstraint.Or)),
            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionWith), nameof(ConstraintExpression.With)),

            (nameof(NUnitFrameworkConstants.NameOfIgnoreCase), nameof(EqualConstraint.IgnoreCase)),
            (nameof(NUnitFrameworkConstants.NameOfUsing), nameof(EqualConstraint.Using)),
        };

        private static readonly (string Constant, Type Type)[] FullNameOfTypeSource = new (string Constant, Type Type)[]
        {
            (nameof(NUnitFrameworkConstants.FullNameOfTypeIs), typeof(Is)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseAttribute), typeof(TestCaseAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute), typeof(TestCaseSourceAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestAttribute), typeof(TestAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeParallelizableAttribute), typeof(ParallelizableAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeValueSourceAttribute), typeof(ValueSourceAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeITestBuilder), typeof(ITestBuilder)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeISimpleTestBuilder), typeof(ISimpleTestBuilder)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeIParameterDataSource), typeof(IParameterDataSource)),

            (nameof(NUnitFrameworkConstants.FullNameOfTypeOneTimeSetUpAttribute), typeof(OneTimeSetUpAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeOneTimeTearDownAttribute), typeof(OneTimeTearDownAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeSetUpAttribute), typeof(SetUpAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTearDownAttribute), typeof(TearDownAttribute)),

            (nameof(NUnitFrameworkConstants.FullNameOfSameAsConstraint), typeof(SameAsConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfSomeItemsConstraint), typeof(SomeItemsConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfEqualToConstraint), typeof(EqualConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfEndsWithConstraint), typeof(EndsWithConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfRegexConstraint), typeof(RegexConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfEmptyStringConstraint), typeof(EmptyStringConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfSamePathConstraint), typeof(SamePathConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfSamePathOrUnderConstraint), typeof(SamePathOrUnderConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfStartsWithConstraint), typeof(StartsWithConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfSubPathConstraint), typeof(SubPathConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfSubstringConstraint), typeof(SubstringConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfContainsConstraint), typeof(ContainsConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfActualValueDelegate), typeof(ActualValueDelegate<>)),
            (nameof(NUnitFrameworkConstants.FullNameOfDelayedConstraint), typeof(DelayedConstraint)),
            (nameof(NUnitFrameworkConstants.FullNameOfTestDelegate), typeof(TestDelegate)),
        };

        [TestCaseSource(nameof(NameOfSource))]
        public void TestNameOfConstants((string Constant, string TypeName) pair)
        {
            Assert.That(GetValue(pair.Constant), Is.EqualTo(pair.TypeName), pair.Constant);
        }

        [TestCaseSource(nameof(FullNameOfTypeSource))]
        public void TestFullNameOfConstants((string Constant, Type Type) pair)
        {
            Assert.That(GetValue(pair.Constant), Is.EqualTo(pair.Type.FullName), pair.Constant);
        }

        [Test]
        public void NameOfAssertAreEqualParameters()
        {
            var parameters = typeof(Assert).GetMethods()
                .First(m => m.Name == nameof(Assert.AreEqual))
                .GetParameters();
            var parameterNames = parameters.Select(p => p.Name);

            Assert.That(parameterNames, Does.Contain(NUnitFrameworkConstants.NameOfActualParameter));
            Assert.That(parameterNames, Does.Contain(NUnitFrameworkConstants.NameOfExpectedParameter));
        }

        [Test]
        public void NameOfAssertThatParameters()
        {
            var parameterNames = typeof(Assert).GetMethods()
                .Where(m => m.Name == nameof(Assert.That))
                .SelectMany(m => m.GetParameters())
                .Select(p => p.Name)
                .Distinct()
                .ToArray();

            Assert.That(parameterNames, Does.Contain(NUnitFrameworkConstants.NameOfActualParameter));
            Assert.That(parameterNames, Does.Contain(NUnitFrameworkConstants.NameOfExpressionParameter));
            Assert.That(parameterNames, Does.Contain(NUnitFrameworkConstants.NameOfConditionParameter));
        }

        [TestCase(nameof(Assert.True))]
        [TestCase(nameof(Assert.IsTrue))]
        [TestCase(nameof(Assert.False))]
        [TestCase(nameof(Assert.IsFalse))]
        [TestCase(nameof(Assert.IsTrue))]
        public void NameOfAssertConditionParameters(string method)
        {
            var parameters = typeof(Assert).GetMethods()
                .First(m => m.Name == method)
                .GetParameters();
            var parameterNames = parameters.Select(p => p.Name);

            Assert.That(parameterNames, Does.Contain(NUnitFrameworkConstants.NameOfConditionParameter));
        }

        [Test]
        public void NUnitAssemblyNameTest()
        {
            Assert.That(
                NUnitFrameworkConstants.NUnitFrameworkAssemblyName,
                Is.EqualTo(typeof(Assert).Assembly.GetName().Name));
        }

        [Test]
        public void EnsureAllNameOfDefinitionsAreTested()
        {
            EnsureAllNameDefinitionsAreTested("NameOf", NameOfSource.Select(pair => pair.Constant));
        }

        [Test]
        public void EnsureAllFullNameOfTypeDefinitionsAreTested()
        {
            EnsureAllNameDefinitionsAreTested("FullNameOf", FullNameOfTypeSource.Select(pair => pair.Constant));
        }

        private static void EnsureAllNameDefinitionsAreTested(string prefix, IEnumerable<string> testedNames)
        {
            IEnumerable<string> allNames =
                typeof(NUnitFrameworkConstants).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral)
                .Select(f => f.Name)
                .Where(name => !name.EndsWith("Parameter", System.StringComparison.Ordinal))
                .Where(name => name.StartsWith(prefix, System.StringComparison.Ordinal));

            Assert.That(testedNames, Is.EquivalentTo(allNames));
        }

        private static string GetValue(string fieldName) =>
            (string)typeof(NUnitFrameworkConstants).GetField(fieldName).GetRawConstantValue()!;
    }
}
