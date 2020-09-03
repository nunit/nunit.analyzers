using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    public abstract class BaseConditionConstraintCodeFix : CodeFixProvider
    {
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var suggestedConstraintString = context.Diagnostics[0].Properties[BaseConditionConstraintAnalyzer.SuggestedConstraintString];

            var description = string.Format(CultureInfo.InvariantCulture,
                CodeFixConstants.UseConstraintDescriptionFormat, suggestedConstraintString);

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();
            var conditionNode = (root.FindNode(context.Span) as ArgumentSyntax)?.Expression;

            if (conditionNode == null)
                return;

            // If condition is logical not expression - use operand
            if (conditionNode is PrefixUnaryExpressionSyntax unarySyntax && unarySyntax.IsKind(SyntaxKind.LogicalNotExpression))
            {
                conditionNode = unarySyntax.Operand;
            }

            var (actual, constraintExpression) = this.GetActualAndConstraintExpression(conditionNode, suggestedConstraintString);

            var assertNode = conditionNode.Ancestors()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            var assertMethod = semanticModel.GetSymbolInfo(assertNode).Symbol as IMethodSymbol;

            if (actual == null || constraintExpression == null || assertNode == null || assertMethod == null)
                return;

            var newAssertNode = UpdateAssertNode(assertNode, assertMethod, actual, constraintExpression);
            var newRoot = root.ReplaceNode(assertNode, newAssertNode);

            var codeAction = CodeAction.Create(
                description,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                description);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        protected abstract (ExpressionSyntax? actual, ExpressionSyntax? constraintExpression) GetActualAndConstraintExpression(
            ExpressionSyntax conditionNode, string suggestedConstraintString);

        protected static InvocationExpressionSyntax? GetConstraintExpression(string constraintString, ExpressionSyntax? expected)
        {
            if (expected == null)
                return null;

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.ParseExpression(constraintString),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expected))));
        }

        protected static InvocationExpressionSyntax UpdateAssertNode(InvocationExpressionSyntax assertNode, IMethodSymbol assertMethod,
            ExpressionSyntax actual, ExpressionSyntax constraintExpression)
        {
            // Replace Assert method to Assert.That
            var newExpression = ((MemberAccessExpressionSyntax)assertNode.Expression)
                .WithName(SyntaxFactory.IdentifierName(NameOfAssertThat));

            // Replace arguments
            var hasConstraint = assertNode.GetArgumentExpression(assertMethod, NameOfExpressionParameter) != null;

            var remainingArguments = hasConstraint
                ? assertNode.ArgumentList.Arguments.Skip(2)
                : assertNode.ArgumentList.Arguments.Skip(1);

            var newArguments = new[]
            {
                SyntaxFactory.Argument(actual),
                SyntaxFactory.Argument(constraintExpression)
            }.Union(remainingArguments);

            var newArgumentsList = assertNode.ArgumentList.WithArguments(newArguments);

            return assertNode
                .WithExpression(newExpression)
                .WithArgumentList(newArgumentsList);
        }
    }
}
