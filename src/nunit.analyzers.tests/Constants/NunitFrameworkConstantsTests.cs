using System.Linq;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnit.Analyzers.Tests.Constants
{
    /// <summary>
    /// Tests to ensure that the string constants in the analyzer project correspond
    /// to the NUnit concepts that they represent.
    /// </summary>
    [TestFixture]
    public sealed class NunitFrameworkConstantsTests
    {
        [TestCase(NunitFrameworkConstants.NameOfEqualConstraintWithin, nameof(EqualConstraint.Within))]
        [TestCase(NunitFrameworkConstants.NameOfIs, nameof(Is))]
        [TestCase(NunitFrameworkConstants.NameOfIsFalse, nameof(Is.False))]
        [TestCase(NunitFrameworkConstants.NameOfIsTrue, nameof(Is.True))]
        [TestCase(NunitFrameworkConstants.NameOfIsEqualTo, nameof(Is.EqualTo))]
        [TestCase(NunitFrameworkConstants.NameOfIsNot, nameof(Is.Not))]
        [TestCase(NunitFrameworkConstants.NameOfIsNotEqualTo, nameof(Is.Not.EqualTo))]
        [TestCase(NunitFrameworkConstants.NameOfIsSameAs, nameof(Is.SameAs))]
        [TestCase(NunitFrameworkConstants.NameOfIsSamePath, nameof(Is.SamePath))]
        [TestCase(NunitFrameworkConstants.NameOfAssert, nameof(Assert))]
        [TestCase(NunitFrameworkConstants.NameOfAssertIsTrue, nameof(Assert.IsTrue))]
        [TestCase(NunitFrameworkConstants.NameOfAssertTrue, nameof(Assert.True))]
        [TestCase(NunitFrameworkConstants.NameOfAssertIsFalse, nameof(Assert.IsFalse))]
        [TestCase(NunitFrameworkConstants.NameOfAssertFalse, nameof(Assert.False))]
        [TestCase(NunitFrameworkConstants.NameOfAssertAreEqual, nameof(Assert.AreEqual))]
        [TestCase(NunitFrameworkConstants.NameOfAssertAreNotEqual, nameof(Assert.AreNotEqual))]
        [TestCase(NunitFrameworkConstants.NameOfAssertAreSame, nameof(Assert.AreSame))]
        [TestCase(NunitFrameworkConstants.NameOfAssertAreNotSame, nameof(Assert.AreNotSame))]
        [TestCase(NunitFrameworkConstants.NameOfAssertThat, nameof(Assert.That))]
        [TestCase(NunitFrameworkConstants.NameOfTestCaseAttribute, nameof(TestCaseAttribute))]
        [TestCase(NunitFrameworkConstants.NameOfExpectedResult, nameof(TestCaseAttribute.ExpectedResult))]
        [TestCase(NunitFrameworkConstants.NameOfExpectedResult, nameof(TestAttribute.ExpectedResult))]
        public void TestConstant(string constant, string nameOfArgument)
        {
            Assert.That(constant, Is.EqualTo(nameOfArgument));
        }

        [Test]
        public void FullNameOfTypeTestCaseAttributeTest()
        {
            Assert.That(
                NunitFrameworkConstants.FullNameOfTypeTestCaseAttribute,
                Is.EqualTo(typeof(TestCaseAttribute).FullName));
        }

        [Test]
        public void NameOfActualParameter()
        {
            var parameterNames = typeof(Assert).GetMethods()
                .First(m => m.Name == nameof(Assert.AreEqual))
                .GetParameters()
                .Select(p => p.Name);

            Assert.That(parameterNames, Does.Contain(NunitFrameworkConstants.NameOfActualParameter));
        }

        [Test]
        public void AssemblyQualifiedNameOfTypeAssertTest()
        {
            Assert.That(
                NunitFrameworkConstants.AssemblyQualifiedNameOfTypeAssert,
                Is.EqualTo(typeof(Assert).AssemblyQualifiedName));
        }
    }
}
