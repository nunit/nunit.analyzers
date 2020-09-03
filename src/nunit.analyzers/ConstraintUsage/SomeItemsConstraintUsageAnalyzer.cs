using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SomeItemsConstraintUsageAnalyzer : BaseConditionConstraintAnalyzer
    {
        private static readonly string DoesContain = $"{NameOfDoes}.{NameOfDoesContain}";
        private static readonly string DoesNotContain = $"{NameOfDoes}.{NameOfDoesNot}.{NameOfDoesContain}";

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.CollectionContainsConstraintUsage,
            title: SomeItemsConstraintUsageConstants.Title,
            messageFormat: SomeItemsConstraintUsageConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: SomeItemsConstraintUsageConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData(
            OperationAnalysisContext context, IOperation actual, bool negated)
        {
            if (actual is IInvocationOperation invocationOperation)
            {
                var symbol = invocationOperation.TargetMethod;
                var argumentCount = invocationOperation.Arguments.Length;

                if ((argumentCount == 1 && IsCollectionContains(symbol))
                    || (argumentCount == 2 && IsLinqContains(symbol)))
                {
                    var suggestedConstraint = negated ? DoesNotContain : DoesContain;
                    return (descriptor, suggestedConstraint);
                }
            }

            return (null, null);
        }

        private static bool IsLinqContains(IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsExtensionMethod
               && methodSymbol.Name == "Contains"
               && methodSymbol.ContainingType.GetFullMetadataName() == typeof(System.Linq.Enumerable).FullName;
        }

        private static bool IsCollectionContains(IMethodSymbol methodSymbol)
        {
            return methodSymbol.Name == "Contains"
                && (methodSymbol.IsInterfaceImplementation(typeof(ICollection<>).FullName!)
                    || methodSymbol.IsInterfaceImplementation(typeof(ICollection).FullName!));
        }
    }
}
