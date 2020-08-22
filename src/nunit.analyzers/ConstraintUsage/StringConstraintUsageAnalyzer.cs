using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringConstraintUsageAnalyzer : BaseConditionConstraintAnalyzer
    {
        #region Descriptors

        private static readonly DiagnosticDescriptor containsDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.StringContainsConstraintUsage,
            title: StringConstraintUsageConstants.ContainsTitle,
            messageFormat: StringConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: StringConstraintUsageConstants.Description);

        private static readonly DiagnosticDescriptor startsWithDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.StringStartsWithConstraintUsage,
            title: StringConstraintUsageConstants.StartsWithTitle,
            messageFormat: StringConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: StringConstraintUsageConstants.Description);

        private static readonly DiagnosticDescriptor endsWithDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.StringEndsWithConstraintUsage,
            title: StringConstraintUsageConstants.EndsWithTitle,
            messageFormat: StringConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: StringConstraintUsageConstants.Description);

        #endregion Descriptors

        private static readonly Dictionary<string, (string doesMethod, DiagnosticDescriptor descriptor)> SupportedMethods
            = new Dictionary<string, (string, DiagnosticDescriptor)>
            {
                [nameof(string.Contains)] = (NunitFrameworkConstants.NameOfDoesContain, containsDescriptor),
                [nameof(string.StartsWith)] = (NunitFrameworkConstants.NameOfDoesStartWith, startsWithDescriptor),
                [nameof(string.EndsWith)] = (NunitFrameworkConstants.NameOfDoesEndWith, endsWithDescriptor)
            };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(containsDescriptor, startsWithDescriptor, endsWithDescriptor);

        protected override (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData(
            OperationAnalysisContext context, IOperation actual, bool negated)
        {
            if (actual is IInvocationOperation invocation
                && invocation.Arguments.Length == 1
                && invocation.TargetMethod.ContainingType.SpecialType == SpecialType.System_String
                && invocation.TargetMethod.Name is var methodName
                && SupportedMethods.ContainsKey(methodName))
            {
                return GetDiagnosticData(methodName, negated);
            }

            return (null, null);
        }

        private static (DiagnosticDescriptor descriptor, string suggestedConstraint) GetDiagnosticData(string stringMethod, bool negated)
        {
            var (doesMethod, descriptor) = SupportedMethods[stringMethod];

            var suggestedContstraint = negated
                ? $"{NunitFrameworkConstants.NameOfDoes}.{NunitFrameworkConstants.NameOfDoesNot}.{doesMethod}"
                : $"{NunitFrameworkConstants.NameOfDoes}.{doesMethod}";

            return (descriptor, suggestedContstraint);
        }
    }
}
