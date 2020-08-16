using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Analyzers.Extensions
{
    internal static class AttributeDataExtensions
    {
        public static AttributeArgumentSyntax? GetConstructorArgumentSyntax(this AttributeData @this, int position,
            CancellationToken cancellationToken = default)
        {
            if (!(@this.ApplicationSyntaxReference.GetSyntax(cancellationToken) is AttributeSyntax attributeSyntax))
                return null;

            return attributeSyntax.ArgumentList.Arguments
                .Where(a => a.NameEquals == null)
                .ElementAtOrDefault(position);
        }

        public static AttributeArgumentSyntax? GetNamedArgumentSyntax(this AttributeData @this, string name,
            CancellationToken cancellationToken = default)
        {
            if (!(@this.ApplicationSyntaxReference.GetSyntax(cancellationToken) is AttributeSyntax attributeSyntax))
                return null;

            return attributeSyntax.ArgumentList.Arguments
                .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == name);
        }
    }
}
