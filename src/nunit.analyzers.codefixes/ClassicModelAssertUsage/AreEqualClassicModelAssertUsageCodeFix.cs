using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public sealed class AreEqualClassicModelAssertUsageCodeFix
        : ClassicModelAssertUsageCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.AreEqualUsage);

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var expectedArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfExpectedParameter].WithNameColon(null);

            ExpressionSyntax constraint;

            if (CodeFixHelper.IsEmpty(expectedArgument.Expression))
            {
                constraint = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEmpty));
            }
            else
            {
                constraint = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEqualTo)))
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(expectedArgument.WithoutTrivia())));

                // The tolerance argument has to be added to the "Is.EqualTo(expected)" as ".Within(tolerance)"
                if (argumentNamesToArguments.TryGetValue(NUnitFrameworkConstants.NameOfDeltaParameter, out var toleranceArgument))
                {
                    // The tolerance argument should be renamed from 'delta' to 'amount' but with the model constraint the
                    // argument is moved to Within which makes it way more explicit so we can just drop the name colon.
                    var toleranceArgumentNoColon = toleranceArgument.WithNameColon(null);

                    constraint = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            constraint,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfEqualConstraintWithin)))
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(toleranceArgumentNoColon.WithoutTrivia())));
                }
            }

            var actualArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfActualParameter].WithNameColon(null);
            return (actualArgument, SyntaxFactory.Argument(constraint));
        }
    }
}
