using Microsoft.CodeAnalysis;

namespace NUnit.Analyzers.Helpers
{
    internal static class NUnitParamAttributeTypeConversions
    {
        /// <summary>
        /// Checks if a conversion is possible from the value type to the target type.
        /// </summary>
        /// <remarks>
        /// Based upon the conversions supported by NUnit's ParamAttributeTypeConversions.
        /// </remarks>
        public static bool CanConvert(ITypeSymbol valueType, ITypeSymbol targetType)
        {
            if (SymbolEqualityComparer.Default.Equals(targetType, valueType))
            {
                return true;
            }

            return CanConvert(valueType.SpecialType, targetType.SpecialType);
        }

        public static bool CanConvert(SpecialType valueType, SpecialType targetType)
        {
            if (valueType == targetType)
            {
                return true;
            }

            if (targetType is SpecialType.System_Int16 or
                              SpecialType.System_Byte or
                              SpecialType.System_SByte or
                              SpecialType.System_Int64 or
                              SpecialType.System_Double)
            {
                return valueType is SpecialType.System_Int32;
            }
            else if (targetType is SpecialType.System_Decimal)
            {
                return valueType is SpecialType.System_Int32 or
                                    SpecialType.System_Double or
                                    SpecialType.System_String;
            }
            else if (targetType is SpecialType.System_DateTime)
            {
                return valueType is SpecialType.System_String;
            }

            // Ignoring TypeDescriptors
            return false;
        }
    }
}
