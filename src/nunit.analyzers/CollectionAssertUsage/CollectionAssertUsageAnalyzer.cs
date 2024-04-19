using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using static NUnit.Analyzers.Constants.NUnitFrameworkConstants;

namespace NUnit.Analyzers.CollectionAssertUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CollectionAssertUsageAnalyzer : BaseAssertionAnalyzer
    {
        internal static readonly ImmutableDictionary<string, string> OneCollectionParameterAsserts =
            new Dictionary<string, string>
            {
                { NameOfCollectionAssertAllItemsAreNotNull, $"{NameOfIs}.{NameOfIsAll}.{NameOfIsNot}.{NameOfIsNull}" },
                { NameOfCollectionAssertAllItemsAreUnique, $"{NameOfIs}.{NameOfIsUnique}" },
                { NameOfCollectionAssertIsEmpty, $"{NameOfIs}.{NameOfIsEmpty}" },
                { NameOfCollectionAssertIsNotEmpty, $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEmpty}" },
                { NameOfCollectionAssertIsOrdered, $"{NameOfIs}.{NameOfIsOrdered}" },
            }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<string, string> TwoCollectionParameterAsserts =
            new Dictionary<string, string>
            {
                { NameOfCollectionAssertAreEqual, $"{NameOfIs}.{NameOfIsEqualTo}(expected).{NameOfEqualConstraintAsCollection}" },
                { NameOfCollectionAssertAreEquivalent, $"{NameOfIs}.{NameOfIsEquivalentTo}(expected)" },
                { NameOfCollectionAssertAreNotEqual, $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEqualTo}(expected).{NameOfEqualConstraintAsCollection}" },
                { NameOfCollectionAssertAreNotEquivalent, $"{NameOfIs}.{NameOfIsNot}.{NameOfIsEquivalentTo}(expected)" },
                { NameOfCollectionAssertIsNotSubsetOf, $"{NameOfIs}.{NameOfIsNot}.{NameOfIsSubsetOf}(expected)" },
                { NameOfCollectionAssertIsSubsetOf, $"{NameOfIs}.{NameOfIsSubsetOf}(expected)" },
                { NameOfCollectionAssertIsNotSupersetOf, $"{NameOfIs}.{NameOfIsNot}.{NameOfIsSupersetOf}(expected)" },
                { NameOfCollectionAssertIsSupersetOf, $"{NameOfIs}.{NameOfIsSupersetOf}(expected)" },
            }.ToImmutableDictionary();

        internal static readonly ImmutableDictionary<string, string> CollectionAndItemParameterAsserts =
            new Dictionary<string, string>
            {
                { NameOfCollectionAssertAllItemsAreInstancesOfType, $"{NameOfIs}.{NameOfIsAll}.{NameOfIsInstanceOf}(expected)" },
                { NameOfCollectionAssertContains, $"{NameOfHas}.{NameOfHasMember}(expected)" },
                { NameOfCollectionAssertDoesNotContain, $"{NameOfHas}.{NameOfHasNo}.{NameOfHasMember}(expected)" },
            }.ToImmutableDictionary();

        private static readonly DiagnosticDescriptor collectionAssertDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.CollectionAssertUsage,
            title: CollectionAssertUsageConstants.CollectionAssertTitle,
            messageFormat: CollectionAssertUsageConstants.CollectionAssertMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: CollectionAssertUsageConstants.CollectionAssertDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(collectionAssertDescriptor);

        protected override bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation)
        {
            return invocationOperation.TargetMethod.ContainingType.IsCollectionAssert();
        }

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            string methodName = assertOperation.TargetMethod.Name;

            if (OneCollectionParameterAsserts.TryGetValue(methodName, out string? constraint) ||
                TwoCollectionParameterAsserts.TryGetValue(methodName, out constraint) ||
                CollectionAndItemParameterAsserts.TryGetValue(methodName, out constraint))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    collectionAssertDescriptor,
                    assertOperation.Syntax.GetLocation(),
                    DiagnosticsHelper.GetProperties(methodName, assertOperation.Arguments),
                    constraint,
                    methodName));
            }
        }
    }
}
