using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Extensions
{
    internal static class AttributeDataExtensions
    {
        internal static readonly char[] AdditionalMethodsSeparators = { ',', ';', ' ' };

        public static bool DerivesFromISimpleTestBuilder(this AttributeData @this, Compilation compilation)
        {
            return DerivesFromInterface(compilation, @this, NUnitFrameworkConstants.FullNameOfTypeISimpleTestBuilder);
        }

        public static bool DerivesFromITestBuilder(this AttributeData @this, Compilation compilation)
        {
            return DerivesFromInterface(compilation, @this, NUnitFrameworkConstants.FullNameOfTypeITestBuilder);
        }

        public static bool DerivesFromIFixtureBuilder(this AttributeData @this, Compilation compilation)
        {
            return DerivesFromInterface(compilation, @this, NUnitFrameworkConstants.FullNameOfTypeIFixtureBuilder);
        }

        public static bool DerivesFromIParameterDataSource(this AttributeData @this, Compilation compilation)
        {
            return DerivesFromInterface(compilation, @this, NUnitFrameworkConstants.FullNameOfTypeIParameterDataSource);
        }

        public static bool IsTestFixtureAttribute(this AttributeData @this, Compilation compilation)
        {
            return @this.DerivesFromIFixtureBuilder(compilation);
        }

        public static bool IsTestMethodAttribute(this AttributeData @this, Compilation compilation)
        {
            return @this.DerivesFromITestBuilder(compilation) ||
                   @this.DerivesFromISimpleTestBuilder(compilation);
        }

        public static bool IsSetUpOrTearDownMethodAttribute(this AttributeData @this, Compilation compilation)
        {
            var attributeType = @this.AttributeClass;

            if (attributeType is null)
                return false;

            return attributeType.IsType(NUnitFrameworkConstants.FullNameOfTypeOneTimeSetUpAttribute, compilation)
                || attributeType.IsType(NUnitFrameworkConstants.FullNameOfTypeOneTimeTearDownAttribute, compilation)
                || attributeType.IsType(NUnitFrameworkConstants.FullNameOfTypeSetUpAttribute, compilation)
                || attributeType.IsType(NUnitFrameworkConstants.FullNameOfTypeTearDownAttribute, compilation);
        }

        public static void GetAdditionalSetUpTearDownMethods(this AnalyzerConfigOptions options,
            out ImmutableHashSet<string> additionalOneTimeSetUpMethods,
            out ImmutableHashSet<string> additionalOneTimeTearDownMethods,
            out ImmutableHashSet<string> additionalSetUpMethods,
            out ImmutableHashSet<string> additionalTearDownMethods)
        {
            // Sometimes virtual method are called from base class SetUp/TearDown methods.
            // Allow the users to specify these as well.
            additionalOneTimeSetUpMethods = ImmutableHashSet<string>.Empty;
            additionalOneTimeTearDownMethods = ImmutableHashSet<string>.Empty;
            additionalSetUpMethods = ImmutableHashSet<string>.Empty;
            additionalTearDownMethods = ImmutableHashSet<string>.Empty;
            if (options.TryGetValue("dotnet_diagnostic.NUnit.additional_one_time_setup_methods", out string? value))
            {
                additionalOneTimeSetUpMethods = additionalOneTimeSetUpMethods.Union(value.Split(AdditionalMethodsSeparators,
                    StringSplitOptions.RemoveEmptyEntries));
            }

            if (options.TryGetValue("dotnet_diagnostic.NUnit.additional_one_time_teardown_methods", out value))
            {
                additionalOneTimeTearDownMethods = additionalOneTimeTearDownMethods.Union(value.Split(AdditionalMethodsSeparators, StringSplitOptions.RemoveEmptyEntries));
            }

            if (options.TryGetValue("dotnet_diagnostic.NUnit.additional_setup_methods", out value))
            {
                additionalSetUpMethods = additionalSetUpMethods.Union(value.Split(AdditionalMethodsSeparators,
                    StringSplitOptions.RemoveEmptyEntries));
            }

            if (options.TryGetValue("dotnet_diagnostic.NUnit.additional_teardown_methods", out value))
            {
                additionalTearDownMethods = additionalTearDownMethods.Union(value.Split(AdditionalMethodsSeparators, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public static bool IsFixtureLifeCycleAttribute(this AttributeData @this, Compilation compilation)
        {
            var attributeType = @this.AttributeClass;

            if (attributeType is null)
                return false;

            return attributeType.IsType(NUnitFrameworkConstants.FullNameOfFixtureLifeCycleAttribute, compilation);
        }

        public static AttributeArgumentSyntax? GetConstructorArgumentSyntax(this AttributeData @this, int position,
            CancellationToken cancellationToken = default)
        {
            if (@this.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is not AttributeSyntax attributeSyntax)
                return null;

            return attributeSyntax.ArgumentList?.Arguments
                .Where(a => a.NameEquals is null)
                .ElementAtOrDefault(position);
        }

        public static ExpressionSyntax? GetAdjustedArgumentSyntax(this AttributeData @this, int position,
            ImmutableArray<TypedConstant> attributePositionalArguments,
            CancellationToken cancellationToken = default)
        {
            AttributeArgumentSyntax? attributeArgumentSyntax;

            if (@this.ConstructorArguments == attributePositionalArguments)
            {
                attributeArgumentSyntax = @this.GetConstructorArgumentSyntax(position, cancellationToken);
            }
            else
            {
                // Arguments have been adjusted to expand explicitly passed in array.
                // Get the first argument.
                attributeArgumentSyntax = @this.GetConstructorArgumentSyntax(0, cancellationToken);

                if (attributeArgumentSyntax?.Expression is ArrayCreationExpressionSyntax arrayCreationExpressionSyntax &&
                    arrayCreationExpressionSyntax.Initializer is InitializerExpressionSyntax initializerExpressionSyntax)
                {
                    // Get the position element of the array initializer
                    return initializerExpressionSyntax.Expressions.ElementAtOrDefault(position);
                }
                else
                {
                    attributeArgumentSyntax = @this.GetConstructorArgumentSyntax(position, cancellationToken);
                }
            }

            return attributeArgumentSyntax?.Expression;
        }

        public static AttributeArgumentSyntax? GetNamedArgumentSyntax(this AttributeData @this, string name,
            CancellationToken cancellationToken = default)
        {
            if (@this.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is not AttributeSyntax attributeSyntax)
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
