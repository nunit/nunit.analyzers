using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Operations;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Operations
{
    public class ConstraintOperationTests
    {
        [Test]
        public async Task SimpleIsExpression()
        {
            var constraintExpression = await CreateConstraintOperation("Is.EqualTo(1)");

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "Is.EqualTo(1)" }));
        }

        [Test]
        public async Task ConstraintConstructor()
        {
            var constraintExpression = await CreateConstraintOperation(
                "new NUnit.Framework.Constraints.EqualConstraint(1)");

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "new NUnit.Framework.Constraints.EqualConstraint(1)" }));
        }

        [TestCase("&")]
        [TestCase("|")]
        public async Task CombinedWithBinaryOperator(string @operator)
        {
            var constraintExpression = await CreateConstraintOperation(
                $"Has.Count.EqualTo(1) {@operator} Has.Some.EqualTo(2)");

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "Has.Count.EqualTo(1)", "Has.Some.EqualTo(2)" }));
        }

        [TestCase("And")]
        [TestCase("Or")]
        [TestCase("With")]
        public async Task CombinedWithOperatorMethod(string method)
        {
            var constraintExpression = await CreateConstraintOperation(
                $"Has.Count.EqualTo(1).{method}.Some.EqualTo(2)");

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] { "Has.Count.EqualTo(1)", "Some.EqualTo(2)" }));
        }

        [Test]
        public async Task CombinedWithMixedOperators()
        {
            var constraintExpression = await CreateConstraintOperation(
                "Is.Not.Empty & Is.EqualTo(new [] { \"1\" }).IgnoreCase.Or.EquivalentTo(new[] { \"1\", \"2\" })");

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] {
                "Is.Not.Empty", "Is.EqualTo(new [] { \"1\" }).IgnoreCase", "EquivalentTo(new[] { \"1\", \"2\" })" }));
        }

        [Test]
        public async Task WhenWithIsNop()
        {
            var constraintExpression = await CreateConstraintOperation(
                "Has.Property(\"Foo\").With.Property(\"Bar\").EqualTo(\"Baz\")");

            var constraintParts = constraintExpression.ConstraintParts.Select(p => p.ToString());
            Assert.That(constraintParts, Is.EqualTo(new[] {
                "Has.Property(\"Foo\").With.Property(\"Bar\").EqualTo(\"Baz\")" }));
        }

        private static async Task<ConstraintExpression> CreateConstraintOperation(string expressionString)
        {
            var testCode = TestUtility.WrapInTestMethod($"Assert.That(1, {expressionString});");
            var (node, model) = await TestHelpers.GetRootAndModel(testCode);

            var d = model.Compilation.GetDiagnostics();

            var expression = node.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .First(e => e.ToString() == expressionString);

            var operation = model.GetOperation(expression);

            return new ConstraintExpression(operation);
        }
    }
}
