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
    public sealed class IsNotInstanceOfClassicModelAssertUsageCodeFix
        : ClassicModelAssertUsageCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.IsNotInstanceOfUsage);

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments,
            TypeArgumentListSyntax typeArguments)
        {
            var constraintArgument = SyntaxFactory.Argument(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsNot)),
                        SyntaxFactory.GenericName(NUnitFrameworkConstants.NameOfIsInstanceOf)
                            .WithTypeArgumentList(typeArguments))));
            var actualArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfActualParameter];
            return (actualArgument, constraintArgument);
        }

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var expectedArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfExpectedParameter];
            var expectedArgumentNameColon = expectedArgument.NameColon is null
                ? null
                : SyntaxFactory.NameColon(NUnitFrameworkConstants.NameOfExpectedTypeParameter);
            var constraintArgument = SyntaxFactory.Argument(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsNot)),
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsInstanceOf)))
                .WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        expectedArgument.WithNameColon(expectedArgumentNameColon)))));

            var actualArgument = argumentNamesToArguments[NUnitFrameworkConstants.NameOfActualParameter];
            var actualArgumentNameColon = actualArgument.NameColon is null
                ? null
                : SyntaxFactory.NameColon(NUnitFrameworkConstants.NameOfActualParameter);
            return (actualArgument.WithNameColon(actualArgumentNameColon), constraintArgument);
        }
    }
}
