using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Operations;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Operations
{
    public class ConstraintExpressionPartTests
    {
        [Test]
        public async Task ConstraintMethodWithNoPrefixesAndSuffixes()
        {
            var constraintPart = await CreateConstraintPart("Does.Contain(1)").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass.Name, Is.EqualTo("Does"));

            Assert.That(constraintPart.Suffixes, Is.Empty);
            Assert.That(constraintPart.Prefixes, Is.Empty);

            Assert.That(constraintPart.Root, IsInvocation("Contain(1)"));
        }

        [Test]
        public async Task ConstraintPropertyWithNoPrefixesAndSuffixes()
        {
            var constraintPart = await CreateConstraintPart("Is.Null").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass.Name, Is.EqualTo("Is"));

            Assert.That(constraintPart.Suffixes, Is.Empty);
            Assert.That(constraintPart.Prefixes, Is.Empty);

            Assert.That(constraintPart.Root, IsMemberAccess("Null"));
        }

        [Test]
        public async Task WithPropertyPrefix()
        {
            var constraintPart = await CreateConstraintPart("Has.Some.EqualTo(1)").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass.Name, Is.EqualTo("Has"));

            Assert.That(constraintPart.Suffixes, Is.Empty);
            Assert.That(constraintPart.Root, IsInvocation("EqualTo(1)"));

            Assert.That(constraintPart.Prefixes.Single(), IsMemberAccess("Some"));
        }

        [Test]
        public async Task WithMethodPrefix()
        {
            var constraintPart = await CreateConstraintPart("Has.Property(\"Prop\").EqualTo(2)").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass.Name, Is.EqualTo("Has"));
            Assert.That(constraintPart.Suffixes, Is.Empty);
            Assert.That(constraintPart.Root, IsInvocation("EqualTo(2)"));

            Assert.That(constraintPart.Prefixes.Single(), IsInvocation("Property(\"Prop\")"));
        }

        [Test]
        public async Task WithPropertySuffix()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(\"A\").IgnoreCase").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass.Name, Is.EqualTo("Is"));
            Assert.That(constraintPart.Prefixes, Is.Empty);
            Assert.That(constraintPart.Root, IsInvocation("EqualTo(\"A\")"));

            Assert.That(constraintPart.Suffixes.Single(), IsMemberAccess("IgnoreCase"));
        }

        [Test]
        public async Task WithMethodSuffix()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(1).After(10)").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass.Name, Is.EqualTo("Is"));
            Assert.That(constraintPart.Prefixes, Is.Empty);
            Assert.That(constraintPart.Root, IsInvocation("EqualTo(1)"));

            Assert.That(constraintPart.Suffixes.Single(), IsInvocation("After(10)"));
        }

        [Test]
        public async Task ConstraintIsBuiltUsingMethodOperator()
        {
            var constraintParts = await CreateConstraintParts("Is.Empty.Or.Some.EqualTo(\"A\").IgnoreCase").ConfigureAwait(false);

            var firstPart = constraintParts[0];
            Assert.That(firstPart.HelperClass.Name, Is.EqualTo("Is"));
            Assert.That(firstPart.Prefixes, Is.Empty);
            Assert.That(firstPart.Root, IsMemberAccess("Empty"));
            Assert.That(firstPart.Suffixes, Is.Empty);

            var secondPart = constraintParts[1];
            Assert.That(secondPart.HelperClass, Is.Null);
            Assert.That(secondPart.Prefixes.Single(), IsMemberAccess("Some"));
            Assert.That(secondPart.Root, IsInvocation("EqualTo(\"A\")"));
            Assert.That(secondPart.Suffixes.Single(), IsMemberAccess("IgnoreCase"));
        }

        [Test]
        public async Task ConstraintIsCreatedViaConstructor()
        {
            var constraintPart = await CreateConstraintPart("new NUnit.Framework.Constraints.EqualConstraint(1)").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass, Is.Null);
            Assert.That(constraintPart.Prefixes, Is.Empty);
            Assert.That(constraintPart.Suffixes, Is.Empty);

            Assert.That(constraintPart.Root?.Syntax.ToString(), Is.EqualTo("new NUnit.Framework.Constraints.EqualConstraint(1)"));
        }

        [Test]
        public async Task PrefixNameReturnsPropertyName()
        {
            var constraintPart = await CreateConstraintPart("Is.Not.Null").ConfigureAwait(false);

            Assert.That(constraintPart.GetPrefixesNames(), Is.EqualTo(new[] { "Not" }));
        }

        [Test]
        public async Task PrefixNameReturnsMethodName()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).EqualTo(2)").ConfigureAwait(false);

            Assert.That(constraintPart.GetPrefixesNames(), Is.EqualTo(new[] { "Exactly" }));
        }

        [Test]
        public async Task SuffixNameReturnsPropertyName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(\"A\").IgnoreCase").ConfigureAwait(false);

            Assert.That(constraintPart.GetSuffixesNames(), Is.EqualTo(new[] { "IgnoreCase" }));
        }

        [Test]
        public async Task SuffixNameReturnsMethodName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(1).After(100, 10)").ConfigureAwait(false);

            Assert.That(constraintPart.GetSuffixesNames(), Is.EqualTo(new[] { "After" }));
        }

        [Test]
        public async Task GetConstraintNameReturnsPropertyName()
        {
            var constraintPart = await CreateConstraintPart("Is.Not.Empty").ConfigureAwait(false);

            Assert.That(constraintPart.GetConstraintName(), Is.EqualTo("Empty"));
        }

        [Test]
        public async Task GetConstraintNameReturnsMethodName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(new[] {1, 2}).IgnoreCase").ConfigureAwait(false);

            Assert.That(constraintPart.GetConstraintName(), Is.EqualTo("EqualTo"));
        }

        [Test]
        public async Task GetHelperClassNameReturnsIdentifierName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(1).IgnoreCase").ConfigureAwait(false);

            Assert.That(constraintPart.HelperClass.Name, Is.EqualTo("Is"));
        }

        [Test]
        public async Task GetPrefixExpressionByName()
        {
            var constraintPart = await CreateConstraintPart("Has.Property(\"Prop\").EqualTo(1)").ConfigureAwait(false);

            var prefixExpression = constraintPart.GetPrefix("Property");

            Assert.That(prefixExpression, IsInvocation("Property(\"Prop\")"));
        }

        [Test]
        public async Task GetSuffixExpressionByName()
        {
            var constraintPart = await CreateConstraintPart("Is.EqualTo(\"a\").IgnoreCase").ConfigureAwait(false);

            var suffixExpression = constraintPart.GetSuffix("IgnoreCase");

            Assert.That(suffixExpression, IsMemberAccess("IgnoreCase"));
        }

        [Test]
        public async Task GetConstraintMethodReturnsMethodSymbol()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).Items.EqualTo(1.0).Within(0.01)").ConfigureAwait(false);

            var methodSymbol = constraintPart.GetConstraintMethod();

            Assert.That(methodSymbol, Is.Not.Null);
            Assert.That(methodSymbol.Name, Is.EqualTo("EqualTo"));
            Assert.That(methodSymbol.ContainingAssembly.Name, Is.EqualTo(NunitFrameworkConstants.NUnitFrameworkAssemblyName));
        }

        [Test]
        public async Task GetConstraintMethodReturnsNullForPropertyConstraint()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).Items.Null.After(1)").ConfigureAwait(false);

            Assert.That(constraintPart.GetConstraintMethod(), Is.Null);
        }

        [Test]
        public async Task GetExpectedArgumentExpression()
        {
            var constraintPart = await CreateConstraintPart("Has.Exactly(2).Items.EqualTo(new[] {1, 2, 3}).After(1)").ConfigureAwait(false);

            var expectedExpression = constraintPart.GetExpectedArgument()?.Syntax;

            Assert.That(expectedExpression.ToString(), Is.EqualTo("new[] {1, 2, 3}"));
        }

        [Test]
        public async Task HasUnknownExpressionsReturnsTrueIfTernaryExpressionPresent()
        {
            var constraintPart = await CreateConstraintPart("(true ? Has.Some : Has.None).EqualTo(1)").ConfigureAwait(false);

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
                expressionString: "prefix.EqualTo(1)").ConfigureAwait(false))[0];

            Assert.That(constraintPart.HasUnknownExpressions(), Is.True);
        }

        private static Framework.Constraints.Constraint IsInvocation(string text)
        {
            return new Framework.Constraints.PredicateConstraint<IOperation>(o =>
            {
                return o is IInvocationOperation invocation
                    && $"{invocation.TargetMethod.Name}({string.Join(", ", invocation.Arguments.Select(a => a.Syntax.ToString()))})" == text;
            });
        }

        private static Framework.Constraints.Constraint IsMemberAccess(string text)
        {
            return new Framework.Constraints.PredicateConstraint<IOperation>(o =>
            {
                return o is IMemberReferenceOperation memberReference
                    && memberReference.Member.Name == text;
            });
        }

        private static async Task<ConstraintExpressionPart> CreateConstraintPart(string expressionString)
        {
            return (await CreateConstraintParts(expressionString).ConfigureAwait(false)).Single();
        }

        private static Task<ConstraintExpressionPart[]> CreateConstraintParts(string expressionString)
        {
            return CreateConstraintParts($"Assert.That(1, {expressionString});", expressionString);
        }

        private static async Task<ConstraintExpressionPart[]> CreateConstraintParts(string testMethod, string expressionString)
        {
            var testCode = TestUtility.WrapInTestMethod(testMethod);
            var (node, model) = await TestHelpers.GetRootAndModel(testCode).ConfigureAwait(false);

            var expression = node.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .First(e => e.ToString() == expressionString);

            var operation = model.GetOperation(expression);

            return new ConstraintExpression(operation).ConstraintParts;
        }
    }
}
