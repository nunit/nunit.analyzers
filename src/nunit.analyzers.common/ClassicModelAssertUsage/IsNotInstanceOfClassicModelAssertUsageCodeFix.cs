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

        protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments, TypeArgumentListSyntax typeArguments)
        {
            arguments.Insert(1, SyntaxFactory.Argument(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsNot)),
                        SyntaxFactory.GenericName(NUnitFrameworkConstants.NameOfIsInstanceOf)
                            .WithTypeArgumentList(typeArguments)))));
        }

        protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
        {
            arguments.Insert(2, SyntaxFactory.Argument(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                            SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsNot)),
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsInstanceOf)))
                .WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(arguments[0])))));

            // Then we have to remove the 1st argument because that's now in the "Is.Not.InstanceOf()"
            arguments.RemoveAt(0);
        }
    }
}
