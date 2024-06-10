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
    public class SomeItemsConstraintUsageCodeFix : BaseConditionConstraintCodeFix
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.CollectionContainsConstraintUsage);

        protected override (ExpressionSyntax? actual, ExpressionSyntax? constraintExpression) GetActualAndConstraintExpression(ExpressionSyntax conditionNode, string suggestedConstraintString)
        {
            // actual.Contains(expected)
            if (conditionNode is InvocationExpressionSyntax invocation
               && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var actual = memberAccess.Expression;
                var expected = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                var constraintExpression = GetConstraintExpression(suggestedConstraintString, expected);

                return (actual, constraintExpression);
            }
            else
            {
                return (null, null);
            }
        }
    }
}
