using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
    {
        NUnitFrameworkConstants.NameOfActualParameter,
        NUnitFrameworkConstants.NameOfConditionParameter,
    };

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AnalyzerIdentifiers.UseAssertThatAsync);

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

        // The first parameter is usually the "actual" parameter, but sometimes it's the "condition" parameter.
        // Since the order is not guaranteed, let's just call it "actualArgument" here.
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

        // All overloads of Assert.ThatAsync have an IResolveConstraint parameter,
        // but not all overloads of Assert.That do. Therefore, we add Is.True to
        // those Assert.That(bool) overloads.
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;
        var nonLambdaArguments = argumentList.Arguments
            .Where(a => a != actualArgument)
            .Select(a => a.WithNameColon(null))
            .ToList();
        var needToPrependIsTrue = !argumentList.Arguments
            .Any(argument => ArgumentExtendsIResolveConstraint(argument, semanticModel, context.CancellationToken));
        if (needToPrependIsTrue)
        {
            nonLambdaArguments.Insert(
                0,
                SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsTrue))));
        }

        var newArgumentList = SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList(
                new[] { SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(insideLambda)) }
                    .Concat(nonLambdaArguments)));

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

    private static bool ArgumentExtendsIResolveConstraint(ArgumentSyntax argumentSyntax, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var argumentExpression = argumentSyntax.Expression;
        var argumentTypeInfo = semanticModel.GetTypeInfo(argumentExpression, cancellationToken);
        var argumentType = argumentTypeInfo.Type;

        var iResolveConstraintSymbol = semanticModel.Compilation.GetTypeByMetadataName("NUnit.Framework.Constraints.IResolveConstraint");

        return argumentType is not null
            && iResolveConstraintSymbol is not null
            && semanticModel.Compilation.ClassifyConversion(argumentType, iResolveConstraintSymbol).IsImplicit;
    }
}
