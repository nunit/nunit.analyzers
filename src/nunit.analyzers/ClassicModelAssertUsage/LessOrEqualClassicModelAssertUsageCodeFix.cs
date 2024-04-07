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
    public sealed class LessOrEqualClassicModelAssertUsageCodeFix
        : ClassicModelAssertUsageCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.LessOrEqualUsage);

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var arg2 = argumentNamesToArguments[NUnitFrameworkConstants.NameOfArg2Parameter];
            var expectedArgumentNameColon = arg2.NameColon is null
                ? null
                : SyntaxFactory.NameColon(NUnitFrameworkConstants.NameOfExpectedParameter);
            var constraintArgument = SyntaxFactory.Argument(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsLessThanOrEqualTo)))
                .WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        arg2.WithNameColon(expectedArgumentNameColon)))));

            var arg1Argument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfArg1Parameter];
            var actualArgumentNameColon = arg1Argument.NameColon is null
                ? null
                : SyntaxFactory.NameColon(NUnitFrameworkConstants.NameOfActualParameter);
            var actualArgument = arg1Argument.WithNameColon(actualArgumentNameColon);
            return (actualArgument, constraintArgument);
        }
    }
}
