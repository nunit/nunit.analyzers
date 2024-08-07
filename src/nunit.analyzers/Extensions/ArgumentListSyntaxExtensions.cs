using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace NUnit.Analyzers.Extensions
{
    internal static class ArgumentListSyntaxExtensions
    {
        public static ArgumentListSyntax WithArguments(
            this ArgumentListSyntax @this,
            IEnumerable<ArgumentSyntax> newArguments)
        {
            var originalArguments = @this.Arguments;
            var originalSeparators = originalArguments.GetSeparators().ToArray();

            // To match the old style as closely as possible, do not attempt anything if the number of arguments stayed the same
            if (originalArguments.Count == newArguments.Count())
            {
                return @this.WithArguments(SyntaxFactory.SeparatedList(newArguments, originalSeparators))
                    .WithAdditionalAnnotations(Formatter.Annotation);
            }

            // Otherwise, the number of arguments has either increased or decreased, in which case
            // there is no one-size-fits-all answer on what to do about the trivias around separators.
            // Therefore, add a newline after the separator if either the opening parenthesis
            // or any of the original separators had a trailing newline.
            var shouldAddTrailingNewlineAfterComma = TryGetFirstEndOfLineTrivia(@this.OpenParenToken, originalSeparators, out var trailingTrivia);

            var nodesAndTokens = new List<SyntaxNodeOrToken> { newArguments.First() };

            foreach (var newArgument in newArguments.Skip(1))
            {
                // If argument is not replaced - take original separator. Otherwise - comma
                var oldIndex = originalArguments.IndexOf(newArgument);
                var separator = originalSeparators.ElementAtOrDefault(oldIndex - 1);

                if (separator == default(SyntaxToken))
                {
                    separator = SyntaxFactory.Token(
                       SyntaxFactory.TriviaList(),
                       SyntaxKind.CommaToken,
                       shouldAddTrailingNewlineAfterComma ? SyntaxFactory.TriviaList(trailingTrivia) : SyntaxFactory.TriviaList());
                }

                nodesAndTokens.Add(separator);
                nodesAndTokens.Add(newArgument);
            }

            var newSeparatedList = SyntaxFactory.SeparatedList<ArgumentSyntax>(nodesAndTokens);

            return @this.WithArguments(newSeparatedList)
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static bool TryGetFirstEndOfLineTrivia(SyntaxToken openParenToken, SyntaxToken[] separators, out SyntaxTrivia trailingTrivia)
        {
            // If there's only one argument, there are no separators.
            // Therefore, use the trailing trivia of the opening parenthesis into account to make our best guess.
            if (separators.Length == 0)
            {
                foreach (var trivia in openParenToken.TrailingTrivia)
                {
                    if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    {
                        trailingTrivia = trivia;
                        return true;
                    }
                }
            }
            else
            {
                foreach (var separator in separators)
                {
                    foreach (var trivia in separator.TrailingTrivia)
                    {
                        if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                        {
                            trailingTrivia = trivia;
                            return true;
                        }
                    }
                }
            }

            trailingTrivia = default;
            return false;
        }
    }
}
