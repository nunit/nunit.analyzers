using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Extensions
{
    internal static class AttributeDataExtensions
    {
        public static bool DerivesFromISimpleTestBuilder(this AttributeData @this, Compilation compilation)
        {
            return DerivesFromInterface(compilation, @this, NunitFrameworkConstants.FullNameOfTypeISimpleTestBuilder);
        }

        public static bool DerivesFromITestBuilder(this AttributeData @this, Compilation compilation)
        {
            return DerivesFromInterface(compilation, @this, NunitFrameworkConstants.FullNameOfTypeITestBuilder);
        }
        public static bool DerivesFromIParameterDataSource(this AttributeData @this, Compilation compilation)
        {
            return DerivesFromInterface(compilation, @this, NunitFrameworkConstants.FullNameOfTypeIParameterDataSource);
        }

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

        private static bool DerivesFromInterface(Compilation compilation, AttributeData attributeData, string interfaceTypeFullName)
        {
            var interfaceType = compilation.GetTypeByMetadataName(interfaceTypeFullName);

            return attributeData.AttributeClass != null
                && attributeData.AttributeClass.AllInterfaces.Any(i => i.Equals(interfaceType));
        }
    }
}
