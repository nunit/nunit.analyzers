using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
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

            object argumentValue = null;
            if(@this.Expression is LiteralExpressionSyntax)
			    argumentValue = (@this.Expression as LiteralExpressionSyntax).Token.Value;

            if(@this.Expression is ImplicitArrayCreationExpressionSyntax)
                argumentValue = ((@this.Expression as ImplicitArrayCreationExpressionSyntax).Initializer.Expressions[0] as LiteralExpressionSyntax).Token.Value;

			if (argumentValue == null)
			{
				return target.IsReferenceType ||
					target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
			}
			else
			{
				var argumentType = model.Compilation.GetTypeByMetadataName(argumentValue.GetType().FullName);
                var targetType = GetTargetType(target);

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
						model.Compilation.GetTypeByMetadataName(typeof(TimeSpan).FullName).IsAssignableFrom(targetType))
					{
						var outValue = default(TimeSpan);
						canConvert = TimeSpan.TryParse(argumentValue as string, out outValue);
					}

					return canConvert;
				}
			}
		}

        private static ITypeSymbol GetTargetType(ITypeSymbol target)
        {
            if(target.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                return (target as INamedTypeSymbol).TypeArguments.ToArray()[0];

            if(target is IArrayTypeSymbol)
                return (target as IArrayTypeSymbol).ElementType;

            return target;
        }

        private static bool TryChangeType(ITypeSymbol targetType, object argumentValue)
		{
			var targetReflectionType = Type.GetType(
				AttributeArgumentSyntaxExtensions.GetFullName(targetType), false);

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

		private static string GetFullName(ITypeSymbol targetType)
		{
			// Note that this does not take into account generics,
			// so if that's ever added to attributes this will have to change.
			var namespaces = new List<string>();

			var @namespace = targetType.ContainingNamespace;

			while(!@namespace.IsGlobalNamespace)
			{
				namespaces.Add(@namespace.Name);
				@namespace = @namespace.ContainingNamespace;
			}

			return $"{string.Join(".", namespaces)}.{targetType.Name}, {targetType.ContainingAssembly.Identity.ToString()}";
		}
	}
}
