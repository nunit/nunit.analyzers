using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Syntax;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Syntax
{
    public class ConstraintPartExpressionTests
    {
        [Test]
        public async Task ConstraintMethodWithNoPrefixesAndSuffixes()
        {
            var constraintPart = await CreateConstraintPart("Does.Contain(1)");

            Assert.That(constraintPart.HelperClassIdentifier.ToString(), Is.EqualTo("Does"));

            Assert.That(constraintPart.SuffixExpressions, Is.Empty);
            Assert.That(constraintPart.PrefixExpressions, Is.Empty);

            Assert.That(constraintPart.RootExpression, IsInvocation("Contain(1)"));
        }

        [Test]
        public async Task ConstraintPropertyWithNoPrefixesAndSuffixes()
        {
            var constraintPart = await CreateConstraintPart("Is.Null");

            Assert.That(constraintPart.HelperClassIdentifier.ToString(), Is.EqualTo("Is"));

            Assert.That(constraintPart.SuffixExpressions, Is.Empty);
            Assert.That(constraintPart.PrefixExpressions, Is.Empty);

            Assert.That(constraintPart.RootExpression, IsMemberAccess("Null"));
        }

        [Test]
        public async Task WithPropertyPrefix()
        {
            var constraintPart = await CreateConstraintPart("Has.Some.EqualTo(1)");

            Assert.That(constraintPart.HelperClassIdentifier.ToString(), Is.EqualTo("Has"));

            Assert.That(constraintPart.SuffixExpressions, Is.Empty);
            Assert.That(constraintPart.RootExpression, IsInvocation("EqualTo(1)"));

            Assert.That(constraintPart.PrefixExpressions.Single(), IsMemberAccess("Some"));
        }

        [Test]
        public async Task WithMethodPrefix()
        {
            var constraintPart = await CreateConstraintPart("Has.Property(\"Prop\").EqualTo(2)");

            Assert.That(constraintPart.HelperClassIdentifier.ToString(), Is.EqualTo("Has"));
            Assert.That(constraintPart.SuffixExpressions, Is.Empty);
            Assert.That(constraintPart.RootExpression, IsInvocation("EqualTo(2)"));

            Assert.That(constraintPart.PrefixExpressions.Single(), IsInvocation("Property(\"Prop\")"));
        }

        [Test]
        public async Task WithPropertySuffix()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(\"A\").IgnoreCase");

            Assert.That(constraintPart.HelperClassIdentifier.ToString(), Is.EqualTo("Is"));
            Assert.That(constraintPart.PrefixExpressions, Is.Empty);
            Assert.That(constraintPart.RootExpression, IsInvocation("EqualTo(\"A\")"));

            Assert.That(constraintPart.SuffixExpressions.Single(), IsMemberAccess("IgnoreCase"));
        }

        [Test]
        public async Task WithMethodSuffix()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(1).After(10)");

            Assert.That(constraintPart.HelperClassIdentifier.ToString(), Is.EqualTo("Is"));
            Assert.That(constraintPart.PrefixExpressions, Is.Empty);
            Assert.That(constraintPart.RootExpression, IsInvocation("EqualTo(1)"));

            Assert.That(constraintPart.SuffixExpressions.Single(), IsInvocation("After(10)"));
        }

        [Test]
        public async Task ConstraintIsBuiltUsingMethodOperator()
        {
            var constraintParts = await CreateConstraintParts("Is.Empty.Or.Some.EqualTo(\"A\").IgnoreCase");

            var firstPart = constraintParts[0];
            Assert.That(firstPart.HelperClassIdentifier.ToString(), Is.EqualTo("Is"));
            Assert.That(firstPart.PrefixExpressions, Is.Empty);
            Assert.That(firstPart.RootExpression, IsMemberAccess("Empty"));
            Assert.That(firstPart.SuffixExpressions, Is.Empty);

            var secondPart = constraintParts[1];
            Assert.That(secondPart.HelperClassIdentifier, Is.Null);
            Assert.That(secondPart.PrefixExpressions.Single(), IsMemberAccess("Some"));
            Assert.That(secondPart.RootExpression, IsInvocation("EqualTo(\"A\")"));
            Assert.That(secondPart.SuffixExpressions.Single(), IsMemberAccess("IgnoreCase"));
        }

        [Test]
        public async Task ConstraintIsCreatedViaConstructor()
        {
            var constraintPart = await CreateConstraintPart("new NUnit.Framework.Constraints.EqualConstraint(1)");

            Assert.That(constraintPart.HelperClassIdentifier, Is.Null);
            Assert.That(constraintPart.PrefixExpressions, Is.Empty);
            Assert.That(constraintPart.SuffixExpressions, Is.Empty);

            Assert.That(constraintPart.RootExpression, Is.TypeOf<ObjectCreationExpressionSyntax>());
            Assert.That(constraintPart.RootExpression.ToString(), Is.EqualTo("new NUnit.Framework.Constraints.EqualConstraint(1)"));
        }

        [Test]
        public async Task PrefixNameReturnsPropertyName()
        {
            var constraintPart = await CreateConstraintPart("Is.Not.Null");

            Assert.That(constraintPart.GetPrefixesNames(), Is.EqualTo(new[] { "Not" }));
        }

        [Test]
        public async Task PrefixNameReturnsMethodName()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).EqualTo(2)");

            Assert.That(constraintPart.GetPrefixesNames(), Is.EqualTo(new[] { "Exactly" }));
        }

        [Test]
        public async Task SuffixNameReturnsPropertyName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(\"A\").IgnoreCase");

            Assert.That(constraintPart.GetSuffixesNames(), Is.EqualTo(new[] { "IgnoreCase" }));
        }

        [Test]
        public async Task SuffixNameReturnsMethodName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(1).After(100, 10)");

            Assert.That(constraintPart.GetSuffixesNames(), Is.EqualTo(new[] { "After" }));
        }

        [Test]
        public async Task GetConstraintNameReturnsPropertyName()
        {
            var constraintPart = await CreateConstraintPart("Is.Not.Empty");

            Assert.That(constraintPart.GetConstraintName(), Is.EqualTo("Empty"));
        }

        [Test]
        public async Task GetConstraintNameReturnsMethodName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(new[] {1, 2}).IgnoreCase");

            Assert.That(constraintPart.GetConstraintName(), Is.EqualTo("EqualTo"));
        }

        [Test]
        public async Task GetHelperClassNameReturnsIdentifierName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(1).IgnoreCase");

            Assert.That(constraintPart.GetHelperClassName(), Is.EqualTo("Is"));
        }

        [Test]
        public async Task GetPrefixExpressionByName()
        {
            var constraintPart = await CreateConstraintPart("Has.Property(\"Prop\").EqualTo(1)");

            var prefixExpression = constraintPart.GetPrefixExpression("Property");

            Assert.That(prefixExpression, IsInvocation("Property(\"Prop\")"));
        }

        [Test]
        public async Task GetSuffixExpressionByName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(\"a\").IgnoreCase");

            var suffixExpression = constraintPart.GetSuffixExpression("IgnoreCase");

            Assert.That(suffixExpression, IsMemberAccess("IgnoreCase"));
        }

        [Test]
        public async Task GetConstraintMethodReturnsMethodSymbol()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).Items.EqualTo(1.0).Within(0.01)");

            var methodSymbol = constraintPart.GetConstraintMethod();

            Assert.That(methodSymbol, Is.Not.Null);
            Assert.That(methodSymbol.Name, Is.EqualTo("EqualTo"));
            Assert.That(methodSymbol.ContainingAssembly.Name, Is.EqualTo(NunitFrameworkConstants.NUnitFrameworkAssemblyName));
        }

        [Test]
        public async Task GetConstraintMethodReturnsNullForPropertyConstraint()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).Items.Null.After(1)");

            Assert.That(constraintPart.GetConstraintMethod(), Is.Null);
        }

        [Test]
        public async Task GetExpectedArgumentExpression()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).Items.EqualTo(new[] {1, 2, 3}).After(1)");

            var expectedExpression = constraintPart.GetExpectedArgumentExpression();

            Assert.That(expectedExpression.ToString(), Is.EqualTo("new[] {1, 2, 3}"));
        }

        [Test]
        public async Task HasUnknownExpressionsReturnsTrueIfTernaryExpressionPresent()
        {
            var constraintPart = await CreateConstraintPart("(true ? Has.Some : Has.None).EqualTo(1)");

            Assert.That(constraintPart.HasUnknownExpressions(), Is.True);
        }

        [Test]
        public async Task HasUnknownExpressionsReturnsTrueIfVariablePresent()
        {
            var constraintPart = (await CreateConstraintParts(
                testMethod: @"
                    var presentExpected = true;
                    var prefix = presentExpected ? Has.Some : Has.None;
                    Assert.That(new[] {1, 2, 3}, prefix.EqualTo(1));",
                expressionString: "prefix.EqualTo(1)"))[0];

            Assert.That(constraintPart.HasUnknownExpressions(), Is.True);
        }

        private static Framework.Constraints.Constraint IsInvocation(string text)
        {
            return new Framework.Constraints.PredicateConstraint<ExpressionSyntax>(e =>
            {
                return e is InvocationExpressionSyntax invocation
                    && invocation.Expression is MemberAccessExpressionSyntax memberAccess
                    && (memberAccess.Name.ToString() + invocation.ArgumentList.ToString()) == text;
            });
        }

        private static Framework.Constraints.Constraint IsMemberAccess(string text)
        {
            return new Framework.Constraints.PredicateConstraint<ExpressionSyntax>(e =>
            {
                return e is MemberAccessExpressionSyntax memberAccess
                    && memberAccess.Name.ToString() == text;
            });
        }

        private static async Task<ConstraintPartExpression> CreateConstraintPart(string expressionString)
        {
            return (await CreateConstraintParts(expressionString)).Single();
        }

        private static Task<ConstraintPartExpression[]> CreateConstraintParts(string expressionString)
        {
            return CreateConstraintParts($"Assert.That(1, {expressionString});", expressionString);
        }

        private static async Task<ConstraintPartExpression[]> CreateConstraintParts(string testMethod, string expressionString)
        {
            var testCode = TestUtility.WrapInTestMethod(testMethod);
            var (node, model) = await TestHelpers.GetRootAndModel(testCode);

            var expression = node.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .First(e => e.ToString() == expressionString);

            return new ConstraintExpression(expression, model).ConstraintParts;
        }
    }
}
