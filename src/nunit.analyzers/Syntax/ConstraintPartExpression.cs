using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.Syntax
{
    internal class ConstraintPartExpression
    {
        private readonly SemanticModel semanticModel;
        private readonly IReadOnlyList<ExpressionSyntax> expressions;

        public ConstraintPartExpression(IReadOnlyList<ExpressionSyntax> expressions, SemanticModel semanticModel)
        {
            this.expressions = expressions;
            this.semanticModel = semanticModel;
        }

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

        public ExpressionSyntax RootExpression
        {
            get
            {
                return this.expressions.FirstOrDefault(e => ReturnsConstraint(e, this.semanticModel));
            }
        }

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

        public string[] GetPrefixesNames()
        {
            return this.PrefixExpressions
                .Select(e => GetName(e))
                .Where(e => e != null)
                .ToArray();
        }

        public string[] GetSuffixesNames()
        {
            return this.SuffixExpressions
                .Select(e => GetName(e))
                .Where(e => e != null)
                .ToArray();
        }

        public ExpressionSyntax GetPrefixExpression(string name)
        {
            return this.PrefixExpressions.FirstOrDefault(p => GetName(p) == name);
        }

        public ExpressionSyntax GetSuffixExpression(string name)
        {
            return this.SuffixExpressions.FirstOrDefault(s => GetName(s) == name);
        }

        public string GetConstraintName()
        {
            return GetName(this.RootExpression);
        }

        public ExpressionSyntax GetExpectedArgumentExpression()
        {
            if (this.RootExpression is InvocationExpressionSyntax invocationExpression)
            {
                return invocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            }

            return null;
        }

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
                returnType = methodSymbol.ReturnType;
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                returnType = propertySymbol.Type;
            }

            return returnType != null && returnType.IsConstraint();
        }
    }
}
