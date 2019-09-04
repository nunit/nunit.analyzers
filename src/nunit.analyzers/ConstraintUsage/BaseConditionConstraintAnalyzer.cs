using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Extensions;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    public abstract class BaseConditionConstraintAnalyzer : BaseAssertionAnalyzer
    {
        internal const string SuggestedConstraintString = nameof(SuggestedConstraintString);

        protected static readonly string[] SupportedPositiveAssertMethods = new[] {
            NameOfAssertThat,
            NameOfAssertTrue,
            NameOfAssertIsTrue
        };

        protected static readonly string[] SupportedNegativeAssertMethods = new[] {
            NameOfAssertFalse,
            NameOfAssertIsFalse
        };

        protected abstract (DiagnosticDescriptor descriptor, string suggestedConstraint) GetDiagnosticData(
            SyntaxNodeAnalysisContext context, ExpressionSyntax actual, bool negated);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            var negated = false;

            if (SupportedNegativeAssertMethods.Contains(methodSymbol.Name))
            {
                negated = true;
            }
            else if (!SupportedPositiveAssertMethods.Contains(methodSymbol.Name))
            {
                return;
            }

            var constraint = assertExpression.GetArgumentExpression(methodSymbol, NameOfExpressionParameter);

            // Constraint should be either absent, or Is.True, or Is.False
            if (constraint != null)
            {
                if (!(constraint is MemberAccessExpressionSyntax memberAccess
                       && memberAccess.Expression is IdentifierNameSyntax identifierSyntax
                       && identifierSyntax.Identifier.Text == NameOfIs
                       && (memberAccess.Name.Identifier.Text == NameOfIsTrue
                           || memberAccess.Name.Identifier.Text == NameOfIsFalse)))
                {
                    return;
                }

                if (memberAccess.Name.Identifier.Text == NameOfIsFalse)
                {
                    negated = !negated;
                }
            }

            var actual = assertExpression.GetArgumentExpression(methodSymbol, NameOfActualParameter)
                ?? assertExpression.GetArgumentExpression(methodSymbol, NameOfConditionParameter);

            if (IsPrefixNotExpression(actual, out var unwrappedActual))
            {
                negated = !negated;
            }

            var (descriptor, suggestedConstraint) = this.GetDiagnosticData(context, unwrappedActual ?? actual, negated);

            if (descriptor != null && suggestedConstraint != null)
            {
                var properties = ImmutableDictionary.Create<string, string>().Add(SuggestedConstraintString, suggestedConstraint);
                var diagnostic = Diagnostic.Create(descriptor, actual.GetLocation(), properties, suggestedConstraint);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsPrefixNotExpression(ExpressionSyntax expression, out ExpressionSyntax operand)
        {
            if (expression is PrefixUnaryExpressionSyntax unarySyntax
                && unarySyntax.IsKind(SyntaxKind.LogicalNotExpression))
            {
                operand = unarySyntax.Operand;
                return true;
            }
            else
            {
                operand = null;
                return false;
            }
        }
    }
}
