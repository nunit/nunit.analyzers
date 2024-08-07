using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    public abstract class BaseConditionConstraintCodeFix : CodeFixProvider
    {
        internal const string UseConstraintDescriptionFormat = "Use {0} constraint";

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            ImmutableDictionary<string, string?> properties = context.Diagnostics[0].Properties;
            var suggestedConstraintString = properties[BaseConditionConstraintAnalyzer.SuggestedConstraintString];
            bool swapOperands = properties[BaseConditionConstraintAnalyzer.SwapOperands] == true.ToString();

            if (suggestedConstraintString is null)
            {
                return;
            }

            var description = string.Format(CultureInfo.InvariantCulture,
                UseConstraintDescriptionFormat, suggestedConstraintString);

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null || semanticModel is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();
            SyntaxNode node = root.FindNode(context.Span);
            var conditionNode = (node as ArgumentSyntax)?.Expression;

            if (conditionNode is null)
                return;

            // If condition is logical not expression - use operand
            if (conditionNode is PrefixUnaryExpressionSyntax unarySyntax && unarySyntax.IsKind(SyntaxKind.LogicalNotExpression))
            {
                conditionNode = unarySyntax.Operand;
            }

            var (actual, constraintExpression) =
                this.GetActualAndConstraintExpression(conditionNode, suggestedConstraintString, swapOperands);

            var assertNode = conditionNode.Ancestors()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (actual is null || constraintExpression is null || assertNode is null)
                return;

            var assertMethod = semanticModel.GetSymbolInfo(assertNode).Symbol as IMethodSymbol;

            if (assertMethod is null)
                return;

            var newAssertNode = UpdateAssertNode(assertNode, conditionNode, assertMethod, actual, constraintExpression);

            if (newAssertNode is null)
                return;

            var newRoot = root.ReplaceNode(assertNode, newAssertNode);

            var codeAction = CodeAction.Create(
                description,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                description);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        protected virtual (ExpressionSyntax? actual, ExpressionSyntax? constraintExpression) GetActualAndConstraintExpression(
            ExpressionSyntax conditionNode, string suggestedConstraintString, bool swapOperands)
        {
            return this.GetActualAndConstraintExpression(conditionNode, suggestedConstraintString);
        }

        protected virtual (ExpressionSyntax? actual, ExpressionSyntax? constraintExpression) GetActualAndConstraintExpression(
            ExpressionSyntax conditionNode, string suggestedConstraintString)
        {
            throw new InvalidOperationException("You must override one of the 'GetActualAndConstraintExpression' versions");
        }

        protected static InvocationExpressionSyntax? GetConstraintExpression(string constraintString, ExpressionSyntax? expected)
        {
            if (expected is null)
                return null;

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.ParseExpression(constraintString),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expected.WithoutTrivia()))));
        }

        protected static InvocationExpressionSyntax? UpdateAssertNode(InvocationExpressionSyntax assertNode, ExpressionSyntax? conditionNode, IMethodSymbol assertMethod,
            ExpressionSyntax actual, ExpressionSyntax constraintExpression)
        {
            // Replace the original ClassicAssert.<Method> invocation name into Assert.That
            var newAssertNode = assertNode.UpdateClassicAssertToAssertThat(out TypeArgumentListSyntax? typeArguments);

            if (newAssertNode is null)
                return null;

            // Replace arguments
            var hasConstraint = assertNode.GetArgumentExpression(assertMethod, NameOfExpressionParameter) is not null;

            var remainingArguments = hasConstraint
                ? assertNode.ArgumentList.Arguments.Skip(2)
                : assertNode.ArgumentList.Arguments.Skip(1);

            var actualArgument = SyntaxFactory.Argument(actual);
            var actualArgumentWithCorrectTrivia = conditionNode is not null
                ? actualArgument.WithLeadingTrivia(conditionNode.GetLeadingTrivia()) // ignore the trailing trivia, as there is a following argument
                : actualArgument;

            var lastOriginalArgument = assertNode.ArgumentList.Arguments.Last();
            var constraintArgument = SyntaxFactory.Argument(constraintExpression).WithTriviaFrom(lastOriginalArgument);
            var constraintArgumentWithCorrectTrivia = remainingArguments.Any()
                ? constraintArgument.WithoutTrailingTrivia() // remove the trailing trivia, as there is a following argument
                : constraintArgument;

            var newArguments = new[]
            {
                actualArgumentWithCorrectTrivia,
                constraintArgumentWithCorrectTrivia
            }.Union(remainingArguments);

            var newArgumentsList = assertNode.ArgumentList.WithArguments(newArguments);

            return newAssertNode.WithArgumentList(newArgumentsList);
        }
    }
}
