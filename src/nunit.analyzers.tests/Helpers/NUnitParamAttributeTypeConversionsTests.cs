using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Helpers;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Helpers
{
    [TestFixture]
    internal sealed class NUnitParamAttributeTypeConversionsTests
    {
        [TestCase(SpecialType.System_Int16)]
        [TestCase(SpecialType.System_Byte)]
        [TestCase(SpecialType.System_SByte)]
        [TestCase(SpecialType.System_Int64)]
        [TestCase(SpecialType.System_Double)]
        [TestCase(SpecialType.System_Decimal)]
        public void CanConvertFromInt32ToOtherTypes(SpecialType targetType)
        {
            var valueType = SpecialType.System_Int32;
            Assert.That(NUnitParamAttributeTypeConversions.CanConvert(valueType, targetType), Is.True);
        }

        [TestCase(SpecialType.System_Int32)]
        [TestCase(SpecialType.System_Double)]
        [TestCase(SpecialType.System_String)]
        public void CanConvertFromOtherTypesToDecimal(SpecialType valueType)
        {
            var targetType = SpecialType.System_Decimal;
            Assert.That(NUnitParamAttributeTypeConversions.CanConvert(valueType, targetType), Is.True);
        }

        [TestCase(SpecialType.System_String)]
        public void CanConvertFromOtherTypesToDateTime(SpecialType valueType)
        {
            var targetType = SpecialType.System_DateTime;
            Assert.That(NUnitParamAttributeTypeConversions.CanConvert(valueType, targetType), Is.True);
        }

        [TestCase(SpecialType.System_SByte)]
        [TestCase(SpecialType.System_Byte)]
        [TestCase(SpecialType.System_Int16)]
        [TestCase(SpecialType.System_UInt16)]
        [TestCase(SpecialType.System_Int32)]
        [TestCase(SpecialType.System_UInt32)]
        [TestCase(SpecialType.System_Int64)]
        [TestCase(SpecialType.System_UInt64)]
        [TestCase(SpecialType.System_Single)]
        [TestCase(SpecialType.System_Double)]
        [TestCase(SpecialType.System_Decimal)]
        [TestCase(SpecialType.System_DateTime)]
        public void CanConvertFromSameTypeToItself(SpecialType valueType)
        {
            var targetType = valueType;
            Assert.That(NUnitParamAttributeTypeConversions.CanConvert(valueType, targetType), Is.True);
        }

        [TestCase(SpecialType.System_Int32, SpecialType.System_UInt16)]
        [TestCase(SpecialType.System_Int32, SpecialType.System_UInt32)]
        [TestCase(SpecialType.System_Int32, SpecialType.System_UInt64)]
        [TestCase(SpecialType.System_UInt32, SpecialType.System_UInt16)]
        [TestCase(SpecialType.System_UInt32, SpecialType.System_UInt64)]
        [TestCase(SpecialType.System_Single, SpecialType.System_Double)]
        [TestCase(SpecialType.System_Double, SpecialType.System_Single)]
        public void CannotConvertFromNonmatchingTypes(SpecialType valueType, SpecialType targetType)
        {
            Assert.That(NUnitParamAttributeTypeConversions.CanConvert(valueType, targetType), Is.False);
        }
    }
}
