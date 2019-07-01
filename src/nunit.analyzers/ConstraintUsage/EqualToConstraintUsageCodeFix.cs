using System.Collections.Immutable;
using System.Composition;
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
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class EqualToConstraintUsageCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.IsEqualToConstraintUsage,
            AnalyzerIdentifiers.IsNotEqualToConstraintUsage);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var negated = context.Diagnostics.Any(d => d.Id == AnalyzerIdentifiers.IsNotEqualToConstraintUsage);
            var description = negated ? CodeFixConstants.UseIsNotEqualToDescription : CodeFixConstants.UseIsEqualToDescription;

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            context.CancellationToken.ThrowIfCancellationRequested();
            var conditionNode = (root.FindNode(context.Span) as ArgumentSyntax)?.Expression;

            if (conditionNode == null)
                return;

            var (actual, expected) = GetActualExpected(conditionNode);

            var assertNode = conditionNode.Ancestors()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            var assertMethod = semanticModel.GetSymbolInfo(assertNode).Symbol as IMethodSymbol;

            if (actual == null || expected == null || assertNode == null || assertMethod == null)
                return;

            var newAssertNode = UpdateAssertNode(assertNode, assertMethod, actual, expected, negated);
            var newRoot = root.ReplaceNode(assertNode, newAssertNode);

            var codeAction = CodeAction.Create(
                description,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                description);

            context.RegisterCodeFix(codeAction, context.Diagnostics);

        }

        private static (ExpressionSyntax actual, ExpressionSyntax expected) GetActualExpected(SyntaxNode conditionNode)
        {
            if (conditionNode is BinaryExpressionSyntax binaryExpression &&
                (binaryExpression.IsKind(SyntaxKind.EqualsExpression) || binaryExpression.IsKind(SyntaxKind.NotEqualsExpression)))
            {
                return (binaryExpression.Left, binaryExpression.Right);
            }
            else
            {
                if (conditionNode is PrefixUnaryExpressionSyntax prefixUnary
                    && prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
                {
                    conditionNode = prefixUnary.Operand;
                }

                if (conditionNode is InvocationExpressionSyntax invocation)
                {
                    var arguments = invocation.ArgumentList.Arguments;

                    // actual.Equals(expected)
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                        && arguments.Count == 1)
                    {
                        return (memberAccess.Expression, arguments[0].Expression);
                    }
                    // Equals(actual, expected)
                    else if (invocation.Expression.IsKind(SyntaxKind.IdentifierName)
                        && arguments.Count == 2)
                    {
                        return (arguments[0].Expression, arguments[1].Expression);
                    }
                }
            }

            return (null, null);
        }

        private static InvocationExpressionSyntax UpdateAssertNode(InvocationExpressionSyntax assertNode, IMethodSymbol assertMethod,
            ExpressionSyntax actual, ExpressionSyntax expected,
            bool negated)
        {
            // Replace Assert method to Assert.That
            var newExpression = ((MemberAccessExpressionSyntax)assertNode.Expression)
                .WithName(SyntaxFactory.IdentifierName(NameOfAssertThat));

            // Replace arguments
            var hasConstraint = assertNode.GetArgumentExpression(assertMethod, NameOfExpressionParameter) != null;

            var remainingArguments = hasConstraint
                ? assertNode.ArgumentList.Arguments.Skip(2)
                : assertNode.ArgumentList.Arguments.Skip(1);

            var constraintString = negated
                ? $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEqualTo}"
                : $"{NameOfIs}.{NameOfIsEqualTo}";

            var constraintExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.ParseExpression(constraintString),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expected))));

            var newArguments = new[] {
                SyntaxFactory.Argument(actual),
                SyntaxFactory.Argument(constraintExpression)
            }.Union(remainingArguments);

            var newArgumentsList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(newArguments));

            return assertNode
                .WithExpression(newExpression)
                .WithArgumentList(newArgumentsList);
        }
    }
}
