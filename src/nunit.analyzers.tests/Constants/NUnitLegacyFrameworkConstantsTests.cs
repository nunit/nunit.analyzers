using System.Collections.Generic;
using System.Linq;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
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
    internal sealed class NUnitLegacyFrameworkConstantsTests : BaseNUnitFrameworkConstantsTests<NUnitLegacyFrameworkConstants>
    {
        public static readonly (string Constant, string TypeName)[] NameOfSource =
        [
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsTrue), nameof(ClassicAssert.IsTrue)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertTrue), nameof(ClassicAssert.True)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsFalse), nameof(ClassicAssert.IsFalse)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertFalse), nameof(ClassicAssert.False)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertAreEqual), nameof(ClassicAssert.AreEqual)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertAreNotEqual), nameof(ClassicAssert.AreNotEqual)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertAreSame), nameof(ClassicAssert.AreSame)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertAreNotSame), nameof(ClassicAssert.AreNotSame)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertNull), nameof(ClassicAssert.Null)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsNull), nameof(ClassicAssert.IsNull)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsNotNull), nameof(ClassicAssert.IsNotNull)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertNotNull), nameof(ClassicAssert.NotNull)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertGreater), nameof(ClassicAssert.Greater)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertGreaterOrEqual), nameof(ClassicAssert.GreaterOrEqual)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertLess), nameof(ClassicAssert.Less)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertLessOrEqual), nameof(ClassicAssert.LessOrEqual)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertZero), nameof(ClassicAssert.Zero)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertNotZero), nameof(ClassicAssert.NotZero)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsNaN), nameof(ClassicAssert.IsNaN)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsEmpty), nameof(ClassicAssert.IsEmpty)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsNotEmpty), nameof(ClassicAssert.IsNotEmpty)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertContains), nameof(ClassicAssert.Contains)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsInstanceOf), nameof(ClassicAssert.IsInstanceOf)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsNotInstanceOf), nameof(ClassicAssert.IsNotInstanceOf)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertPositive), nameof(ClassicAssert.Positive)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertNegative), nameof(ClassicAssert.Negative)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsAssignableFrom), nameof(ClassicAssert.IsAssignableFrom)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfAssertIsNotAssignableFrom), nameof(ClassicAssert.IsNotAssignableFrom)),

            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssert), nameof(StringAssert)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertContains), nameof(StringAssert.Contains)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertDoesNotContain), nameof(StringAssert.DoesNotContain)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertStartsWith), nameof(StringAssert.StartsWith)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertDoesNotStartWith), nameof(StringAssert.DoesNotStartWith)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertEndsWith), nameof(StringAssert.EndsWith)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertDoesNotEndWith), nameof(StringAssert.DoesNotEndWith)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertAreEqualIgnoringCase), nameof(StringAssert.AreEqualIgnoringCase)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertAreNotEqualIgnoringCase), nameof(StringAssert.AreNotEqualIgnoringCase)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertIsMatch), nameof(StringAssert.IsMatch)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfStringAssertDoesNotMatch), nameof(StringAssert.DoesNotMatch)),

            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssert), nameof(CollectionAssert)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAllItemsAreInstancesOfType), nameof(CollectionAssert.AllItemsAreInstancesOfType)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAllItemsAreNotNull), nameof(CollectionAssert.AllItemsAreNotNull)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAllItemsAreUnique), nameof(CollectionAssert.AllItemsAreUnique)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreEqual), nameof(CollectionAssert.AreEqual)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreEquivalent), nameof(CollectionAssert.AreEquivalent)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreNotEqual), nameof(CollectionAssert.AreNotEqual)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertAreNotEquivalent), nameof(CollectionAssert.AreNotEquivalent)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertContains), nameof(CollectionAssert.Contains)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertDoesNotContain), nameof(CollectionAssert.DoesNotContain)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsNotSubsetOf), nameof(CollectionAssert.IsNotSubsetOf)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsSubsetOf), nameof(CollectionAssert.IsSubsetOf)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsNotSupersetOf), nameof(CollectionAssert.IsNotSupersetOf)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsSupersetOf), nameof(CollectionAssert.IsSupersetOf)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsEmpty), nameof(CollectionAssert.IsEmpty)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsNotEmpty), nameof(CollectionAssert.IsNotEmpty)),
            (nameof(NUnitLegacyFrameworkConstants.NameOfCollectionAssertIsOrdered), nameof(CollectionAssert.IsOrdered)),

            (nameof(NUnitLegacyFrameworkConstants.NameOfClassicAssert), "ClassicAssert"),
        ];

        protected override IEnumerable<string> Names => Constant(NameOfSource);

        protected override IEnumerable<string> FullNames => [];

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
        public void NUnitLegacyAssemblyNameTest()
        {
            // We are testing that the value of the constant is correct
#pragma warning disable NUnit2007 // The actual value should not be a constant
#if NUNIT4
            Assert.That(NUnitLegacyFrameworkConstants.NUnitFrameworkLegacyAssemblyName,
#else
            Assert.That(NUnitFrameworkConstants.NUnitFrameworkAssemblyName,
#endif
                Is.EqualTo(typeof(ClassicAssert).Assembly.GetName().Name));
#pragma warning restore NUnit2007 // The actual value should not be a constant
        }
    }
}
