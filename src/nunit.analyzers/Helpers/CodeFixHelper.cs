using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Helpers
{
    internal static class CodeFixHelper
    {
        public static InvocationExpressionSyntax? UpdateClassicAssertToAssertThat(
            this InvocationExpressionSyntax invocationExpression,
            out TypeArgumentListSyntax? typeArguments)
        {
            // Replace the original method invocation name to "That".
            var invocationTargetNode = invocationExpression.Expression as MemberAccessExpressionSyntax;

            if (invocationTargetNode is null)
            {
                typeArguments = null;
                return null;
            }

            SimpleNameSyntax methodNode = invocationTargetNode.Name;
            typeArguments = methodNode is GenericNameSyntax genericNode ? genericNode.TypeArgumentList : null;

            MemberAccessExpressionSyntax newInvocationTargetNode =
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssert),
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssertThat))
                .WithTriviaFrom(invocationTargetNode);

            return invocationExpression.ReplaceNode(invocationTargetNode, newInvocationTargetNode);
        }

        /// <summary>
        /// This is assumed to be arguments for an 'Assert.That(actual, constraint, "...: {0} - {1}", param0, param1)`
        /// which needs converting into 'Assert.That(actual, constraint, $"...: {param0} - {param1}").
        /// </summary>
        /// <param name="arguments">The arguments passed to the 'Assert' method. </param>
        /// <param name="minimumNumberOfArguments">The argument needed for the actual method, any more are assumed messages.</param>
        public static void UpdateStringFormatToFormattableString(List<ArgumentSyntax> arguments, int minimumNumberOfArguments = 2)
        {
            int firstParamsArgument = minimumNumberOfArguments + 1;

            // If only 1 extra argument is passed, it must be a non-formattable message.
            if (arguments.Count <= firstParamsArgument)
                return;

            ExpressionSyntax formatSpecificationArgument = arguments[minimumNumberOfArguments].Expression;
            if (formatSpecificationArgument is not LiteralExpressionSyntax literalExpression)
            {
                // We only support converting if the format specification is a constant string.
                return;
            }

            string formatSpecification = literalExpression.Token.ValueText;

            int formatArgumentCount = arguments.Count - firstParamsArgument;
            ExpressionSyntax[] formatArguments = new ExpressionSyntax[formatArgumentCount];
            for (int i = 0; i < formatArgumentCount; i++)
            {
                formatArguments[i] = arguments[firstParamsArgument + i].Expression;
            }

            IEnumerable<InterpolatedStringContentSyntax> interpolatedStringContent =
                UpdateStringFormatToFormattableString(formatSpecification, formatArguments);

            InterpolatedStringExpressionSyntax interpolatedString = SyntaxFactory.InterpolatedStringExpression(
                SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken),
                SyntaxFactory.List(interpolatedStringContent));

            // Replace format specification argument with interpolated string.
            arguments[minimumNumberOfArguments] = SyntaxFactory.Argument(interpolatedString);

            // Delete params arguments.
            var nextArgument = minimumNumberOfArguments + 1;
            arguments.RemoveRange(nextArgument, arguments.Count - nextArgument);
        }

        internal static IEnumerable<InterpolatedStringContentSyntax> UpdateStringFormatToFormattableString(string formatSpecification, ExpressionSyntax[] formatArguments)
        {
            int startIndex = 0;
            for (; startIndex < formatSpecification.Length;)
            {
                int argumentSpecification = formatSpecification.IndexOf('{', startIndex);
                if (argumentSpecification < 0)
                {
                    // No more formattable arguments, use remaining text.
                    string text = formatSpecification.Substring(startIndex, formatSpecification.Length - startIndex);
                    yield return SyntaxFactory.InterpolatedStringText(InterpolatedStringTextToken(text));
                    break;
                }
                else if (argumentSpecification + 1 < formatSpecification.Length &&
                         formatSpecification[argumentSpecification + 1] == '{')
                {
                    // Special case, double '{' is an escaped '{' and should be treated as text.
                    argumentSpecification += 2;
                    string text = formatSpecification.Substring(startIndex, argumentSpecification - startIndex);
                    yield return SyntaxFactory.InterpolatedStringText(InterpolatedStringTextToken(text));
                    startIndex = argumentSpecification;
                }
                else
                {
                    if (argumentSpecification > startIndex)
                    {
                        // Copy text up to the '{'
                        string text = formatSpecification.Substring(startIndex, argumentSpecification - startIndex);
                        yield return SyntaxFactory.InterpolatedStringText(InterpolatedStringTextToken(text));
                    }

                    // Decode argument number
                    int index = 0;
                    while (char.IsDigit(formatSpecification[++argumentSpecification]))
                    {
                        index = (10 * index) + (formatSpecification[argumentSpecification] - '0');
                    }

                    int argumentSpecificationEnd = formatSpecification.IndexOf('}', argumentSpecification);

                    // Insert appropriate argument
                    if (index < formatArguments.Length)
                    {
                        var formatArgument = formatArguments[index];
                        if (formatArgument.DescendantNodesAndSelf(x => true, false)
                                          .OfType<ConditionalExpressionSyntax>()
                                          .Any())
                        {
                            // Colon is not allowed in an Interpolation, wrap expression in parenthesis: (expession).
                            formatArgument = SyntaxFactory.ParenthesizedExpression(formatArgument);
                        }

                        int argumentAlignmentSpecification = formatSpecification.IndexOf(',', argumentSpecification, argumentSpecificationEnd - argumentSpecification);
                        int argumentFormatSpecification = formatSpecification.IndexOf(':', argumentSpecification, argumentSpecificationEnd - argumentSpecification);

                        InterpolationAlignmentClauseSyntax? interpolationAlignment = null;
                        InterpolationFormatClauseSyntax? interpolationFormat = null;

                        if (argumentAlignmentSpecification > 0)
                        {
                            int alignmentSpecificationLength = (argumentFormatSpecification > 0 ?
                                argumentFormatSpecification : argumentSpecificationEnd) - ++argumentAlignmentSpecification;
                            string alignmentString = formatSpecification.Substring(argumentAlignmentSpecification, alignmentSpecificationLength);
                            int.TryParse(alignmentString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int alignment);

                            ExpressionSyntax alignmentExpression = alignment >= 0 ?
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(alignment)) :
                                SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression,
                                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(-alignment)));

                            interpolationAlignment = SyntaxFactory.InterpolationAlignmentClause(
                                SyntaxFactory.Token(SyntaxKind.CommaToken), alignmentExpression);
                        }

                        if (argumentFormatSpecification > 0)
                        {
                            string formatClause = formatSpecification.Substring(++argumentFormatSpecification, argumentSpecificationEnd - argumentFormatSpecification);
                            interpolationFormat = SyntaxFactory.InterpolationFormatClause(
                                SyntaxFactory.Token(SyntaxKind.ColonToken),
                                InterpolatedStringTextToken(formatClause));
                        }

                        yield return SyntaxFactory.Interpolation(formatArgument, interpolationAlignment, interpolationFormat);
                    }

                    startIndex = argumentSpecificationEnd + 1;
                }
            }
        }

        private static SyntaxToken InterpolatedStringTextToken(string text)
        {
            // FormatLiteral doesn't escape double quotes when passing in quote: false
            // It does when passing in quote: true but then it also surrounds it with double quotes.
            // So we do that replacement ourselves.
            string escapedText = SymbolDisplay.FormatLiteral(text, false)
                                              .Replace("\"", "\\\"");

            return SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken,
                                       escapedText, text, SyntaxTriviaList.Empty);
        }
    }
}
