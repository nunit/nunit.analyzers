using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public sealed class AreEqualClassicModelAssertUsageCodeFix
        : ClassicModelAssertUsageCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.AreEqualUsage);

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax ConstraintArgument) UpdateArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var expectedArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfExpectedParameter];
            var actualArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfActualParameter];

            // Note that if there's a 3rd argument and it's a double,
            // it has to be added to the "Is.EqualTo(1st argument)" with ".Within(3rd argument)"
            var equalToInvocationNode = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEqualTo)))
                .WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(expectedArgument)));

            const string NameOfDeltaParameter = "delta";
            if (argumentNamesToArguments.TryGetValue(NameOfDeltaParameter, out var toleranceArgument))
            {
                // The tolerance argument should be renamed from 'delta' to 'amount' but with the model constraint the
                // argument is moved to Within which makes it way more explicit so we can just drop the name colon.
                var toleranceArgumentNoColon = toleranceArgument.WithNameColon(null);

                equalToInvocationNode = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        equalToInvocationNode,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfEqualConstraintWithin)))
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(toleranceArgumentNoColon)));
            }

            return (actualArgument, SyntaxFactory.Argument(equalToInvocationNode));
        }
    }
}
