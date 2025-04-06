using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Extensions
{
    internal static class IMethodSymbolExtensions
    {
        /// <summary>
        /// Gets the parameters into required, optional, and params counts.
        /// </summary>
        /// <param name="this">The <see cref="IMethodSymbol"/> reference to get parameters from.</param>
        /// <param name="hasCancelAfterAttribute"><see langword="true"/> if <paramref name="this"/> has a <c>CancelAfterAttribute</c>.</param>
        /// <param name="cancellationTokenType">The symbol reference for <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>
        /// The first count is the required parameters, the second is the optional count,
        /// and the last is the <see langword="params" /> count.
        /// </returns>
        /// <remarks>
        /// When the <c>CancelAfterAttribute</c> is in play and the last parameter has type <see cref="System.Threading.CancellationToken"/>
        /// this parameter is optional, if not supplied by the user it will be supplied by NUnit.
        /// </remarks>
        internal static (uint requiredParameters, uint optionalParameters, uint paramsCount) GetParameterCounts(
            this IMethodSymbol @this,
            bool hasCancelAfterAttribute,
            INamedTypeSymbol? cancellationTokenType)
        {
            var parameters = @this.Parameters;

            var requiredParameters = default(uint);
            var optionalParameters = default(uint);
            var paramsParameters = default(uint);

            foreach (var parameter in parameters)
            {
                if (parameter.IsOptional)
                {
                    optionalParameters++;
                }
                else if (parameter.IsParams)
                {
                    paramsParameters++;
                }
                else
                {
                    requiredParameters++;
                }
            }

            var hasCancellationToken = parameters.Length > 0 &&
                                       SymbolEqualityComparer.Default.Equals(parameters[parameters.Length - 1].Type, cancellationTokenType);
            if (hasCancelAfterAttribute && hasCancellationToken)
            {
                // This parameter is optional, if not specified it will be supplied by NUnit.
                optionalParameters++;
                requiredParameters--;
            }

            return (requiredParameters, optionalParameters, paramsParameters);
        }

        /// <summary>
        /// Returns true if method is implementation of method in interface.
        /// </summary>
        internal static bool IsInterfaceImplementation(this IMethodSymbol @this, string interfaceFullName)
        {
            var interfaceType = @this.ContainingType.AllInterfaces.FirstOrDefault(i => i.GetFullMetadataName() == interfaceFullName);

            if (interfaceType is null)
                return false;

            return interfaceType.GetMembers().OfType<IMethodSymbol>()
                .Any(interfaceMethod => interfaceMethod.Name == @this.Name
                    && SymbolEqualityComparer.Default.Equals(@this.ContainingType.FindImplementationForInterfaceMember(interfaceMethod), @this));
        }

        internal static bool IsTestRelatedMethod(this IMethodSymbol methodSymbol, Compilation compilation)
        {
            return methodSymbol.HasTestRelatedAttributes(compilation) ||
                (methodSymbol.OverriddenMethod is not null && methodSymbol.OverriddenMethod.IsTestRelatedMethod(compilation));
        }

        internal static bool HasTestRelatedAttributes(this IMethodSymbol methodSymbol, Compilation compilation)
        {
            return methodSymbol.GetAttributes().Any(
                a => a.IsTestMethodAttribute(compilation) || a.IsSetUpOrTearDownMethodAttribute(compilation));
        }

        internal static bool IsTestFixture(this ITypeSymbol typeSymbol, Compilation compilation)
        {
            return typeSymbol.GetAllAttributes().Any(a => a.IsTestFixtureAttribute(compilation)) ||
                   typeSymbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsTestRelatedMethod(compilation));
        }

        internal static bool IsInstancePerTestCaseFixture(this ITypeSymbol typeSymbol, Compilation compilation)
        {
            // Is there a FixtureLifeCycleAttribute?
            AttributeData? fixtureLifeCycleAttribute = typeSymbol.GetAllAttributes().FirstOrDefault(x => x.IsFixtureLifeCycleAttribute(compilation));
            return fixtureLifeCycleAttribute is not null &&
                fixtureLifeCycleAttribute.ConstructorArguments.Length == 1 &&
                fixtureLifeCycleAttribute.ConstructorArguments[0] is TypedConstant typeConstant &&
                typeConstant.Kind == TypedConstantKind.Enum &&
                typeConstant.Type.IsType(NUnitFrameworkConstants.FullNameOfLifeCycle, compilation) &&
                typeConstant.Value is 1 /* LifeCycle.InstancePerTestCase */;
        }
    }
}
