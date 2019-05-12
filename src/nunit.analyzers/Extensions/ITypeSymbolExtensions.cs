using System.Collections.Generic;
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
                other.Interfaces.Any(@this.IsAssignableFrom));
        }

        internal static bool IsAssert(this ITypeSymbol @this)
        {
            return @this != null &&
                NunitFrameworkConstants.AssemblyQualifiedNameOfTypeAssert.Contains(@this.ContainingAssembly.Name) &&
                @this.Name == NunitFrameworkConstants.NameOfAssert;
        }

        internal static string GetFullMetadataName(this ITypeSymbol @this)
        {
            // e.g. System.Collections.Generic.IEnumerable`1

            var namespaces = new Stack<string>();

            var @namespace = @this.ContainingNamespace;

            while (!@namespace.IsGlobalNamespace)
            {
                namespaces.Push(@namespace.Name);
                @namespace = @namespace.ContainingNamespace;
            }

            return $"{string.Join(".", namespaces)}.{@this.MetadataName}";
        }

        internal static bool IsTypeParameterAndDeclaredOnMethod(this ITypeSymbol typeSymbol)
            => typeSymbol.TypeKind == TypeKind.TypeParameter &&
               (typeSymbol as ITypeParameterSymbol)?.DeclaringMethod != null;
    }
}
