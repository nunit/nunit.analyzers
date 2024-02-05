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

        protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
        {
            // Note that if there's a 3rd argument and it's a double,
            // it has to be added to the "Is.EqualTo(1st argument)" with ".Within(3rd argument)"
            var equalToInvocationNode = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIs),
                    SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfIsEqualTo)))
                .WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(arguments[0])));

            var hasToleranceValue = diagnostic.Properties[AnalyzerPropertyKeys.HasToleranceValue] == true.ToString();

            if (hasToleranceValue)
            {
                // The tolerance argument should be renamed from 'delta' to 'amount' but with the model constraint the
                // argument is moved to Within which makes it way more explicit so we can just drop the name colon.
                var toleranceArgumentNoColon = arguments[2].WithNameColon(null);

                equalToInvocationNode = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        equalToInvocationNode,
                        SyntaxFactory.IdentifierName(NUnitFrameworkConstants.NameOfEqualConstraintWithin)))
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(toleranceArgumentNoColon)));
            }

            arguments.Insert(2, SyntaxFactory.Argument(equalToInvocationNode));

            // Then we have to remove the 1st argument because that's now in the "Is.EqualTo()"
            arguments.RemoveAt(0);

            // ... and if the 3rd argument was a double, that has to go too.
            if (hasToleranceValue)
            {
                arguments.RemoveAt(2);
            }
        }
    }
}
