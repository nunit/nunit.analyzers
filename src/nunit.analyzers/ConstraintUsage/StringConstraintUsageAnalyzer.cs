using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(containsDescriptor, startsWithDescriptor, endsWithDescriptor);

        private static readonly Dictionary<string, (string doesMethod, DiagnosticDescriptor descriptor)> SupportedMethods
            = new Dictionary<string, (string, DiagnosticDescriptor)>
            {
                [nameof(string.Contains)] = (NunitFrameworkConstants.NameOfDoesContain, containsDescriptor),
                [nameof(string.StartsWith)] = (NunitFrameworkConstants.NameOfDoesStartWith, startsWithDescriptor),
                [nameof(string.EndsWith)] = (NunitFrameworkConstants.NameOfDoesEndWith, endsWithDescriptor)
            };

        protected override (DiagnosticDescriptor? descriptor, string? suggestedConstraint) GetDiagnosticData(
            SyntaxNodeAnalysisContext context, ExpressionSyntax actual, bool negated)
        {
            if (actual is InvocationExpressionSyntax invocationExpression
                && invocationExpression.ArgumentList.Arguments.Count == 1
                && invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression
                && IsMethodSupported(memberAccessExpression.Name, context, out var methodName))
            {
                return GetDiagnosticData(methodName, negated);
            }

            return (null, null);
        }

        private static bool IsMethodSupported(SimpleNameSyntax nameSyntax, SyntaxNodeAnalysisContext context,
            [NotNullWhen(true)] out string? methodName)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(nameSyntax).Symbol;

            if (symbol is IMethodSymbol methodSymbol
                && methodSymbol.ContainingType.SpecialType == SpecialType.System_String
                && SupportedMethods.ContainsKey(methodSymbol.Name))
            {
                methodName = methodSymbol.Name;
                return true;
            }
            else
            {
                methodName = null;
                return false;
            }
        }

        private static (DiagnosticDescriptor, string) GetDiagnosticData(string stringMethod, bool negated)
        {
            var (doesMethod, descriptor) = SupportedMethods[stringMethod];

            var suggestedContstraint = negated
                ? $"{NunitFrameworkConstants.NameOfDoes}.{NunitFrameworkConstants.NameOfDoesNot}.{doesMethod}"
                : $"{NunitFrameworkConstants.NameOfDoes}.{doesMethod}";

            return (descriptor, suggestedContstraint);
        }
    }
}
