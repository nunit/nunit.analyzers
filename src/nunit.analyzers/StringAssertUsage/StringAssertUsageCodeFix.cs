using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;

namespace NUnit.Analyzers.StringAssertUsage
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    internal class StringAssertUsageCodeFix : ClassicModelAssertUsageCodeFix
    {
        internal static readonly ImmutableDictionary<string, string> StringAssertToExpectedParameterName =
            new Dictionary<string, string>()
            {
                { NameOfStringAssertContains, NameOfExpectedParameter },
                { NameOfStringAssertDoesNotContain, NameOfExpectedParameter },
                { NameOfStringAssertStartsWith, NameOfExpectedParameter },
                { NameOfStringAssertDoesNotStartWith, NameOfExpectedParameter },
                { NameOfStringAssertEndsWith, NameOfExpectedParameter },
                { NameOfStringAssertDoesNotEndWith, NameOfExpectedParameter },
                { NameOfStringAssertAreEqualIgnoringCase, NameOfExpectedParameter },
                { NameOfStringAssertAreNotEqualIgnoringCase, NameOfExpectedParameter },
                { NameOfStringAssertIsMatch, NameOfPatternParameter },
                { NameOfStringAssertDoesNotMatch, NameOfPatternParameter },
            }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<string, Constraints> StringAssertToConstraints =
            new Dictionary<string, Constraints>
            {
                { NameOfStringAssertContains, new Constraints(NameOfDoes, default(string), NameOfDoesContain) },
                { NameOfStringAssertDoesNotContain, new Constraints(NameOfDoes, NameOfDoesNot, NameOfDoesContain) },
                { NameOfStringAssertStartsWith, new Constraints(NameOfDoes, default(string), NameOfDoesStartWith) },
                { NameOfStringAssertDoesNotStartWith, new Constraints(NameOfDoes, NameOfDoesNot, NameOfDoesStartWith) },
                { NameOfStringAssertEndsWith, new Constraints(NameOfDoes, default(string), NameOfDoesEndWith) },
                { NameOfStringAssertDoesNotEndWith, new Constraints(NameOfDoes, NameOfDoesNot, NameOfDoesEndWith) },
                { NameOfStringAssertAreEqualIgnoringCase, new Constraints(NameOfIs, default(string), NameOfIsEqualTo, NameOfEqualConstraintIgnoreCase) },
                { NameOfStringAssertAreNotEqualIgnoringCase, new Constraints(NameOfIs, NameOfIsNot, NameOfIsEqualTo, NameOfEqualConstraintIgnoreCase) },
                { NameOfStringAssertIsMatch, new Constraints(NameOfDoes, default(string), NameOfDoesMatch) },
                { NameOfStringAssertDoesNotMatch, new Constraints(NameOfDoes, NameOfDoesNot, NameOfDoesMatch) },
            }.ToImmutableDictionary();

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            AnalyzerIdentifiers.StringAssertUsage);

        protected override (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            var methodName = diagnostic.Properties[AnalyzerPropertyKeys.ModelName]!;
            var expectedParameterName = StringAssertToExpectedParameterName[methodName];
            var expectedArgument = argumentNamesToArguments[expectedParameterName].WithNameColon(null);
            var constraints = StringAssertToConstraints[methodName];
            var constraintArgument = Argument(constraints.CreateConstraint(expectedArgument));

            var actualArgument = argumentNamesToArguments[NameOfActualParameter].WithNameColon(null);
            return (actualArgument, constraintArgument);
        }
    }
}
