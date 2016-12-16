using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Globalization;
using System.Linq;

namespace NUnit.Analyzers.Extensions
{
	internal static class AttributeArgumentSyntaxExtensions
	{
		internal static bool CanAssignTo(this AttributeArgumentSyntax @this, ITypeSymbol target, SemanticModel model)
		{
			// See https://github.com/nunit/nunit/blob/master/src/NUnitFramework/framework/Attributes/TestCaseAttribute.cs#L363
			// for the reasoning behind this implementation.
			var argumentValue = (@this.Expression as LiteralExpressionSyntax).Token.Value;

			if (argumentValue == null)
			{
				return target.IsReferenceType ||
					target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
			}
			else
			{
				var argumentType = model.Compilation.GetTypeByMetadataName(argumentValue.GetType().FullName);
				var targetType =
					(target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) ?
					(target as INamedTypeSymbol).TypeArguments.ToArray()[0] : target;

				if (targetType.IsAssignableFrom(argumentType))
				{
					return true;
				}
				else
				{
					var canConvert = false;

					if (targetType.SpecialType == SpecialType.System_Int16 || targetType.SpecialType == SpecialType.System_Byte ||
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
						return AttributeArgumentSyntaxExtensions.TryChangeType(targetType, argumentValue);
					}
					else if(argumentType.SpecialType == SpecialType.System_String &&
						model.Compilation.GetTypeByMetadataName(typeof(TimeSpan).FullName).IsAssignableFrom(target))
					{
						var outValue = default(TimeSpan);
						canConvert = TimeSpan.TryParse(argumentValue as string, out outValue);
					}

					return canConvert;
				}
			}
		}

		private static bool TryChangeType(ITypeSymbol targetType, object argumentValue)
		{
			var targetReflectionType = Type.GetType(targetType.MetadataName, false);

			if(targetReflectionType != null)
			{
				try
				{
					Convert.ChangeType(argumentValue, targetReflectionType, CultureInfo.InvariantCulture);
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
			else
			{
				return false;
			}
		}
	}
}
