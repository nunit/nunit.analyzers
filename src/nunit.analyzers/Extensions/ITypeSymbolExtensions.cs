using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Extensions
{
    internal static class ITypeSymbolExtensions
    {
        internal static bool IsAssignableFrom(this ITypeSymbol @this, ITypeSymbol other)
        {
            return @this != null &&
                other != null &&
                (@this.Equals(other) ||
                @this.IsAssignableFrom(other.BaseType) ||
                other.Interfaces.Any(_ => @this.IsAssignableFrom(_)));
        }

        internal static bool IsAssert(this ITypeSymbol @this)
        {
            return @this != null &&
                NunitFrameworkConstants.AssemblyQualifiedNameOfTypeAssert.Contains(@this.ContainingAssembly.Name) &&
                @this.Name == NunitFrameworkConstants.NameOfAssert;
        }
    }
}
