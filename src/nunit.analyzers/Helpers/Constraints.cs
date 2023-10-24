using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NUnit.Analyzers.Helpers
{
    internal sealed class Constraints
    {
        public Constraints(string staticClass, string? modifier, string? constraintMethod, string? property1 = default, string? property2 = null)
        {
            this.StaticClass = staticClass;
            this.Modifier = modifier;
            this.ConstraintMethod = constraintMethod;
            this.Property1 = property1;
            this.Property2 = property2;
        }

        public string StaticClass { get; }
        public string? Modifier { get; }
        public string? ConstraintMethod { get; }
        public string? Property1 { get; }
        public string? Property2 { get; }

        public ExpressionSyntax CreateConstraint(ArgumentSyntax? expected = null)
        {
            ExpressionSyntax expression = IdentifierName(this.StaticClass);

            if (this.Modifier is not null)
            {
                expression = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    IdentifierName(this.Modifier));
            }

            if (this.ConstraintMethod is not null)
            {
                expression = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression,
                        IdentifierName(this.ConstraintMethod)),
                    ArgumentList(expected is null ? default : SingletonSeparatedList(expected)));
            }

            if (this.Property1 is not null)
            {
                expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression,
                        IdentifierName(this.Property1));
            }

            if (this.Property2 is not null)
            {
                expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression,
                        IdentifierName(this.Property2));
            }

            return expression;
        }
    }
}
