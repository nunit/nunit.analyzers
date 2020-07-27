using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NUnit.Analyzers.Extensions
{
    internal static class INamespaceOrTypeSymbolExtensions
    {
        /// <summary>
        /// Checks the full name of a type or namespace without incurring the expense of building a full name string.
        /// </summary>
        public static bool HasFullName(this INamespaceOrTypeSymbol symbol, params string[] segments)
        {
            if (segments.Length < 1)
                throw new ArgumentException("At least one name segment must be specified.", nameof(segments));

            if (segments.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Name segments must not be null or empty.", nameof(segments));

            if (symbol.ContainingType is object) return false; // Nested types can't be specified with this method

            if (symbol.Name != segments.Last()) return false;

            var currentNamespace = symbol.ContainingNamespace;

            for (var i = segments.Length - 2; i >= 0; i--)
            {
                if (currentNamespace.IsGlobalNamespace) return false;
                if (currentNamespace.Name != segments[i]) return false;
                currentNamespace = currentNamespace.ContainingNamespace;
            }

            return currentNamespace.IsGlobalNamespace;
        }
    }
}
