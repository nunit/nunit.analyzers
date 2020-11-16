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

        public static bool IsTestMethodAttribute(this AttributeData @this, Compilation compilation)
        {
            return @this.DerivesFromITestBuilder(compilation) ||
                   @this.DerivesFromISimpleTestBuilder(compilation);
        }

        public static bool IsSetUpOrTearDownMethodAttribute(this AttributeData @this, Compilation compilation)
        {
            var attributeType = @this.AttributeClass;

            if (attributeType == null)
                return false;

            return attributeType.IsType(NunitFrameworkConstants.FullNameOfTypeOneTimeSetUpAttribute, compilation)
                || attributeType.IsType(NunitFrameworkConstants.FullNameOfTypeOneTimeTearDownAttribute, compilation)
                || attributeType.IsType(NunitFrameworkConstants.FullNameOfTypeSetUpAttribute, compilation)
                || attributeType.IsType(NunitFrameworkConstants.FullNameOfTypeTearDownAttribute, compilation);
        }

        public static AttributeArgumentSyntax? GetConstructorArgumentSyntax(this AttributeData @this, int position,
            CancellationToken cancellationToken = default)
        {
            if (!(@this.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax attributeSyntax))
                return null;

            return attributeSyntax.ArgumentList?.Arguments
                .Where(a => a.NameEquals == null)
                .ElementAtOrDefault(position);
        }

        public static AttributeArgumentSyntax? GetNamedArgumentSyntax(this AttributeData @this, string name,
            CancellationToken cancellationToken = default)
        {
            if (!(@this.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax attributeSyntax))
                return null;

            return attributeSyntax.ArgumentList?.Arguments
                .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == name);
        }

        private static bool DerivesFromInterface(Compilation compilation, AttributeData attributeData, string interfaceTypeFullName)
        {
            if (attributeData.AttributeClass is null)
                return false;

            var interfaceType = compilation.GetTypeByMetadataName(interfaceTypeFullName);

            if (interfaceType is null)
                return false;

            return attributeData.AttributeClass.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceType));
        }
    }
}
