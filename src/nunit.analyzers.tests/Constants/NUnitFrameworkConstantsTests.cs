using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

#if NUNIT4
using NUnit.Framework.Legacy;
#else
using ClassicAssert = NUnit.Framework.Assert;
#endif

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
            (nameof(NUnitFrameworkConstants.NameOfIsZero), nameof(Is.Zero)),
            (nameof(NUnitFrameworkConstants.NameOfIsNaN), nameof(Is.NaN)),
            (nameof(NUnitFrameworkConstants.NameOfIsEmpty), nameof(Is.Empty)),
            (nameof(NUnitFrameworkConstants.NameOfIsInstanceOf), nameof(Is.InstanceOf)),
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
#if NUNIT4
            (nameof(NUnitFrameworkConstants.NameOfMultipleAsync), nameof(Assert.MultipleAsync)),
#else
            (nameof(NUnitFrameworkConstants.NameOfMultipleAsync), "MultipleAsync"),
#endif

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

            (nameof(NUnitFrameworkConstants.NameOfAssertIsTrue), nameof(ClassicAssert.IsTrue)),
            (nameof(NUnitFrameworkConstants.NameOfAssertTrue), nameof(ClassicAssert.True)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsFalse), nameof(ClassicAssert.IsFalse)),
            (nameof(NUnitFrameworkConstants.NameOfAssertFalse), nameof(ClassicAssert.False)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreEqual), nameof(ClassicAssert.AreEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreNotEqual), nameof(ClassicAssert.AreNotEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreSame), nameof(ClassicAssert.AreSame)),
            (nameof(NUnitFrameworkConstants.NameOfAssertAreNotSame), nameof(ClassicAssert.AreNotSame)),
            (nameof(NUnitFrameworkConstants.NameOfAssertNull), nameof(ClassicAssert.Null)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNull), nameof(ClassicAssert.IsNull)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNotNull), nameof(ClassicAssert.IsNotNull)),
            (nameof(NUnitFrameworkConstants.NameOfAssertNotNull), nameof(ClassicAssert.NotNull)),
            (nameof(NUnitFrameworkConstants.NameOfAssertThat), nameof(ClassicAssert.That)),
            (nameof(NUnitFrameworkConstants.NameOfAssertGreater), nameof(ClassicAssert.Greater)),
            (nameof(NUnitFrameworkConstants.NameOfAssertGreaterOrEqual), nameof(ClassicAssert.GreaterOrEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertLess), nameof(ClassicAssert.Less)),
            (nameof(NUnitFrameworkConstants.NameOfAssertLessOrEqual), nameof(ClassicAssert.LessOrEqual)),
            (nameof(NUnitFrameworkConstants.NameOfAssertZero), nameof(ClassicAssert.Zero)),
            (nameof(NUnitFrameworkConstants.NameOfAssertNotZero), nameof(ClassicAssert.NotZero)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNaN), nameof(ClassicAssert.IsNaN)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsEmpty), nameof(ClassicAssert.IsEmpty)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNotEmpty), nameof(ClassicAssert.IsNotEmpty)),
            (nameof(NUnitFrameworkConstants.NameOfAssertContains), nameof(ClassicAssert.Contains)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsInstanceOf), nameof(ClassicAssert.IsInstanceOf)),
            (nameof(NUnitFrameworkConstants.NameOfAssertIsNotInstanceOf), nameof(ClassicAssert.IsNotInstanceOf)),

            (nameof(NUnitFrameworkConstants.NameOfAssertCatch), nameof(Assert.Catch)),
            (nameof(NUnitFrameworkConstants.NameOfAssertCatchAsync), nameof(Assert.CatchAsync)),
            (nameof(NUnitFrameworkConstants.NameOfAssertThrows), nameof(Assert.Throws)),
            (nameof(NUnitFrameworkConstants.NameOfAssertThrowsAsync), nameof(Assert.ThrowsAsync)),

            (nameof(NUnitFrameworkConstants.NameOfStringAssert), nameof(StringAssert)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertContains), nameof(StringAssert.Contains)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertDoesNotContain), nameof(StringAssert.DoesNotContain)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertStartsWith), nameof(StringAssert.StartsWith)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertDoesNotStartWith), nameof(StringAssert.DoesNotStartWith)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertEndsWith), nameof(StringAssert.EndsWith)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertDoesNotEndWith), nameof(StringAssert.DoesNotEndWith)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertAreEqualIgnoringCase), nameof(StringAssert.AreEqualIgnoringCase)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertAreNotEqualIgnoringCase), nameof(StringAssert.AreNotEqualIgnoringCase)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertIsMatch), nameof(StringAssert.IsMatch)),
            (nameof(NUnitFrameworkConstants.NameOfStringAssertDoesNotMatch), nameof(StringAssert.DoesNotMatch)),

            (nameof(NUnitFrameworkConstants.NameOfCollectionAssert), nameof(CollectionAssert)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertAllItemsAreInstancesOfType), nameof(CollectionAssert.AllItemsAreInstancesOfType)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertAllItemsAreNotNull), nameof(CollectionAssert.AllItemsAreNotNull)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertAllItemsAreUnique), nameof(CollectionAssert.AllItemsAreUnique)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertAreEqual), nameof(CollectionAssert.AreEqual)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertAreEquivalent), nameof(CollectionAssert.AreEquivalent)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertAreNotEqual), nameof(CollectionAssert.AreNotEqual)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertAreNotEquivalent), nameof(CollectionAssert.AreNotEquivalent)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertContains), nameof(CollectionAssert.Contains)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertDoesNotContain), nameof(CollectionAssert.DoesNotContain)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertIsNotSubsetOf), nameof(CollectionAssert.IsNotSubsetOf)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertIsSubsetOf), nameof(CollectionAssert.IsSubsetOf)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertIsNotSupersetOf), nameof(CollectionAssert.IsNotSupersetOf)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertIsSupersetOf), nameof(CollectionAssert.IsSupersetOf)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertIsEmpty), nameof(CollectionAssert.IsEmpty)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertIsNotEmpty), nameof(CollectionAssert.IsNotEmpty)),
            (nameof(NUnitFrameworkConstants.NameOfCollectionAssertIsOrdered), nameof(CollectionAssert.IsOrdered)),

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

            (nameof(NUnitFrameworkConstants.NameOfCancelAfterAttribute), nameof(CancelAfterAttribute)),

            (nameof(NUnitFrameworkConstants.NameOfExpectedResult), nameof(TestAttribute.ExpectedResult)),

            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionAnd), nameof(EqualConstraint.And)),
            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionOr), nameof(EqualConstraint.Or)),
            (nameof(NUnitFrameworkConstants.NameOfConstraintExpressionWith), nameof(ConstraintExpression.With)),

            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintIgnoreCase), nameof(EqualConstraint.IgnoreCase)),
            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintUsing), nameof(EqualConstraint.Using)),
            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintWithin), nameof(EqualConstraint.Within)),
            (nameof(NUnitFrameworkConstants.NameOfEqualConstraintAsCollection), nameof(EqualConstraint.AsCollection)),

            (nameof(NUnitFrameworkConstants.NameOfClassicAssert), "ClassicAssert"),
        };

        private static readonly (string Constant, Type Type)[] FullNameOfTypeSource = new (string Constant, Type Type)[]
        {
            (nameof(NUnitFrameworkConstants.FullNameOfTypeIs), typeof(Is)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseAttribute), typeof(TestCaseAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute), typeof(TestCaseSourceAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestAttribute), typeof(TestAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeParallelizableAttribute), typeof(ParallelizableAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeValueSourceAttribute), typeof(ValueSourceAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeValuesAttribute), typeof(ValuesAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeITestBuilder), typeof(ITestBuilder)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeISimpleTestBuilder), typeof(ISimpleTestBuilder)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeIParameterDataSource), typeof(IParameterDataSource)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseData), typeof(TestCaseData)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTestCaseParameters), typeof(TestCaseParameters)),

            (nameof(NUnitFrameworkConstants.FullNameOfTypeOneTimeSetUpAttribute), typeof(OneTimeSetUpAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeOneTimeTearDownAttribute), typeof(OneTimeTearDownAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeSetUpAttribute), typeof(SetUpAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfTypeTearDownAttribute), typeof(TearDownAttribute)),

            (nameof(NUnitFrameworkConstants.FullNameOfFixtureLifeCycleAttribute), typeof(FixtureLifeCycleAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfLifeCycle), typeof(LifeCycle)),

            (nameof(NUnitFrameworkConstants.FullNameOfCancelAfterAttribute), typeof(CancelAfterAttribute)),
            (nameof(NUnitFrameworkConstants.FullNameOfCancellationToken), typeof(CancellationToken)),

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
            var parameters = typeof(ClassicAssert).GetMethods()
                .First(m => m.Name == nameof(ClassicAssert.AreEqual))
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

        [TestCase(nameof(ClassicAssert.True))]
        [TestCase(nameof(ClassicAssert.IsTrue))]
        [TestCase(nameof(ClassicAssert.False))]
        [TestCase(nameof(ClassicAssert.IsFalse))]
        [TestCase(nameof(ClassicAssert.IsTrue))]
        public void NameOfAssertConditionParameters(string method)
        {
            var parameters = typeof(ClassicAssert).GetMethods()
                .First(m => m.Name == method)
                .GetParameters();
            var parameterNames = parameters.Select(p => p.Name);

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

        [Test]
        public void NUnitLegacyAssemblyNameTest()
        {
            // We are testing that the value of the constant is correct
#pragma warning disable NUnit2007 // The actual value should not be a constant
#if NUNIT4
            Assert.That(NUnitFrameworkConstants.NUnitFrameworkLegacyAssemblyName,
#else
            Assert.That(NUnitFrameworkConstants.NUnitFrameworkAssemblyName,
#endif
                Is.EqualTo(typeof(ClassicAssert).Assembly.GetName().Name));
#pragma warning restore NUnit2007 // The actual value should not be a constant
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

        private static string? GetValue(string fieldName) =>
            typeof(NUnitFrameworkConstants).GetField(fieldName)?.GetRawConstantValue() as string;
    }
}
