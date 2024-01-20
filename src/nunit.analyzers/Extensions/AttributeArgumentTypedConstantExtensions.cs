using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NUnit.Analyzers.Extensions
{
    internal static class AttributeArgumentTypedConstantExtensions
    {
        #region CanAssignTo

        private static readonly IReadOnlyList<Type> ConvertibleTypes = new List<Type>
        {
            typeof(short),
            typeof(byte),
            typeof(long),
            typeof(sbyte),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
        };

        // Intrinsic type converters for types
        // https://github.com/dotnet/runtime/blob/master/src/libraries/System.ComponentModel.TypeConverter/src/System/ComponentModel/ReflectTypeDescriptionProvider.cs
        // For converters that exist in .NET Standard 1.6 we use the converter.
        // Otherwise, we assume that the converter exists and it can convert the value.
        private static readonly List<(Type type, Lazy<TypeConverter>? typeConverter)> IntrinsicTypeConverters =
            new()
            {
                (typeof(bool), new Lazy<TypeConverter>(() => new BooleanConverter())),
                (typeof(byte), new Lazy<TypeConverter>(() => new ByteConverter())),
                (typeof(sbyte), new Lazy<TypeConverter>(() => new SByteConverter())),
                (typeof(char), new Lazy<TypeConverter>(() => new CharConverter())),
                (typeof(double), new Lazy<TypeConverter>(() => new DoubleConverter())),
                (typeof(string), new Lazy<TypeConverter>(() => new StringConverter())),
                (typeof(int), new Lazy<TypeConverter>(() => new Int32Converter())),
                (typeof(short), new Lazy<TypeConverter>(() => new Int16Converter())),
                (typeof(long), new Lazy<TypeConverter>(() => new Int64Converter())),
                (typeof(float), new Lazy<TypeConverter>(() => new SingleConverter())),
                (typeof(ushort), new Lazy<TypeConverter>(() => new UInt16Converter())),
                (typeof(uint), new Lazy<TypeConverter>(() => new UInt32Converter())),
                (typeof(ulong), new Lazy<TypeConverter>(() => new UInt64Converter())),
                (typeof(CultureInfo), null),
                (typeof(DateTime), new Lazy<TypeConverter>(() => new DateTimeConverter())),
                (typeof(DateTimeOffset), new Lazy<TypeConverter>(() => new DateTimeOffsetConverter())),
                (typeof(decimal), new Lazy<TypeConverter>(() => new DecimalConverter())),
                (typeof(TimeSpan), new Lazy<TypeConverter>(() => new TimeSpanConverter())),
                (typeof(Guid), new Lazy<TypeConverter>(() => new GuidConverter())),
                (typeof(Uri), new Lazy<TypeConverter>(() => new UriTypeConverter())),
                (typeof(Version), null),
            };

        internal static bool CanAssignTo(this TypedConstant @this, ITypeSymbol target, Compilation compilation,
            bool allowImplicitConversion = false,
            bool allowEnumToUnderlyingTypeConversion = false,
            bool suppressNullableWarning = false)
        {
            // See https://github.com/nunit/nunit/blob/f16d12d6fa9e5c879601ad57b4b24ec805c66054/src/NUnitFramework/framework/Attributes/TestCaseAttribute.cs#L396
            // for the reasoning behind this implementation.

            object? argumentValue = GetValue(@this);

            ITypeSymbol? argumentType = @this.Type;
            ITypeSymbol? targetType = GetTargetType(target);

            if (allowEnumToUnderlyingTypeConversion && targetType?.TypeKind == TypeKind.Enum)
                targetType = (targetType as INamedTypeSymbol)?.EnumUnderlyingType;

            if (targetType is null)
                return false;

            var typeParameter = targetType as ITypeParameterSymbol;

            if (argumentValue is null)
            {
                if (
                    (target.IsReferenceType && (target.NullableAnnotation != NullableAnnotation.NotAnnotated || suppressNullableWarning))
                    || target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    return true;
                }

                if (typeParameter is not null)
                {
                    if (typeParameter.HasValueTypeConstraint ||
                        typeParameter.HasNotNullConstraint ||
                        (typeParameter.HasReferenceTypeConstraint && typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.NotAnnotated))
                    {
                        return false;
                    }

                    // Either no constraint or class?
                    return true;
                }
            }
            else
            {
                if (allowEnumToUnderlyingTypeConversion && argumentType?.TypeKind == TypeKind.Enum)
                    argumentType = (argumentType as INamedTypeSymbol)?.EnumUnderlyingType;

                if (argumentType is null)
                    return false;

                if (typeParameter is not null)
                {
                    if ((typeParameter.HasReferenceTypeConstraint && !argumentType.IsReferenceType) ||
                        (typeParameter.HasValueTypeConstraint && argumentType.IsReferenceType))
                    {
                        return false;
                    }

                    return true;
                }

                if (targetType.IsAssignableFrom(argumentType)
                    || (allowImplicitConversion && HasBuiltInImplicitConversion(argumentType, targetType, compilation)))
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

                    if (canConvert)
                    {
                        return AttributeArgumentTypedConstantExtensions.TryChangeType(targetType, argumentValue);
                    }
                }
            }

            return CanBeTranslatedByTypeConverter(targetType, argumentValue, compilation);
        }

        private static object? GetValue(TypedConstant typedConstant)
        {
            if (typedConstant.IsNull)
                return null;

            return typedConstant.Kind == TypedConstantKind.Array
                ? typedConstant.Values
                : typedConstant.Value;
        }

        private static ITypeSymbol GetTargetType(ITypeSymbol target)
        {
            if (target is INamedTypeSymbol namedType &&
                target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                return namedType.TypeArguments[0];
            }

            return target;
        }

        private static bool TryChangeType(ITypeSymbol targetTypeSymbol, object argumentValue)
        {
            var typeName = targetTypeSymbol.GetFullMetadataName();
            var targetType = ConvertibleTypes.FirstOrDefault(t => t.FullName == typeName);

            if (targetType is null)
            {
                return false;
            }

            try
            {
                Convert.ChangeType(argumentValue, targetType, CultureInfo.InvariantCulture);
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        private static bool HasBuiltInImplicitConversion(ITypeSymbol argumentType, ITypeSymbol targetType, Compilation compilation)
        {
            var conversion = compilation.ClassifyConversion(argumentType, targetType);
            return conversion.IsImplicit && !conversion.IsUserDefined;
        }

        private static bool CanBeTranslatedByTypeConverter(
            ITypeSymbol targetTypeSymbol,
            object? argumentValue,
            Compilation compilation)
        {
            var typeName = targetTypeSymbol.GetFullMetadataName();
            var targetType = IntrinsicTypeConverters.FirstOrDefault(t => t.type.FullName == typeName);

            if (targetType != default)
            {
                if (argumentValue is not null)
                {
                    if (targetType.typeConverter is null)
                    {
                        return argumentValue is string;
                    }

                    var typeConverter = targetType.typeConverter.Value;
                    if (typeConverter.CanConvertFrom(argumentValue.GetType()))
                    {
                        try
                        {
                            typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, argumentValue);
                            return true;
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch
#pragma warning restore CA1031 // Do not catch general exception types
                        {
                        }
                    }
                }
            }
            else
            {
                if (compilation.GetTypeByMetadataName(typeof(TypeConverterAttribute).FullName!) is { } typeConverterAttribute
                    && targetTypeSymbol.GetAllAttributes().Any(data => SymbolEqualityComparer.Default.Equals(typeConverterAttribute, data.AttributeClass)))
                {
                    // If there is an explicit listed TypeConverter, we assume it can convert anything.
                    return true;
                }
            }

            return false;
        }

#endregion

#pragma warning disable SA1202 // Elements should be ordered by access. Prefer grouping
        internal static ImmutableArray<TypedConstant> AdjustArguments(this ImmutableArray<TypedConstant> attributePositionalArguments)
#pragma warning restore SA1202 // Elements should be ordered by access
        {
            if (attributePositionalArguments.Length == 1
                && attributePositionalArguments[0] is { Kind: TypedConstantKind.Array } arrayArgument
                && arrayArgument.Type is IArrayTypeSymbol arrayType
                && arrayType.ElementType.SpecialType == SpecialType.System_Object)
            {
                // params overload, need to check array values instead

                if (arrayArgument.IsNull)
                {
                    // If null is provided - analyze it as single null value, not as null array
                    // The code below creates an invalid TypedConstant (Error, NullType)
                    // but no public constructor is available to create a proper one.
                    return ImmutableArray.Create(default(TypedConstant));
                }
                else
                {
                    // This is a special case where the argument is explicitly specified as an array.
                    // TestCase(new object[] { 1.0, 2 }) is then translated into TestCase(1.0, 2)
                    return arrayArgument.Values;
                }
            }

            return attributePositionalArguments;
        }
    }
}
