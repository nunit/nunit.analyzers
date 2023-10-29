using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;

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
            var methodSymbol = assertOperation.TargetMethod;

            if (StringAssertToConstraint.TryGetValue(methodSymbol.Name, out string? constraint))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    stringAssertDescriptor,
                    assertOperation.Syntax.GetLocation(),
                    new Dictionary<string, string?>
                    {
                        [AnalyzerPropertyKeys.ModelName] = methodSymbol.Name,
                    }.ToImmutableDictionary(),
                    constraint,
                    methodSymbol.Name));
            }
        }
    }
}
