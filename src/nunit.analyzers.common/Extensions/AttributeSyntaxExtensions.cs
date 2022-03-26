using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Analyzers.Extensions
{
    internal static class AttributeSyntaxExtensions
    {
        /// <summary>
        /// Gets the arguments into positional and named arrays.
        /// </summary>
        /// <param name="this">The <see cref="AttributeSyntax"/> reference to get parameters from.</param>
        /// <returns>
        /// The first array are the positional arguments, and the second contains the named arguments.
        /// </returns>
        internal static (ImmutableArray<AttributeArgumentSyntax> positionalArguments, ImmutableArray<AttributeArgumentSyntax> namedArguments)
            GetArguments(this AttributeSyntax @this)
        {
            var positionalArguments = new List<AttributeArgumentSyntax>();
            var namedArguments = new List<AttributeArgumentSyntax>();

            if (@this.ArgumentList is not null)
            {
                var arguments = @this.ArgumentList.Arguments;

                foreach (var argument in arguments)
                {
                    if (argument.DescendantNodes().OfType<NameEqualsSyntax>().Any())
                    {
                        namedArguments.Add(argument);
                    }
                    else
                    {
                        positionalArguments.Add(argument);
                    }
                }
            }

            return (positionalArguments.ToImmutableArray(), namedArguments.ToImmutableArray());
        }
    }
}
