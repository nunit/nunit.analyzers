using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.Syntax
{
    /// <summary>
    /// Represents part of <see cref="ConstraintExpression"/>, which is combined with others with 
    /// binary operators ('&', '|')  or methods ('And', 'Or', 'With').
    /// </summary>
    internal class ConstraintPartExpression
    {
        private readonly SemanticModel semanticModel;
        private readonly IReadOnlyList<ExpressionSyntax> expressions;

        public ConstraintPartExpression(IReadOnlyList<ExpressionSyntax> expressions, SemanticModel semanticModel)
        {
            this.expressions = expressions;
            this.semanticModel = semanticModel;
        }

        /// <summary>
        /// Constraint modifiers that go before actual constraint,
        /// e.g. 'Not', 'Some', 'Property(propertyName)', etc.
        /// </summary>
        public IEnumerable<ExpressionSyntax> PrefixExpressions
        {
            get
            {
                // e.g. Has.Property("Prop").Not.EqualTo("1")
                // -->
                // Has.Property("Prop"),
                // Has.Property("Prop").Not

                // Take expressions until found expression returns a constraint
                return this.expressions
                    .Where(e => e is MemberAccessExpressionSyntax || e is InvocationExpressionSyntax)
                    .TakeWhile(e => !ReturnsConstraint(e, this.semanticModel));
            }
        }

        /// <summary>
        /// Actual constraint, e.g. 'EqualTo(expected)', 'Null', 'Empty', etc.
        /// </summary>
        public ExpressionSyntax RootExpression
        {
            get
            {
                return this.expressions.FirstOrDefault(e => ReturnsConstraint(e, this.semanticModel));
            }
        }

        /// <summary>
        /// Constraint modifiers that go after actual constraint,
        /// e.g. 'IgnoreCase', 'After(timeout)', 'Within(range)', etc.
        /// </summary>
        public IEnumerable<ExpressionSyntax> SuffixExpressions
        {
            get
            {
                // e.g. Has.Property("Prop").Not.EqualTo("1").IgnoreCase
                // -->
                // Has.Property("Prop").Not.EqualTo("1").IgnoreCase

                // Skip all suffixes, and first expression returning constraint (e.g. 'EqualTo("1")')
                return this.expressions
                    .SkipWhile(e => !ReturnsConstraint(e, this.semanticModel))
                    .Skip(1);
            }
        }

        /// <summary>
        /// Returns prefixes names (i.e. method or property names).
        /// </summary>
        public string[] GetPrefixesNames()
        {
            return this.PrefixExpressions
                .Select(e => GetName(e))
                .Where(e => e != null)
                .ToArray();
        }

        /// <summary>
        /// Returns suffixes names (i.e. method or property names).
        /// </summary>
        public string[] GetSuffixesNames()
        {
            return this.SuffixExpressions
                .Select(e => GetName(e))
                .Where(e => e != null)
                .ToArray();
        }

        /// <summary>
        /// Returns prefix expression with provided name, or null, if none found.
        /// </summary>
        public ExpressionSyntax GetPrefixExpression(string name)
        {
            return this.PrefixExpressions.FirstOrDefault(p => GetName(p) == name);
        }

        /// <summary>
        /// Returns suffix expression with provided name, or null, if none found.
        /// </summary>
        public ExpressionSyntax GetSuffixExpression(string name)
        {
            return this.SuffixExpressions.FirstOrDefault(s => GetName(s) == name);
        }

        /// <summary>
        /// Returns constraint root name (i.e. method or property names).
        /// </summary>
        public string GetConstraintName()
        {
            return GetName(this.RootExpression);
        }

        /// <summary>
        /// If constraint root is method, returns expected argument expression.
        /// Otherwise - null.
        /// </summary>
        public ExpressionSyntax GetExpectedArgumentExpression()
        {
            if (this.RootExpression is InvocationExpressionSyntax invocationExpression)
            {
                return invocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            }

            return null;
        }

        /// <summary>
        /// If constraint root is method, return corresponding <see cref="IMethodSymbol"/>.
        /// Otherwise (e.g. if constraint root is property) - null.
        /// </summary>
        public IMethodSymbol GetConstraintMethod()
        {
            if (this.RootExpression == null)
                return null;

            var symbol = this.semanticModel.GetSymbolInfo(this.RootExpression).Symbol;
            return symbol as IMethodSymbol;
        }

        /// <summary>
        /// Returns true if part has any conditional or variable expressions.
        /// </summary>
        public bool HasUnknownExpressions()
        {
            foreach (var part in this.expressions)
            {
                switch (part)
                {
                    // e.g. '.EqualTo(1)'
                    case InvocationExpressionSyntax _:
                    // e.g. '.IgnoreCase'
                    case MemberAccessExpressionSyntax _:
                        break;

                    // Identifier is allowed only if it is type Symbol
                    // e.g. 'Is', 'Has', etc.
                    case IdentifierNameSyntax _:
                        var symbol = this.semanticModel.GetSymbolInfo(part).Symbol;

                        if (!(symbol is ITypeSymbol typeSymbol)
                            || typeSymbol.ContainingAssembly.Name != NunitFrameworkConstants.NUnitFrameworkAssemblyName)
                        {
                            return true;
                        }

                        break;

                    default:
                        return true;
                }
            }

            return false;
        }

        public TextSpan Span
        {
            get
            {
                int spanStart;

                // If constraint is built using And/Or methods, and current part is second operand, 
                // we need to cut after operator.
                if (this.expressions[0] is MemberAccessExpressionSyntax memberAccess)
                    spanStart = memberAccess.Name.Span.Start;
                else if (this.expressions[0] is InvocationExpressionSyntax invocation && invocation.Expression is MemberAccessExpressionSyntax invokedMemberAccess)
                    spanStart = invokedMemberAccess.Name.Span.Start;
                else
                    spanStart = this.expressions[0].SpanStart;

                var spanEnd = this.expressions.Last().Span.End;

                return TextSpan.FromBounds(spanStart, spanEnd);
            }
        }

        public override string ToString()
        {
            var lastExpression = this.expressions.Last();
            var startDelta = this.Span.Start - lastExpression.Span.Start;

            return lastExpression.ToString().Substring(startDelta);
        }

        private static string GetName(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Name.Identifier.Text;
                case InvocationExpressionSyntax invocation:
                    return GetName(invocation.Expression);
                default:
                    return null;
            }
        }

        private static bool ReturnsConstraint(ExpressionSyntax expressionSyntax, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetSymbolInfo(expressionSyntax).Symbol;

            ITypeSymbol returnType = null;

            if (symbol is IMethodSymbol methodSymbol)
            {
                returnType = methodSymbol.MethodKind == MethodKind.Constructor
                    ? methodSymbol.ContainingType
                    : methodSymbol.ReturnType;
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                returnType = propertySymbol.Type;
            }

            return returnType != null && returnType.IsConstraint();
        }
    }
}
