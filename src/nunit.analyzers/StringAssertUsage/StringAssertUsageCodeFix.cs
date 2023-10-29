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

        protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
        {
            string methodName = diagnostic.Properties[AnalyzerPropertyKeys.ModelName]!;
            if (StringAssertToConstraints.TryGetValue(methodName, out Constraints? constraints))
            {
                arguments.Insert(2, Argument(constraints.CreateConstraint(arguments[0])));

                // Then we have to remove the 1st argument because that's now in the "constaint"
                arguments.RemoveAt(0);
            }
        }
    }
}
