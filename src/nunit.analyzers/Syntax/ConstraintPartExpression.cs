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

        private IdentifierNameSyntax helperClassIdentifier;
        private IReadOnlyCollection<ExpressionSyntax> prefixes;
        private ExpressionSyntax root;
        private IReadOnlyCollection<ExpressionSyntax> suffixes;

        public ConstraintPartExpression(IReadOnlyList<ExpressionSyntax> expressions, SemanticModel semanticModel)
        {
            this.expressions = expressions;
            this.semanticModel = semanticModel;
        }

        /// <summary>
        /// Helper class used to access constraints,
        /// e.g. 'Has', 'Is', 'Throws', 'Does', etc.
        /// </summary>
        public IdentifierNameSyntax HelperClassIdentifier
        {
            get
            {
                if (this.helperClassIdentifier == null)
                {
                    (this.helperClassIdentifier, this.prefixes, this.root, this.suffixes) = SplitExpressions(this.expressions, this.semanticModel);
                }
                return this.helperClassIdentifier;
            }
        }

        /// <summary>
        /// Constraint modifiers that go before actual constraint,
        /// e.g. 'Not', 'Some', 'Property(propertyName)', etc.
        /// </summary>
        public IReadOnlyCollection<ExpressionSyntax> PrefixExpressions
        {
            get
            {
                if (this.prefixes == null)
                {
                    (this.helperClassIdentifier, this.prefixes, this.root, this.suffixes) = SplitExpressions(this.expressions, this.semanticModel);
                }
                return this.prefixes;
            }
        }

        /// <summary>
        /// Actual constraint, e.g. 'EqualTo(expected)', 'Null', 'Empty', etc.
        /// </summary>
        public ExpressionSyntax RootExpression
        {
            get
            {
                if (this.root == null)
                {
                    (this.helperClassIdentifier, this.prefixes, this.root, this.suffixes) = SplitExpressions(this.expressions, this.semanticModel);
                }
                return this.root;
            }
        }

        /// <summary>
        /// Constraint modifiers that go after actual constraint,
        /// e.g. 'IgnoreCase', 'After(timeout)', 'Within(range)', etc.
        /// </summary>
        public IReadOnlyCollection<ExpressionSyntax> SuffixExpressions
        {
            get
            {
                if (this.suffixes == null)
                {
                    (this.helperClassIdentifier, this.prefixes, this.root, this.suffixes) = SplitExpressions(this.expressions, this.semanticModel);
                }
                return this.suffixes;
            }
        }

        public string GetHelperClassName()
        {
            return this.HelperClassIdentifier?.GetName();
        }

        /// <summary>
        /// Returns prefixes names (i.e. method or property names).
        /// </summary>
        public string[] GetPrefixesNames()
        {
            return this.PrefixExpressions
                .Select(e => e.GetName())
                .Where(e => e != null)
                .ToArray();
        }

        /// <summary>
        /// Returns suffixes names (i.e. method or property names).
        /// </summary>
        public string[] GetSuffixesNames()
        {
            return this.SuffixExpressions
                .Select(e => e.GetName())
                .Where(e => e != null)
                .ToArray();
        }

        /// <summary>
        /// Returns prefix expression with provided name, or null, if none found.
        /// </summary>
        public ExpressionSyntax GetPrefixExpression(string name)
        {
            return this.PrefixExpressions.FirstOrDefault(p => p.GetName() == name);
        }

        /// <summary>
        /// Returns suffix expression with provided name, or null, if none found.
        /// </summary>
        public ExpressionSyntax GetSuffixExpression(string name)
        {
            return this.SuffixExpressions.FirstOrDefault(s => s.GetName() == name);
        }

        /// <summary>
        /// Returns constraint root name (i.e. method or property names).
        /// </summary>
        public string GetConstraintName()
        {
            return this.RootExpression.GetName();
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

        private static (IdentifierNameSyntax identifier, IReadOnlyCollection<ExpressionSyntax> prefixes, ExpressionSyntax root, IReadOnlyCollection<ExpressionSyntax> suffixes)
            SplitExpressions(IReadOnlyList<ExpressionSyntax> expressions, SemanticModel semanticModel)
        {
            IdentifierNameSyntax helperIdentifierName = null;
            var prefixes = new List<ExpressionSyntax>();
            ExpressionSyntax root = null;
            var suffixes = new List<ExpressionSyntax>();

            var rootFound = false;

            for (var i = 0; i < expressions.Count; i++)
            {
                var expression = expressions[i];

                if (i == 0 && expressions.Count > 1 && expression is IdentifierNameSyntax identifierSyntax)
                {
                    // HelperIdentifier - first identifier expression (e.g. Has, Is, etc)
                    helperIdentifierName = identifierSyntax;
                }
                else if (!rootFound)
                {
                    if (!ReturnsConstraint(expression, semanticModel))
                    {
                        // Prefixes - take expressions until found expression that returns a constraint.
                        prefixes.Add(expression);
                    }
                    else
                    {
                        // Root - first expression that returns a constraint.
                        root = expression;
                        rootFound = true;
                    }
                }
                else
                {
                    // Suffixes - everything after root.
                    suffixes.Add(expression);
                }
            }

            return (helperIdentifierName, prefixes, root, suffixes);
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
