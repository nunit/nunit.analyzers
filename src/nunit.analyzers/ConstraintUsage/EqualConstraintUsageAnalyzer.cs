using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualConstraintUsageAnalyzer : BaseAssertionAnalyzer
    {
        internal const string SuggestedConstraintString = nameof(SuggestedConstraintString);

        private static readonly string IsEqualTo = $"{NameOfIs}.{NameOfIsEqualTo}";
        private static readonly string IsNotEqualTo = $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEqualTo}";

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.EqualConstraintUsage,
            title: EqualConstraintUsageConstants.Title,
            messageFormat: EqualConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: EqualConstraintUsageConstants.Description);

        private static readonly string[] SupportedPositiveAssertMethods = new[] {
            NameOfAssertThat,
            NameOfAssertTrue,
            NameOfAssertIsTrue
        };

        private static readonly string[] SupportedNegativeAssertMethods = new[] {
            NameOfAssertFalse,
            NameOfAssertIsFalse
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
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

            var shouldReport = false;

            // 'actual == expected', 'Equals(actual, expected)' or 'actual.Equals(expected)'
            if (actual.IsKind(SyntaxKind.EqualsExpression)
                || IsStaticObjectEquals(actual, context)
                || IsInstanceObjectEquals(actual, context))
            {
                shouldReport = true;
            }

            // 'actual != expected', '!Equals(actual, expected)' or '!actual.Equals(expected)'
            else if (actual.IsKind(SyntaxKind.NotEqualsExpression)
                || (IsPrefixNotExpression(actual, out var notOperand)
                    && (IsStaticObjectEquals(notOperand, context)
                        || IsInstanceObjectEquals(notOperand, context))))
            {
                shouldReport = true;
                negated = !negated;
            }

            if (shouldReport)
            {
                var suggestedConstraint = negated ? IsNotEqualTo : IsEqualTo;
                var properties = ImmutableDictionary.Create<string, string>().Add(SuggestedConstraintString, suggestedConstraint);
                context.ReportDiagnostic(Diagnostic.Create(descriptor, actual.GetLocation(), properties, suggestedConstraint));
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

        private static bool IsStaticObjectEquals(ExpressionSyntax expressionSyntax, SyntaxNodeAnalysisContext context)
        {
            if (!(expressionSyntax is InvocationExpressionSyntax invocation))
                return false;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

            return methodSymbol != null
                && methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 2
                && methodSymbol.Name == nameof(object.Equals)
                && methodSymbol.ContainingType.SpecialType == SpecialType.System_Object;
        }

        private static bool IsInstanceObjectEquals(ExpressionSyntax expressionSyntax, SyntaxNodeAnalysisContext context)
        {
            if (!(expressionSyntax is InvocationExpressionSyntax invocation))
                return false;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

            return methodSymbol != null
                && !methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.Name == nameof(object.Equals);
        }
    }
}
