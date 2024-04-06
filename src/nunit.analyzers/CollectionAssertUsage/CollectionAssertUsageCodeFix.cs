using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;

namespace NUnit.Analyzers.CollectionAssertUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    internal class CollectionAssertUsageCodeFix : ClassicModelAssertUsageCodeFix
    {
        internal static readonly ImmutableDictionary<string, Constraints> CollectionAssertToParameterlessConstraints =
            new Dictionary<string, Constraints>
            {
                { NameOfCollectionAssertAllItemsAreNotNull, new Constraints(NameOfIs, NameOfIsAll, default(string), NameOfIsNot, NameOfIsNull) },
                { NameOfCollectionAssertAllItemsAreUnique, new Constraints(NameOfIs, NameOfIsUnique, default(string)) },
                { NameOfCollectionAssertIsEmpty, new Constraints(NameOfIs, default(string), default(string), NameOfIsEmpty) },
                { NameOfCollectionAssertIsNotEmpty, new Constraints(NameOfIs, NameOfIsNot, default(string), NameOfIsEmpty) },
                { NameOfCollectionAssertIsOrdered, new Constraints(NameOfIs, NameOfIsOrdered, default(string), default(string)) },
            }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<string, Constraints> CollectionAssertToOneSwappedParameterConstraints =
            new Dictionary<string, Constraints>
            {
                { NameOfCollectionAssertAreEqual, new Constraints(NameOfIs, default(string), NameOfIsEqualTo, NameOfEqualConstraintAsCollection) },
                { NameOfCollectionAssertAreEquivalent, new Constraints(NameOfIs, default(string), NameOfIsEquivalentTo) },
                { NameOfCollectionAssertAreNotEqual, new Constraints(NameOfIs, NameOfIsNot, NameOfIsEqualTo, NameOfEqualConstraintAsCollection) },
                { NameOfCollectionAssertAreNotEquivalent, new Constraints(NameOfIs, NameOfIsNot, NameOfIsEquivalentTo) },
            }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<string, Constraints> CollectionAssertToOneUnswappedParameterConstraints =
            new Dictionary<string, Constraints>
            {
                { NameOfCollectionAssertAllItemsAreInstancesOfType, new Constraints(NameOfIs, NameOfIsAll, NameOfIsInstanceOf) },
                { NameOfCollectionAssertContains, new Constraints(NameOfHas, default(string), NameOfHasMember) },
                { NameOfCollectionAssertDoesNotContain, new Constraints(NameOfHas, NameOfHasNo, NameOfHasMember) },
                { NameOfCollectionAssertIsNotSubsetOf, new Constraints(NameOfIs, NameOfIsNot, NameOfIsSubsetOf) },
                { NameOfCollectionAssertIsSubsetOf, new Constraints(NameOfIs, default(string), NameOfIsSubsetOf) },
                { NameOfCollectionAssertIsNotSupersetOf, new Constraints(NameOfIs, NameOfIsNot, NameOfIsSupersetOf) },
                { NameOfCollectionAssertIsSupersetOf, new Constraints(NameOfIs, default(string), NameOfIsSupersetOf) },
            }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<string, string> CollectionAssertToFirstParameterName =
            new Dictionary<string, string>()
            {
                { NameOfCollectionAssertAllItemsAreNotNull, NameOfCollectionParameter },
                { NameOfCollectionAssertAllItemsAreUnique, NameOfCollectionParameter },
                { NameOfCollectionAssertIsEmpty, NameOfCollectionParameter },
                { NameOfCollectionAssertIsNotEmpty, NameOfCollectionParameter },
                { NameOfCollectionAssertIsOrdered, NameOfCollectionParameter },
                { NameOfCollectionAssertAreEqual, NameOfExpectedParameter },
                { NameOfCollectionAssertAreEquivalent, NameOfExpectedParameter },
                { NameOfCollectionAssertAreNotEqual, NameOfExpectedParameter },
                { NameOfCollectionAssertAreNotEquivalent, NameOfExpectedParameter },
                { NameOfCollectionAssertAllItemsAreInstancesOfType, NameOfCollectionParameter },
                { NameOfCollectionAssertContains, NameOfCollectionParameter },
                { NameOfCollectionAssertDoesNotContain, NameOfCollectionParameter },
                { NameOfCollectionAssertIsNotSubsetOf, NameOfSubsetParameter },
                { NameOfCollectionAssertIsSubsetOf, NameOfSubsetParameter },
                { NameOfCollectionAssertIsNotSupersetOf, NameOfSupersetParameter },
                { NameOfCollectionAssertIsSupersetOf, NameOfSupersetParameter },
            }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<string, string> CollectionAssertToSecondParameterName =
            new Dictionary<string, string>()
            {
                { NameOfCollectionAssertAreEqual, NameOfActualParameter },
                { NameOfCollectionAssertAreEquivalent, NameOfActualParameter },
                { NameOfCollectionAssertAreNotEqual, NameOfActualParameter },
                { NameOfCollectionAssertAreNotEquivalent, NameOfActualParameter },
                { NameOfCollectionAssertAllItemsAreInstancesOfType, NameOfExpectedTypeParameter },
                { NameOfCollectionAssertContains, NameOfActualParameter },
                { NameOfCollectionAssertDoesNotContain, NameOfActualParameter },
                { NameOfCollectionAssertIsNotSubsetOf, NameOfSupersetParameter },
                { NameOfCollectionAssertIsSubsetOf, NameOfSupersetParameter },
                { NameOfCollectionAssertIsNotSupersetOf, NameOfSubsetParameter },
                { NameOfCollectionAssertIsSupersetOf, NameOfSubsetParameter },
            }.ToImmutableDictionary();

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.CollectionAssertUsage);

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var (actualArgument, constraintArgument) = GetActualAndConstraintArguments(diagnostic, argumentNamesToArguments);

            if (argumentNamesToArguments.TryGetValue(NameOfComparerParameter, out ArgumentSyntax? comparerArgument))
            {
                ExpressionSyntax expression = constraintArgument.Expression;

                // We need to drop the 'AsCollection' when using an IComparer.
                if (expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Name.ToString() == NameOfEqualConstraintAsCollection)
                {
                    expression = memberAccessExpression.Expression;
                }

                constraintArgument = Argument(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression,
                            IdentifierName(NameOfEqualConstraintUsing)),
                        ArgumentList(SingletonSeparatedList(comparerArgument))));
            }

            return (actualArgument, constraintArgument);
        }

        private static (ArgumentSyntax actualArgument, ArgumentSyntax constraintArgument) GetActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var methodName = diagnostic.Properties[AnalyzerPropertyKeys.ModelName]!;
            var firstParameterName = CollectionAssertToFirstParameterName[methodName];
            var firstArgument = argumentNamesToArguments[firstParameterName];

            if (CollectionAssertToParameterlessConstraints.TryGetValue(methodName, out Constraints? constraints))
            {
                var actualArgument = firstArgument.WithNameColon(null);
                var constraintArgument = Argument(constraints.CreateConstraint());
                return (actualArgument, constraintArgument);
            }
            else if (CollectionAssertToOneSwappedParameterConstraints.TryGetValue(methodName, out constraints))
            {
                var secondParameterName = CollectionAssertToSecondParameterName[methodName];
                var secondArgument = argumentNamesToArguments[secondParameterName];
                var actualArgument = secondArgument.WithNameColon(null);

                var constraintArgument = Argument(constraints.CreateConstraint(firstArgument.WithNameColon(null)));
                return (actualArgument, constraintArgument);
            }
            else if (CollectionAssertToOneUnswappedParameterConstraints.TryGetValue(methodName, out constraints))
            {
                var secondParameterName = CollectionAssertToSecondParameterName[methodName];
                var secondArgument = argumentNamesToArguments[secondParameterName];
                var constraintArgument = Argument(constraints.CreateConstraint(secondArgument.WithNameColon(null)));

                var actualArgument = firstArgument.WithNameColon(null);
                return (actualArgument, constraintArgument);
            }
            else
            {
                throw new InvalidOperationException($"Unknown method name: {methodName}");
            }
        }
    }
}
