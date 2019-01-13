using System;
using Microsoft.CodeAnalysis;

namespace NUnit.Analyzers.Extensions
{
    internal static class IMethodSymbolExtensions
    {
        /// <summary>
        /// Gets the parameters into required, optional, and params counts.
        /// </summary>
        /// <param name="this">The <see cref="IMethodSymbol"/> reference to get parameters from.</param>
        /// <returns>
        /// The first count is the required parameters, the second is the optional count, 
        /// and the last is the <code>params</code> count.
        /// </returns>
        internal static Tuple<uint, uint, uint> GetParameterCounts(this IMethodSymbol @this)
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

            return new Tuple<uint, uint, uint>(requiredParameters, optionalParameters, paramsParameters);
        }
    }
}
