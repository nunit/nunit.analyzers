using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.ConstActualValueUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstActualValueUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ConstActualValueUsage,
            title: ConstActualValueUsageAnalyzerConstants.Title,
            messageFormat: ConstActualValueUsageAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ConstActualValueUsageAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            bool IsLiteralExpression(ExpressionSyntax expression)
            {
                if (expression is LiteralExpressionSyntax)
                    return true;

                if (expression is PrefixUnaryExpressionSyntax prefixUnaryExpression)
                    return IsLiteralExpression(prefixUnaryExpression.Operand);

                if (expression is BinaryExpressionSyntax binaryExpression)
                    return IsLiteralExpression(binaryExpression.Left) && IsLiteralExpression(binaryExpression.Right);

                if (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
                    return IsLiteralExpression(parenthesizedExpression.Expression);

                return false;
            }

            bool IsConstant(ExpressionSyntax expression)
            {
                if (IsLiteralExpression(expression))
                    return true;

                var argumentSymbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;

                return (argumentSymbol is ILocalSymbol localSymbol && localSymbol.IsConst) ||
                       (argumentSymbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst);
            }

            bool IsEmpty(ExpressionSyntax expression)
            {
                return expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Name.Identifier.Text == "Empty";
            }

            void Report(ExpressionSyntax expression)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    expression.GetLocation()));
            }

            var actualExpression = assertExpression.GetArgumentExpression(methodSymbol, NunitFrameworkConstants.NameOfActualParameter);

            if (actualExpression == null)
                return;

            if (IsLiteralExpression(actualExpression))
            {
                // Classic error case
                Report(actualExpression);
                return;
            }

            if (!IsConstant(actualExpression) && !IsEmpty(actualExpression))
                return; // Standard use case

            // The actual expression is a constant field, check if expected is also constant
            var expectedExpression = assertExpression.GetArgumentExpression(methodSymbol, NunitFrameworkConstants.NameOfExpectedParameter);

            if (expectedExpression == null)
            {
                // Check for Assert.That IsEqualTo constraint
                expectedExpression = this.GetExpectedExpression(assertExpression, context.SemanticModel);
            }

            if (expectedExpression != null && !IsConstant(expectedExpression) && !IsEmpty(expectedExpression))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    actualExpression.GetLocation()));
            }
        }

        private ExpressionSyntax? GetExpectedExpression(InvocationExpressionSyntax assertExpression, SemanticModel semanticModel)
        {
            if (AssertHelper.TryGetActualAndConstraintExpressions(assertExpression, semanticModel,
                out _, out var constraintExpression))
            {
                return constraintExpression.ConstraintParts
                        .Select(part => part.GetExpectedArgumentExpression())
                        .Where(e => e != null)
                        .FirstOrDefault();
            }

            return null;
        }
    }
}
