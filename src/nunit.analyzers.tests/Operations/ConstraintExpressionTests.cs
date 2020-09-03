using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Operations;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Operations
{
    public class ConstraintExpressionTests
    {
        [Test]
        public async Task SimpleIsExpression()
        {
            var constraintExpression = await CreateConstraintExpression("Is.EqualTo(1)").ConfigureAwait(false);

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "Is.EqualTo(1)" }));
        }

        [Test]
        public async Task ConstraintConstructor()
        {
            var constraintExpression = await CreateConstraintExpression(
                "new NUnit.Framework.Constraints.EqualConstraint(1)").ConfigureAwait(false);

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "new NUnit.Framework.Constraints.EqualConstraint(1)" }));
        }

        [TestCase("&")]
        [TestCase("|")]
        public async Task CombinedWithBinaryOperator(string @operator)
        {
            var constraintExpression = await CreateConstraintExpression(
                $"Has.Count.EqualTo(1) {@operator} Has.Some.EqualTo(2)").ConfigureAwait(false);

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "Has.Count.EqualTo(1)", "Has.Some.EqualTo(2)" }));
        }

        [TestCase("And")]
        [TestCase("Or")]
        [TestCase("With")]
        public async Task CombinedWithOperatorMethod(string method)
        {
            var constraintExpression = await CreateConstraintExpression(
                $"Has.Count.EqualTo(1).{method}.Some.EqualTo(2)").ConfigureAwait(false);

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "Has.Count.EqualTo(1)", "Some.EqualTo(2)" }));
        }

        [Test]
        public async Task CombinedWithMixedOperators()
        {
            var constraintExpression = await CreateConstraintExpression(
                "Is.Not.Empty & Is.EqualTo(new [] { \"1\" }).IgnoreCase.Or.EquivalentTo(new[] { \"1\", \"2\" })").ConfigureAwait(false);

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());

            var expectedParts = new[]
            {
                "Is.Not.Empty",
                "Is.EqualTo(new [] { \"1\" }).IgnoreCase",
                "EquivalentTo(new[] { \"1\", \"2\" })"
            };
            Assert.That(constraintParts, Is.EqualTo(expectedParts));
        }

        [Test]
        public async Task WhenWithIsNop()
        {
            var constraintExpression = await CreateConstraintExpression(
                "Has.Property(\"Foo\").With.Property(\"Bar\").EqualTo(\"Baz\")").ConfigureAwait(false);

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());

            Assert.That(constraintParts, Is.EqualTo(
                new[] { "Has.Property(\"Foo\").With.Property(\"Bar\").EqualTo(\"Baz\")" }));
        }

        private static async Task<ConstraintExpression> CreateConstraintExpression(string expressionString)
        {
            var testCode = TestUtility.WrapInTestMethod($"Assert.That(1, {expressionString});");
            var (node, model) = await TestHelpers.GetRootAndModel(testCode).ConfigureAwait(false);

            var expression = node.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .First(e => e.ToString() == expressionString);

            var operation = model.GetOperation(expression);

            return new ConstraintExpression(operation);
        }
    }
}
