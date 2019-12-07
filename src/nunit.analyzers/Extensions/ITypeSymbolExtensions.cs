using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        internal static bool IsAwaitable(this ITypeSymbol @this, out ITypeSymbol returnType)
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
    }
}
