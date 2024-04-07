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
    public sealed class GreaterClassicModelAssertUsageCodeFix
        : ClassicModelAssertUsageCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.GreaterUsage);

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
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsGreaterThan)))
                .WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        arg2.WithNameColon(expectedArgumentNameColon)))));

            var arg1 = argumentNamesToArguments[NUnitFrameworkConstants.NameOfArg1Parameter];
            var actualArgumentNameColon = arg1.NameColon is null
                ? null
                : SyntaxFactory.NameColon(NUnitFrameworkConstants.NameOfActualParameter);
            var actualArgument = arg1.WithNameColon(actualArgumentNameColon);
            return (actualArgument, constraintArgument);
        }
    }
}
