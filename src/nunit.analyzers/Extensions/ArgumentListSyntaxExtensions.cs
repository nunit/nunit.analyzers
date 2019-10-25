using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnit.Analyzers.Extensions
{
    internal static class ArgumentListSyntaxExtensions
    {
        public static ArgumentListSyntax WithArguments(
            this ArgumentListSyntax @this,
            IEnumerable<ArgumentSyntax> newArguments)
        {
            var originalArguments = @this.Arguments;
            var originalSeparators = originalArguments.GetSeparators();

            var nodesAndTokens = new List<SyntaxNodeOrToken> { newArguments.First() };

            foreach (var newArgument in newArguments.Skip(1))
            {
                // If argument is not replaced - take original separator. Otherwise - comma
                var oldIndex = originalArguments.IndexOf(newArgument);
                var separator = originalSeparators.ElementAtOrDefault(oldIndex - 1);

                if (separator == default(SyntaxToken))
                {
                    separator = SyntaxFactory.Token(SyntaxKind.CommaToken);
                }

                nodesAndTokens.Add(separator);
                nodesAndTokens.Add(newArgument);
            }

            var newSeparatedList = SyntaxFactory.SeparatedList<ArgumentSyntax>(nodesAndTokens);

            return @this.WithArguments(newSeparatedList);
        }
    }
}
