using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace NUnit.Analyzers.Tests.Constants
{
    /// <summary>
    /// Tests to ensure that the string constants in the analyzer project correspond
    /// to the NUnit concepts that they represent.
    /// </summary>
    [TestFixture]
    internal sealed class NUnitFrameworkConstantsTests : BaseNUnitFrameworkConstantsTests<NUnitFrameworkConstants>
    {
        public static readonly (string Constant, string TypeName)[] NameOfSource =
        [
            (nameof(NUnitFrameworkConstants.NameOfIs), nameof(Is)),
            (nameof(NUnitFrameworkConstants.NameOfIsFalse), nameof(Is.False)),
            (nameof(NUnitFrameworkConstants.NameOfIsTrue), nameof(Is.True)),
            (nameof(NUnitFrameworkConstants.NameOfIsEqualTo), nameof(Is.EqualTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsEquivalentTo), nameof(Is.EquivalentTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsSubsetOf), nameof(Is.SubsetOf)),
            (nameof(NUnitFrameworkConstants.NameOfIsSupersetOf), nameof(Is.SupersetOf)),
            (nameof(NUnitFrameworkConstants.NameOfIsNot), nameof(Is.Not)),
            (nameof(NUnitFrameworkConstants.NameOfIsSameAs), nameof(Is.SameAs)),
            (nameof(NUnitFrameworkConstants.NameOfIsSamePath), nameof(Is.SamePath)),
            (nameof(NUnitFrameworkConstants.NameOfIsNull), nameof(Is.Null)),
            (nameof(NUnitFrameworkConstants.NameOfIsGreaterThan), nameof(Is.GreaterThan)),
            (nameof(NUnitFrameworkConstants.NameOfIsGreaterThanOrEqualTo), nameof(Is.GreaterThanOrEqualTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsLessThan), nameof(Is.LessThan)),
            (nameof(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo), nameof(Is.LessThanOrEqualTo)),
            (nameof(NUnitFrameworkConstants.NameOfIsPositive), nameof(Is.Positive)),
            (nameof(NUnitFrameworkConstants.NameOfIsNegative), nameof(Is.Negative)),
            (nameof(NUnitFrameworkConstants.NameOfIsZero), nameof(Is.Zero)),
            (nameof(NUnitFrameworkConstants.NameOfIsNaN), nameof(Is.NaN)),
            (nameof(NUnitFrameworkConstants.NameOfIsEmpty), nameof(Is.Empty)),
            (nameof(NUnitFrameworkConstants.NameOfIsInstanceOf), nameof(Is.InstanceOf)),
            (nameof(NUnitFrameworkConstants.NameOfIsAssignableFrom), nameof(Is.AssignableFrom)),
            (nameof(NUnitFrameworkConstants.NameOfIsAll), nameof(Is.All)),
            (nameof(NUnitFrameworkConstants.NameOfIsUnique), nameof(Is.Unique)),
            (nameof(NUnitFrameworkConstants.NameOfIsOrdered), nameof(Is.Ordered)),

            (nameof(NUnitFrameworkConstants.NameOfContains), nameof(Contains)),
            (nameof(NUnitFrameworkConstants.NameOfContainsItem), nameof(Contains.Item)),

            (nameof(NUnitFrameworkConstants.NameOfDoes), nameof(Does)),
            (nameof(NUnitFrameworkConstants.NameOfDoesNot), nameof(Does.Not)),
            (nameof(NUnitFrameworkConstants.NameOfDoesContain), nameof(Does.Contain)),
            (nameof(NUnitFrameworkConstants.NameOfDoesStartWith), nameof(Does.StartWith)),
            (nameof(NUnitFrameworkConstants.NameOfDoesEndWith), nameof(Does.EndWith)),
            (nameof(NUnitFrameworkConstants.NameOfDoesMatch), nameof(Does.Match)),

            (nameof(NUnitFrameworkConstants.NameOfHas), nameof(Has)),
            (nameof(NUnitFrameworkConstants.NameOfHasProperty), nameof(Has.Property)),
            (nameof(NUnitFrameworkConstants.NameOfHasCount), nameof(Has.Count)),
            (nameof(NUnitFrameworkConstants.NameOfHasLength), nameof(Has.Length)),
            (nameof(NUnitFrameworkConstants.NameOfHasMessage), nameof(Has.Message)),
            (nameof(NUnitFrameworkConstants.NameOfHasInnerException), nameof(Has.InnerException)),
            (nameof(NUnitFrameworkConstants.NameOfHasNo), nameof(Has.No)),
            (nameof(NUnitFrameworkConstants.NameOfHasMember), nameof(Has.Member)),

            (nameof(NUnitFrameworkConstants.NameOfMultiple), nameof(Assert.Multiple)),

            (nameof(NUnitFrameworkConstants.NameOfOut), nameof(TestContext.Out)),
            (nameof(NUnitFrameworkConstants.NameOfWrite), nameof(TestContext.Out.Write)),
            (nameof(NUnitFrameworkConstants.NameOfWriteLine), nameof(TestContext.Out.WriteLine)),

            (nameof(NUnitFrameworkConstants.NameOfThrows), nameof(Throws)),
            (nameof(NUnitFrameworkConstants.NameOfThrowsArgumentException), nameof(Throws.ArgumentException)),
            (nameof(NUnitFrameworkConstants.NameOfThrowsArgumentNullException), nameof(Throws.ArgumentNullException)),
            (nameof(NUnitFrameworkConstants.NameOfThrowsInvalidOperationException), nameof(Throws.InvalidOperationException)),
            (nameof(NUnitFrameworkConstants.NameOfThrowsTargetInvocationException), nameof(Throws.TargetInvocationException)),

            (nameof(NUnitFrameworkConstants.NameOfAssert), nameof(Assert)),
            (nameof(NUnitFrameworkConstants.NameOfAssume), nameof(Assume)),

            (nameof(NUnitFrameworkConstants.NameOfAssertPass), nameof(Assert.Pass)),
            (nameof(NUnitFrameworkConstants.NameOfAssertFail), nameof(Assert.Fail)),
            (nameof(NUnitFrameworkConstants.NameOfAssertWarn), nameof(Assert.Warn)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIgnore), nameof(Assert.Ignore)),
            (nameof(NUnitFrameworkConstants.NameOfAssertInconclusive), nameof(Assert.Inconclusive)),

            (nameof(NUnitFrameworkConstants.NameOfAssertThat), nameof(Assert.That)),

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
            (nameof(NUnitFrameworkConstants.NameOfValuesAttribute), nameof(ValuesAttribute)),

            (nameof(NUnitFrameworkConstants.NameOfOneTimeSetUpAttribute), nameof(OneTimeSetUpAttribute)),
            (nameof(NUnitFrameworkConstants.NameOfOneTimeTearDownAttribute), nameof(OneTimeTearDownAttribute)),
            (nameof(NUnitFrameworkConstants.NameOfSetUpAttribute), nameof(SetUpAttribute)),
            (nameof(NUnitFrameworkConstants.NameOfTearDownAttribute), nameof(TearDownAttribute)),

            (nameof(NUnitFrameworkConstants.NameOfExpectedResult), nameof(TestAttribute.ExpectedResult)),

            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionAnd), nameof(EqualConstraint.And)),
            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionOr), nameof(EqualConstraint.Or)),
            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionWith), nameof(ConstraintExpression.With)),

            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintIgnoreCase), nameof(EqualConstraint.IgnoreCase)),
            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintUsing), nameof(EqualConstraint.Using)),
            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintWithin), nameof(EqualConstraint.Within)),
            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintAsCollection), nameof(EqualConstraint.AsCollection)),
        ];

        public static readonly (string Constant, Type Type)[] FullNameOfTypeSource =
        [
            (nameof(NUnitFrameworkConstants.FullNameOfTypeIs), typeof(Is)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseAttribute), typeof(TestCaseAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute), typeof(TestCaseSourceAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestAttribute), typeof(TestAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeParallelizableAttribute), typeof(ParallelizableAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeValueSourceAttribute), typeof(ValueSourceAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeValuesAttribute), typeof(ValuesAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeITestBuilder), typeof(ITestBuilder)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeISimpleTestBuilder), typeof(ISimpleTestBuilder)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeIFixtureBuilder), typeof(IFixtureBuilder)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeIParameterDataSource), typeof(IParameterDataSource)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseData), typeof(TestCaseData)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseParameters), typeof(TestCaseParameters)),

            (nameof(NUnitFrameworkConstants.FullNameOfTypeOneTimeSetUpAttribute), typeof(OneTimeSetUpAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeOneTimeTearDownAttribute), typeof(OneTimeTearDownAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeSetUpAttribute), typeof(SetUpAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTearDownAttribute), typeof(TearDownAttribute)),

            (nameof(NUnitFrameworkConstants.FullNameOfFixtureLifeCycleAttribute), typeof(FixtureLifeCycleAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfLifeCycle), typeof(LifeCycle)),

            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestContext), typeof(TestContext)),

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
            (nameof(NUnitFrameworkConstants.FullNameOfThrows), typeof(Throws)),
        ];

        protected override IEnumerable<string> Names => Constant(NameOfSource);

        protected override IEnumerable<string> FullNames => Constant(FullNameOfTypeSource);

        [Test]
        public void TestPrefixOfAllEqualConstraints()
        {
            Assert.That(NUnitFrameworkConstants.PrefixOfAllEqualToConstraints, Is.EqualTo("NUnit.Framework.Constraints.Equal"));
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

        [Test]
        public void NUnitAssemblyNameTest()
        {
            // We are testing that the value of the constant is correct
#pragma warning disable NUnit2007 // The actual value should not be a constant
            Assert.That(
                NUnitFrameworkConstants.NUnitFrameworkAssemblyName,
                Is.EqualTo(typeof(Assert).Assembly.GetName().Name));
#pragma warning restore NUnit2007 // The actual value should not be a constant
        }
    }
}
