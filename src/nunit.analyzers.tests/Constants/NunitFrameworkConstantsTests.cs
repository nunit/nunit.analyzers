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
    public sealed class NunitFrameworkConstantsTests
    {
        private static readonly (string Constant, string TypeName)[] NameOfSource = new (string Constant, string TypeName)[]
        {
            (nameof(NunitFrameworkConstants.NameOfEqualConstraintWithin), nameof(EqualConstraint.Within)),
            (nameof(NunitFrameworkConstants.NameOfIs), nameof(Is)),
            (nameof(NunitFrameworkConstants.NameOfIsFalse), nameof(Is.False)),
            (nameof(NunitFrameworkConstants.NameOfIsTrue), nameof(Is.True)),
            (nameof(NunitFrameworkConstants.NameOfIsEqualTo), nameof(Is.EqualTo)),
            (nameof(NunitFrameworkConstants.NameOfIsEquivalentTo), nameof(Is.EquivalentTo)),
            (nameof(NunitFrameworkConstants.NameOfIsSubsetOf), nameof(Is.SubsetOf)),
            (nameof(NunitFrameworkConstants.NameOfIsSupersetOf), nameof(Is.SupersetOf)),
            (nameof(NunitFrameworkConstants.NameOfIsNot), nameof(Is.Not)),
            (nameof(NunitFrameworkConstants.NameOfIsNotEqualTo), nameof(Is.Not.EqualTo)),
            (nameof(NunitFrameworkConstants.NameOfIsSameAs), nameof(Is.SameAs)),
            (nameof(NunitFrameworkConstants.NameOfIsSamePath), nameof(Is.SamePath)),
            (nameof(NunitFrameworkConstants.NameOfNull), nameof(Is.Null)),
            (nameof(NunitFrameworkConstants.NameOfIsGreaterThan), nameof(Is.GreaterThan)),
            (nameof(NunitFrameworkConstants.NameOfIsGreaterThanOrEqualTo), nameof(Is.GreaterThanOrEqualTo)),
            (nameof(NunitFrameworkConstants.NameOfIsLessThan), nameof(Is.LessThan)),
            (nameof(NunitFrameworkConstants.NameOfIsLessThanOrEqualTo), nameof(Is.LessThanOrEqualTo)),
            (nameof(NunitFrameworkConstants.NameOfIsPositive), nameof(Is.Positive)),
            (nameof(NunitFrameworkConstants.NameOfIsZero), nameof(Is.Zero)),
            (nameof(NunitFrameworkConstants.NameOfIsNaN), nameof(Is.NaN)),
            (nameof(NunitFrameworkConstants.NameOfIsEmpty), nameof(Is.Empty)),
            (nameof(NunitFrameworkConstants.NameOfIsInstanceOf), nameof(Is.InstanceOf)),

            (nameof(NunitFrameworkConstants.NameOfContains), nameof(Contains)),
            (nameof(NunitFrameworkConstants.NameOfContainsItem), nameof(Contains.Item)),

            (nameof(NunitFrameworkConstants.NameOfDoes), nameof(Does)),
            (nameof(NunitFrameworkConstants.NameOfDoesNot), nameof(Does.Not)),
            (nameof(NunitFrameworkConstants.NameOfDoesContain), nameof(Does.Contain)),
            (nameof(NunitFrameworkConstants.NameOfDoesStartWith), nameof(Does.StartWith)),
            (nameof(NunitFrameworkConstants.NameOfDoesEndWith), nameof(Does.EndWith)),

            (nameof(NunitFrameworkConstants.NameOfHas), nameof(Has)),
            (nameof(NunitFrameworkConstants.NameOfHasProperty), nameof(Has.Property)),
            (nameof(NunitFrameworkConstants.NameOfHasCount), nameof(Has.Count)),
            (nameof(NunitFrameworkConstants.NameOfHasLength), nameof(Has.Length)),
            (nameof(NunitFrameworkConstants.NameOfHasMessage), nameof(Has.Message)),
            (nameof(NunitFrameworkConstants.NameOfHasInnerException), nameof(Has.InnerException)),
            (nameof(NunitFrameworkConstants.NameOfHasNo), nameof(Has.No)),

            (nameof(NunitFrameworkConstants.NameOfMultiple), nameof(Assert.Multiple)),
            (nameof(NunitFrameworkConstants.NameOfThrows), nameof(Assert.Throws)),

            (nameof(NunitFrameworkConstants.NameOfAssert), nameof(Assert)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsTrue), nameof(Assert.IsTrue)),
            (nameof(NunitFrameworkConstants.NameOfAssertTrue), nameof(Assert.True)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsFalse), nameof(Assert.IsFalse)),
            (nameof(NunitFrameworkConstants.NameOfAssertFalse), nameof(Assert.False)),
            (nameof(NunitFrameworkConstants.NameOfAssertAreEqual), nameof(Assert.AreEqual)),
            (nameof(NunitFrameworkConstants.NameOfAssertAreNotEqual), nameof(Assert.AreNotEqual)),
            (nameof(NunitFrameworkConstants.NameOfAssertAreSame), nameof(Assert.AreSame)),
            (nameof(NunitFrameworkConstants.NameOfAssertAreNotSame), nameof(Assert.AreNotSame)),
            (nameof(NunitFrameworkConstants.NameOfAssertNull), nameof(Assert.Null)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsNull), nameof(Assert.IsNull)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsNotNull), nameof(Assert.IsNotNull)),
            (nameof(NunitFrameworkConstants.NameOfAssertNotNull), nameof(Assert.NotNull)),
            (nameof(NunitFrameworkConstants.NameOfAssertThat), nameof(Assert.That)),
            (nameof(NunitFrameworkConstants.NameOfAssertGreater), nameof(Assert.Greater)),
            (nameof(NunitFrameworkConstants.NameOfAssertGreaterOrEqual), nameof(Assert.GreaterOrEqual)),
            (nameof(NunitFrameworkConstants.NameOfAssertLess), nameof(Assert.Less)),
            (nameof(NunitFrameworkConstants.NameOfAssertLessOrEqual), nameof(Assert.LessOrEqual)),
            (nameof(NunitFrameworkConstants.NameOfAssertZero), nameof(Assert.Zero)),
            (nameof(NunitFrameworkConstants.NameOfAssertNotZero), nameof(Assert.NotZero)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsNaN), nameof(Assert.IsNaN)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsEmpty), nameof(Assert.IsEmpty)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsNotEmpty), nameof(Assert.IsNotEmpty)),
            (nameof(NunitFrameworkConstants.NameOfAssertContains), nameof(Assert.Contains)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsInstanceOf), nameof(Assert.IsInstanceOf)),
            (nameof(NunitFrameworkConstants.NameOfAssertIsNotInstanceOf), nameof(Assert.IsNotInstanceOf)),

            (nameof(NunitFrameworkConstants.NameOfAssertCatch), nameof(Assert.Catch)),
            (nameof(NunitFrameworkConstants.NameOfAssertCatchAsync), nameof(Assert.CatchAsync)),
            (nameof(NunitFrameworkConstants.NameOfAssertThrows), nameof(Assert.Throws)),
            (nameof(NunitFrameworkConstants.NameOfAssertThrowsAsync), nameof(Assert.ThrowsAsync)),

            (nameof(NunitFrameworkConstants.NameOfConstraint), nameof(Constraint)),

            (nameof(NunitFrameworkConstants.NameOfTestCaseAttribute), nameof(TestCaseAttribute)),
            (nameof(NunitFrameworkConstants.NameOfTestCaseSourceAttribute), nameof(TestCaseSourceAttribute)),
            (nameof(NunitFrameworkConstants.NameOfTestAttribute), nameof(TestAttribute)),
            (nameof(NunitFrameworkConstants.NameOfParallelizableAttribute), nameof(ParallelizableAttribute)),
            (nameof(NunitFrameworkConstants.NameOfValueSourceAttribute), nameof(ValueSourceAttribute)),

            (nameof(NunitFrameworkConstants.NameOfExpectedResult), nameof(TestAttribute.ExpectedResult)),

            (nameof(NunitFrameworkConstants.NameOfConstraintExpressionAnd), nameof(EqualConstraint.And)),
            (nameof(NunitFrameworkConstants.NameOfConstraintExpressionOr), nameof(EqualConstraint.Or)),
            (nameof(NunitFrameworkConstants.NameOfConstraintExpressionWith), nameof(ConstraintExpression.With)),

            (nameof(NunitFrameworkConstants.NameOfIgnoreCase), nameof(EqualConstraint.IgnoreCase)),
            (nameof(NunitFrameworkConstants.NameOfUsing), nameof(EqualConstraint.Using)),
        };

        private static readonly (string Constant, Type Type)[] FullNameOfTypeSource = new (string Constant, Type Type)[]
        {
            (nameof(NunitFrameworkConstants.FullNameOfTypeIs), typeof(Is)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeTestCaseAttribute), typeof(TestCaseAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute), typeof(TestCaseSourceAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeTestAttribute), typeof(TestAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeParallelizableAttribute), typeof(ParallelizableAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeValueSourceAttribute), typeof(ValueSourceAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeITestBuilder), typeof(ITestBuilder)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeISimpleTestBuilder), typeof(ISimpleTestBuilder)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeIParameterDataSource), typeof(IParameterDataSource)),

            (nameof(NunitFrameworkConstants.FullNameOfTypeOneTimeSetUpAttribute), typeof(OneTimeSetUpAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeOneTimeTearDownAttribute), typeof(OneTimeTearDownAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeSetUpAttribute), typeof(SetUpAttribute)),
            (nameof(NunitFrameworkConstants.FullNameOfTypeTearDownAttribute), typeof(TearDownAttribute)),

            (nameof(NunitFrameworkConstants.FullNameOfSameAsConstraint), typeof(SameAsConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfSomeItemsConstraint), typeof(SomeItemsConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfEqualToConstraint), typeof(EqualConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfEndsWithConstraint), typeof(EndsWithConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfRegexConstraint), typeof(RegexConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfEmptyStringConstraint), typeof(EmptyStringConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfSamePathConstraint), typeof(SamePathConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfSamePathOrUnderConstraint), typeof(SamePathOrUnderConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfStartsWithConstraint), typeof(StartsWithConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfSubPathConstraint), typeof(SubPathConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfSubstringConstraint), typeof(SubstringConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfContainsConstraint), typeof(ContainsConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfActualValueDelegate), typeof(ActualValueDelegate<>)),
            (nameof(NunitFrameworkConstants.FullNameOfDelayedConstraint), typeof(DelayedConstraint)),
            (nameof(NunitFrameworkConstants.FullNameOfTestDelegate), typeof(TestDelegate)),
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

            Assert.That(parameterNames, Does.Contain(NunitFrameworkConstants.NameOfActualParameter));
            Assert.That(parameterNames, Does.Contain(NunitFrameworkConstants.NameOfExpectedParameter));
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

            Assert.That(parameterNames, Does.Contain(NunitFrameworkConstants.NameOfActualParameter));
            Assert.That(parameterNames, Does.Contain(NunitFrameworkConstants.NameOfExpressionParameter));
            Assert.That(parameterNames, Does.Contain(NunitFrameworkConstants.NameOfConditionParameter));
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

            Assert.That(parameterNames, Does.Contain(NunitFrameworkConstants.NameOfConditionParameter));
        }

        [Test]
        public void NUnitAssemblyNameTest()
        {
            Assert.That(
                NunitFrameworkConstants.NUnitFrameworkAssemblyName,
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
                typeof(NunitFrameworkConstants).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral)
                .Select(f => f.Name)
                .Where(name => !name.EndsWith("Parameter", System.StringComparison.Ordinal))
                .Where(name => name.StartsWith(prefix, System.StringComparison.Ordinal));

            Assert.That(testedNames, Is.EquivalentTo(allNames));
        }

        private static string GetValue(string fieldName) =>
            (string)typeof(NunitFrameworkConstants).GetField(fieldName).GetRawConstantValue()!;
    }
}
