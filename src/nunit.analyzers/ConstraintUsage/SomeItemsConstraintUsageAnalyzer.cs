using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
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

        protected override (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData
            (SyntaxNodeAnalysisContext context, ExpressionSyntax actual, bool negated)
        {
            if (actual is InvocationExpressionSyntax invocationExpression
                && invocationExpression.ArgumentList.Arguments.Count == 1
                && invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(memberAccessExpression.Name).Symbol;

                if (this.IsCollectionContains(symbol) || this.IsLinqContains(symbol))
                {
                    var suggestedConstraint = negated ? DoesNotContain : DoesContain;
                    return (descriptor, suggestedConstraint);
                }
            }
            return (null, null);
        }

        private bool IsLinqContains(ISymbol? symbol)
        {
            return symbol is IMethodSymbol methodSymbol
               && methodSymbol.IsExtensionMethod
               && methodSymbol.Name == "Contains"
               && methodSymbol.ContainingType.GetFullMetadataName() == typeof(System.Linq.Enumerable).FullName;
        }

        private bool IsCollectionContains(ISymbol? symbol)
        {
            return symbol is IMethodSymbol methodSymbol
                && methodSymbol.Name == "Contains"
                && (methodSymbol.IsInterfaceImplementation(typeof(ICollection<>)!.FullName!)
                    || methodSymbol.IsInterfaceImplementation(typeof(ICollection)!.FullName!));
        }
    }
}
