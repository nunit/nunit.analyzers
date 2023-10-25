using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq.Expressions;
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

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.CollectionAssertUsage);

        protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
        {
            string methodName = diagnostic.Properties[AnalyzerPropertyKeys.ModelName]!;

            int comparerParameterIndex = diagnostic.Properties[AnalyzerPropertyKeys.ComparerParameterIndex] switch
            {
                "2" => 2,
                "1" => 1,
                _ => 0,
            };

            ArgumentSyntax? comparerArgument = null;
            if (comparerParameterIndex > 0)
            {
                // Remember 'comparer' parameter to be added as an 'Using' suffix.
                comparerArgument = arguments[comparerParameterIndex];
                arguments.RemoveAt(comparerParameterIndex);
            }

            if (CollectionAssertToParameterlessConstraints.TryGetValue(methodName, out Constraints? constraints))
            {
                arguments.Insert(1, Argument(constraints.CreateConstraint()));
            }
            else if (CollectionAssertToOneSwappedParameterConstraints.TryGetValue(methodName, out constraints))
            {
                arguments.Insert(2, Argument(constraints.CreateConstraint(arguments[0])));

                // Then we have to remove the 1st argument because that's now in the "constaint"
                arguments.RemoveAt(0);
            }
            else if (CollectionAssertToOneUnswappedParameterConstraints.TryGetValue(methodName, out constraints))
            {
                arguments[1] = Argument(constraints.CreateConstraint(arguments[1]));
            }

            if (comparerArgument is not null)
            {
                ExpressionSyntax expression = arguments[1].Expression;

                // We need to drop the 'AsCollection' when using an IComparer.
                if (expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Name.ToString() == NameOfEqualConstraintAsCollection)
                {
                    expression = memberAccessExpression.Expression;
                }

                arguments[1] = Argument(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            expression,
                            IdentifierName(NameOfEqualConstraintUsing)),
                        ArgumentList(SingletonSeparatedList(comparerArgument))));
            }
        }
    }
}
