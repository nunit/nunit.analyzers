using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.Linq;

namespace NUnit.Analyzers.Extensions
{
	internal static class ITypeSymbolExtensions
	{
		internal static bool IsAssignableFrom(this ITypeSymbol @this, ITypeSymbol other)
		{
			return @this != null &&
				other != null &&
				((@this.MetadataName == other.MetadataName && 
					@this.ContainingAssembly.MetadataName == other.ContainingAssembly.MetadataName) ||
				@this.IsAssignableFrom(other.BaseType) ||
				other.Interfaces.Any(_ => @this.IsAssignableFrom(_)));
		}

		internal static bool IsAssert(this ITypeSymbol @this)
		{
			return @this != null &&
				typeof(Assert).AssemblyQualifiedName.Contains(@this.ContainingAssembly.Name) &&
				@this.Name == nameof(Assert);
		}
	}
}
