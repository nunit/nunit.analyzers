using Microsoft.CodeAnalysis;

namespace NUnit.Analyzers.Extensions
{
    internal static class SyntaxReferenceExtensions
    {
        public static Location GetLocation(this SyntaxReference? @this)
        {
            return @this?.SyntaxTree.GetLocation(@this.Span) ?? Location.None;
        }
    }
}
