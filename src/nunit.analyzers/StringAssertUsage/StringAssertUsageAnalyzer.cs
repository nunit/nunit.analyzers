using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;
using static NUnit.Analyzers.Constants.NUnitLegacyFrameworkConstants;

namespace NUnit.Analyzers.StringAssertUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StringAssertUsageAnalyzer : BaseAssertionAnalyzer
    {
        internal static readonly ImmutableDictionary<string, string> StringAssertToConstraint =
            new Dictionary<string, string>
            {
                { NameOfStringAssertContains, $"{NameOfDoes}.{NameOfDoesContain}(expected)" },
                { NameOfStringAssertDoesNotContain, $"{NameOfDoes}.{NameOfDoesNot}.{NameOfDoesContain}(expected)" },
                { NameOfStringAssertStartsWith, $"{NameOfDoes}.{NameOfDoesStartWith}(expected)" },
                { NameOfStringAssertDoesNotStartWith, $"{NameOfDoes}.{NameOfDoesNot}.{NameOfDoesStartWith}(expected)" },
                { NameOfStringAssertEndsWith, $"{NameOfDoes}.{NameOfDoesEndWith}(expected)" },
                { NameOfStringAssertDoesNotEndWith, $"{NameOfDoes}.{NameOfDoesNot}.{NameOfDoesEndWith}(expected)" },
                { NameOfStringAssertAreEqualIgnoringCase, $"{NameOfIs}.{NameOfIsEqualTo}(expected).{NameOfEqualConstraintIgnoreCase}" },
                { NameOfStringAssertAreNotEqualIgnoringCase, $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEqualTo}(expected).{NameOfEqualConstraintIgnoreCase}" },
                { NameOfStringAssertIsMatch, $"{NameOfDoes}.{NameOfDoesMatch}(expected)" },
                { NameOfStringAssertDoesNotMatch, $"{NameOfDoes}.{NameOfDoesNot}.{NameOfDoesMatch}(expected)" },
            }.ToImmutableDictionary();

        private static readonly DiagnosticDescriptor stringAssertDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.StringAssertUsage,
            title: StringAssertUsageConstants.StringAssertTitle,
            messageFormat: StringAssertUsageConstants.StringAssertMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: StringAssertUsageConstants.StringAssertDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(stringAssertDescriptor);

        protected override bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation)
        {
            return invocationOperation.TargetMethod.ContainingType.IsStringAssert();
        }

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            string methodName = assertOperation.TargetMethod.Name;

            if (StringAssertToConstraint.TryGetValue(methodName, out string? constraint))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    stringAssertDescriptor,
                    assertOperation.Syntax.GetLocation(),
                    DiagnosticsHelper.GetProperties(methodName, assertOperation.Arguments),
                    constraint,
                    methodName));
            }
        }
    }
}
