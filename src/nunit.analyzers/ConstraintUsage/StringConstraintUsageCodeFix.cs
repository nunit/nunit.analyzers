using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class StringConstraintUsageCodeFix : BaseConditionConstraintCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.StringContainsConstraintUsage,
            AnalyzerIdentifiers.StringStartsWithConstraintUsage,
            AnalyzerIdentifiers.StringEndsWithConstraintUsage);

        protected override (ExpressionSyntax? actual, ExpressionSyntax? constraintExpression) GetActualAndConstraintExpression(ExpressionSyntax conditionNode, string suggestedConstraintString)
        {
            // actual.Contains(expected)
            if (conditionNode is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var actual = memberAccess.Expression;
                var expected = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                var constraintExpression = GetConstraintExpression(suggestedConstraintString, expected);

                // Fix trivia
                return actual is not null && constraintExpression is not null
                    ? (actual.WithTriviaFrom(constraintExpression), constraintExpression.WithTriviaFrom(actual))
                    : (actual, constraintExpression);
            }
            else
            {
                return (null, null);
            }
        }
    }
}
