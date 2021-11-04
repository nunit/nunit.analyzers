using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Extensions
{
    internal static class ITypeSymbolExtensions
    {
        internal static bool IsAssignableFrom(this ITypeSymbol @this, ITypeSymbol? other)
        {
            return @this != null &&
                other != null &&
                (SymbolEqualityComparer.Default.Equals(@this, other) ||
                @this.IsAssignableFrom(other.BaseType) ||
                other.Interfaces.Any(@this.IsAssignableFrom));
        }

        internal static bool IsAssert(this ITypeSymbol @this)
        {
            return @this != null &&
                @this.ContainingAssembly.Name == NUnitFrameworkConstants.NUnitFrameworkAssemblyName &&
                @this.Name == NUnitFrameworkConstants.NameOfAssert;
        }

        internal static bool IsConstraint(this ITypeSymbol @this)
        {
            return @this != null && @this.GetAllBaseTypes()
                .Any(t => t.Name == NUnitFrameworkConstants.NameOfConstraint
                    && @this.ContainingAssembly.Name == NUnitFrameworkConstants.NUnitFrameworkAssemblyName);
        }

        internal static bool IsType([NotNullWhen(true)] this ITypeSymbol? @this, string fullMetadataName, Compilation compilation)
        {
            var typeSymbol = compilation.GetTypeByMetadataName(fullMetadataName);

            return SymbolEqualityComparer.Default.Equals(typeSymbol, @this);
        }

        internal static string GetFullMetadataName(this ITypeSymbol @this)
        {
            // e.g. System.Collections.Generic.IEnumerable`1

            var names = new Stack<string>();
            var type = @this.ContainingType;

            while (type != null)
            {
                names.Push(type.Name);
                type = type.ContainingType;
            }

            var @namespace = @this.ContainingNamespace;
            if (@namespace != null)
            {
                while (!@namespace.IsGlobalNamespace)
                {
                    names.Push(@namespace.Name);
                    @namespace = @namespace.ContainingNamespace;
                }
            }

            return $"{string.Join(".", names)}.{@this.MetadataName}";
        }

        internal static IEnumerable<INamedTypeSymbol> GetAllBaseTypes(this ITypeSymbol @this)
        {
            var current = @this;

            while (current.BaseType != null)
            {
                yield return current.BaseType;
                current = current.BaseType;
            }
        }

        internal static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol @this)
        {
            var inheritedMembers = @this.TypeKind == TypeKind.Interface
                ? @this.AllInterfaces.SelectMany(i => i.GetMembers())
                : @this.GetAllBaseTypes().SelectMany(t => t.GetMembers());

            return @this.GetMembers().Concat(inheritedMembers);
        }

        internal static IEnumerable<AttributeData> GetAllAttributes(this ITypeSymbol? @this)
        {
            for (var current = @this; current is object; current = current.BaseType)
            {
                foreach (var data in current.GetAttributes())
                {
                    yield return data;
                }
            }
        }

        internal static bool IsTypeParameterAndDeclaredOnMethod(this ITypeSymbol typeSymbol)
            => typeSymbol.TypeKind == TypeKind.TypeParameter &&
               (typeSymbol as ITypeParameterSymbol)?.DeclaringMethod != null;

        internal static bool IsAwaitable(this ITypeSymbol @this,
            [NotNullWhen(true)] out ITypeSymbol? returnType)
        {
            returnType = null;

            if (@this == null)
                return false;

            // Type should have GetAwaiter method with no parameters
            var getAwaiterMethod = @this
                .GetMembers(nameof(Task.GetAwaiter))
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.Parameters.Length == 0
                    && m.TypeParameters.Length == 0
                    && m.ReturnType != null);

            if (getAwaiterMethod == null)
                return false;

            var awaiterType = getAwaiterMethod.ReturnType;

            // Awaiter type should implement INotifyCompletion interface,
            // have bool property IsCompleted and method GetResult with no parameters

            var hasINotifyCompletionInterface = awaiterType.AllInterfaces.Any(i => i.Name == nameof(INotifyCompletion));

            if (!hasINotifyCompletionInterface)
                return false;

            var hasIsCompletedProperty = awaiterType
                .GetMembers(nameof(TaskAwaiter.IsCompleted))
                .OfType<IPropertySymbol>()
                .Any(p => p.Type.SpecialType == SpecialType.System_Boolean);

            if (!hasIsCompletedProperty)
                return false;

            var getResultMethod = awaiterType
                .GetMembers(nameof(TaskAwaiter.GetResult))
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.TypeParameters.Length == 0
                    && m.Parameters.Length == 0);

            if (getResultMethod == null)
                return false;

            returnType = getResultMethod.ReturnType;
            return true;
        }

        /// <summary>
        /// Return value indicates whether type implements IEnumerable interface.
        /// </summary>
        /// <param name="elementType">Contains IEnumerable generic argument, or null, if type implements
        /// only non-generic IEnumerable interface, or no IEnumerable interface at all.</param>
        internal static bool IsIEnumerable(this ITypeSymbol @this, out ITypeSymbol? elementType)
        {
            elementType = null;

            var allInterfaces = @this.AllInterfaces;

            if (@this is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Interface)
                allInterfaces = allInterfaces.Add(namedType);

            var genericIEnumerableInterface = allInterfaces.FirstOrDefault(i =>
                i.GetFullMetadataName() == "System.Collections.Generic.IEnumerable`1");

            if (genericIEnumerableInterface != null)
            {
                elementType = genericIEnumerableInterface.TypeArguments.FirstOrDefault();
                return true;
            }

            var nonGenericIEnumerableInterface = allInterfaces.FirstOrDefault(i =>
                i.GetFullMetadataName() == "System.Collections.IEnumerable");

            if (nonGenericIEnumerableInterface != null)
            {
                return true;
            }

            return false;
        }
    }
}
