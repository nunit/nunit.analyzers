using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

// We group the private methods with the public ones for better readability here.
#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1202 // Elements should be ordered by access

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
        /// Unless we cannot, in which case we create '() => string.Format(specification, args)'.
        /// </summary>
        /// <param name="messageArgument">The argument that corresponds to the composite format string.</param>
        /// <param name="args">The list of arguments that correspond to format items.</param>
        /// <param name="unconditional">If the message is not conditional on the test outcome.</param>
        /// <param name="argsIsArray">The params args is passed as an array instead of individual parameters.</param>
        public static ArgumentSyntax? GetInterpolatedMessageArgumentOrDefault(ArgumentSyntax? messageArgument, List<ArgumentSyntax> args, bool unconditional, bool argsIsArray)
        {
            if (messageArgument is null)
                return null;

            messageArgument = messageArgument.WithNameColon(null);

            var formatSpecificationArgument = messageArgument.Expression;
            if (formatSpecificationArgument.IsKind(SyntaxKind.NullLiteralExpression))
                return null;

            // No arguments, nothing to convert. Just a message.
            if (args.Count == 0)
                return messageArgument;

            IEnumerable<ExpressionSyntax>? formatArgumentExpressions;
            if (args.Count == 1 && args[0].Expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
            {
                // Even though the argument is an array, it is an inline array creation we can work with.
                formatArgumentExpressions = implicitArrayCreation.Initializer.Expressions;
                argsIsArray = false;
            }
            else
            {
                formatArgumentExpressions = args.Select(x => x.Expression);
            }

            // We cannot convert to an interpolated string if:
            // - The format specification is not a literal we can analyze.
            // - The 'args' argument is a real array instead of a list of params array.
            // If this is the case convert code to use () => string.Format(format, args)
            if (formatSpecificationArgument is not LiteralExpressionSyntax literalExpression || argsIsArray)
            {
                var stringFormat = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.PredefinedType(
                            SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                        SyntaxFactory.IdentifierName(nameof(string.Format))))
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                        args.Select(x => x.WithNameColon(null)).Prepend(messageArgument))));

                return SyntaxFactory.Argument(unconditional ? stringFormat : SyntaxFactory.ParenthesizedLambdaExpression(stringFormat));
            }

            var formatSpecification = literalExpression.Token.ValueText;

            var interpolatedStringContent = UpdateStringFormatToFormattableString(
                formatSpecification,
                formatArgumentExpressions.Select(e => e.WithoutTrivia()).ToArray());
            var interpolatedString = SyntaxFactory.InterpolatedStringExpression(
                SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken),
                SyntaxFactory.List(interpolatedStringContent));
            return SyntaxFactory.Argument(interpolatedString);
        }

        /// <summary>
        /// This is assumed to be arguments for an 'Assert.That(actual, constraint, "...: {0} - {1}", param0, param1)`
        /// which needs converting into 'Assert.That(actual, constraint, $"...: {param0} - {param1}").
        /// </summary>
        /// <param name="arguments">The arguments passed to the 'Assert' method. </param>
        /// <param name="minimumNumberOfArguments">The argument needed for the actual method, any more are assumed messages.</param>
        /// <param name="argsIsArray">The params args is passed as an array instead of individual parameters.</param>
        public static void UpdateStringFormatToFormattableString(List<ArgumentSyntax> arguments, int minimumNumberOfArguments, bool argsIsArray)
        {
            // If only 1 extra argument is passed, it must be a non-formattable message.
            if (arguments.Count <= minimumNumberOfArguments + 1)
                return;

            ArgumentSyntax? message = GetInterpolatedMessageArgumentOrDefault(
                arguments[minimumNumberOfArguments],
                arguments.Skip(minimumNumberOfArguments + 1).ToList(),
                unconditional: minimumNumberOfArguments == 0,
                argsIsArray);

            var nextArgument = minimumNumberOfArguments;
            if (message is not null)
            {
                arguments[minimumNumberOfArguments] = message;
                nextArgument++;
            }

            // Delete remaining arguments.
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
                            // Colon is not allowed in an Interpolation, wrap expression in parenthesis: (expression).
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

        internal static SyntaxNode UpdateTestMethodSignatureIfNecessary(
            SyntaxNode root,
            SyntaxNode original,
            SyntaxNode replacement,
            bool shouldBeAsync)
        {
            var annotation = new SyntaxAnnotation();
            var annotatedReplacement = replacement.WithAdditionalAnnotations(annotation);
            var rootWithAnnotatedReplacement = root.ReplaceNode(original, annotatedReplacement);
            var replacementInUpdatedRoot = rootWithAnnotatedReplacement.GetAnnotatedNodes(annotation).First();

            var methodDeclaration = replacementInUpdatedRoot.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (!shouldBeAsync || methodDeclaration is null || methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return rootWithAnnotatedReplacement;
            }

            var namespaceDeclaration = methodDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
            var compilationUnit = methodDeclaration.FirstAncestorOrSelf<CompilationUnitSyntax>();
            var systemThreadingTasksUsingExists =
                compilationUnit?.Usings.Any(IsUsingSystemThreadingTasks) is true ||
                namespaceDeclaration?.Usings.Any(IsUsingSystemThreadingTasks) is true;

            var taskTypeName = GetTaskTypeSyntax(systemThreadingTasksUsingExists);

            var newMethodDeclaration = methodDeclaration
                .WithModifiers(methodDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)))
                .WithReturnType(taskTypeName);

            return rootWithAnnotatedReplacement.ReplaceNode(methodDeclaration, newMethodDeclaration);
        }

        internal const string SystemThreadingTasksNamespace = "System.Threading.Tasks";
        internal const string TaskTypeName = "Task";

        private static bool IsUsingSystemThreadingTasks(UsingDirectiveSyntax u) =>
            u.Name.ToString() == SystemThreadingTasksNamespace;

        private static TypeSyntax GetTaskTypeSyntax(bool systemThreadingTasksUsingExists)
            => systemThreadingTasksUsingExists
                ? SyntaxFactory.ParseTypeName(TaskTypeName)
                : QualifiedNameFromParts("System", "Threading", "Tasks", TaskTypeName);

        private static NameSyntax QualifiedNameFromParts(params string[] parts)
        {
            NameSyntax name = SyntaxFactory.IdentifierName(parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                name = SyntaxFactory.QualifiedName(name, SyntaxFactory.IdentifierName(parts[i]));
            }

            return name;
        }
    }
}
