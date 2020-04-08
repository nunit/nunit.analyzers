using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Analyzers.Extensions
{
    internal static class AttributeArgumentSyntaxExtensions
    {
        internal static bool CanAssignTo(this AttributeArgumentSyntax @this, ITypeSymbol target, SemanticModel model,
            bool allowImplicitConversion = false,
            bool allowEnumToUnderlyingTypeConversion = false)
        {
            //See https://github.com/nunit/nunit/blob/f16d12d6fa9e5c879601ad57b4b24ec805c66054/src/NUnitFramework/framework/Attributes/TestCaseAttribute.cs#L396
            //for the reasoning behind this implementation.
            TypeInfo sourceTypeInfo = model.GetTypeInfo(@this.Expression);
            ITypeSymbol argumentType = sourceTypeInfo.Type;

            if (argumentType == null)
            {
                return target.IsReferenceType ||
                    target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
            }
            else
            {
                var targetType = GetTargetType(target);

                if (allowEnumToUnderlyingTypeConversion && targetType?.TypeKind == TypeKind.Enum)
                    targetType = (targetType as INamedTypeSymbol)?.EnumUnderlyingType;

                if (allowEnumToUnderlyingTypeConversion && argumentType?.TypeKind == TypeKind.Enum)
                    argumentType = (argumentType as INamedTypeSymbol)?.EnumUnderlyingType;

                if (targetType == null || argumentType == null)
                    return false;

                if (targetType.IsAssignableFrom(argumentType)
                    || (allowImplicitConversion && HasBuiltInImplicitConversion(argumentType, targetType, model)))
                {
                    return true;
                }
                else
                {
                    var canConvert = false;

                    if (targetType.SpecialType == SpecialType.System_Int16 || targetType.SpecialType == SpecialType.System_Byte ||
                        targetType.SpecialType == SpecialType.System_Int64 ||
                        targetType.SpecialType == SpecialType.System_SByte || targetType.SpecialType == SpecialType.System_Double)
                    {
                        canConvert = argumentType.SpecialType == SpecialType.System_Int32;
                    }
                    else if (targetType.SpecialType == SpecialType.System_Decimal)
                    {
                        canConvert = argumentType.SpecialType == SpecialType.System_Double ||
                            argumentType.SpecialType == SpecialType.System_String ||
                            argumentType.SpecialType == SpecialType.System_Int32;
                    }
                    else if (targetType.SpecialType == SpecialType.System_DateTime)
                    {
                        canConvert = argumentType.SpecialType == SpecialType.System_String;
                    }
                    // Intrinsic type converters
                    // https://github.com/dotnet/runtime/blob/master/src/libraries/System.ComponentModel.TypeConverter/src/System/ComponentModel/ReflectTypeDescriptionProvider.cs
                    else if (targetType.ContainingNamespace?.Name == "System" && (
                        targetType.Name == "CultureInfo" ||
                        targetType.Name == "DateTimeOffset" ||
                        targetType.Name == "TimeSpan" ||
                        targetType.Name == "Guid" ||
                        targetType.Name == "Uri"))
                    {
                        canConvert = argumentType.SpecialType == SpecialType.System_String;
                    }

                    if (canConvert)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        private static ITypeSymbol GetTargetType(ITypeSymbol target)
        {
            if (target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                return (target as INamedTypeSymbol).TypeArguments.ToArray()[0];

            return target;
        }

        private static bool HasBuiltInImplicitConversion(ITypeSymbol argumentType, ITypeSymbol targetType, SemanticModel model)
        {
            var conversion = model.Compilation.ClassifyConversion(argumentType, targetType);
            return conversion.IsImplicit && !conversion.IsUserDefined;
        }
    }
}
