using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseAssertThatAsync;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class UseAssertThatAsyncCodeFix : CodeFixProvider
{
    private static readonly string[] firstParameterCandidates =
    [
        NUnitFrameworkConstants.NameOfActualParameter,
        NUnitFrameworkConstants.NameOfConditionParameter,
    ];

    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(AnalyzerIdentifiers.UseAssertThatAsync);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var assertThatInvocation = root.FindNode(diagnosticSpan) as InvocationExpressionSyntax;
        if (assertThatInvocation is null)
            return;

        var argumentList = assertThatInvocation.ArgumentList;
        var actualArgument = argumentList.Arguments.SingleOrDefault(
            a => firstParameterCandidates.Contains(a.NameColon?.Name.Identifier.Text))
            ?? argumentList.Arguments[0];

        if (actualArgument.Expression is not AwaitExpressionSyntax awaitExpression)
            return;

        // Remove the await keyword (and .ConfigureAwait() if it exists)
        var insideLambda = awaitExpression.Expression is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.Text == "ConfigureAwait"
            ? memberAccess.Expression.WithTriviaFrom(awaitExpression)
            : awaitExpression.Expression;

        var memberAccessExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssert),
            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfAssertThatAsync));

        // If there's only one argument, is must have been Assert.That(bool).
        // However, the overload Assert.ThatAsync(bool) doesn't exist, so add Is.True in that case.
        var nonLambdaArguments = argumentList.Arguments.Count == 1
            ? [SyntaxFactory.Argument(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsTrue)))]
            : argumentList.Arguments
                .Where(a => a != actualArgument)
                .Select(a => a.WithNameColon(null))
                .ToArray();
        var newArgumentList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList(
            [
                SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(insideLambda)),
                 .. nonLambdaArguments,
            ]));

        var assertThatAsyncInvocation = SyntaxFactory.AwaitExpression(
            SyntaxFactory.InvocationExpression(memberAccessExpression, newArgumentList));

        var newRoot = root.ReplaceNode(assertThatInvocation, assertThatAsyncInvocation);
        context.RegisterCodeFix(
            CodeAction.Create(
                UseAssertThatAsyncConstants.Title,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                UseAssertThatAsyncConstants.Description),
            diagnostic);
    }
}
