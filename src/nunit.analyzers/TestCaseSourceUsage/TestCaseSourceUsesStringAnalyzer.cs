using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.TestCaseSourceUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestCaseSourceUsesStringAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor CreateDescriptor(string message) =>
            new DiagnosticDescriptor(
                AnalyzerIdentifiers.TestCaseSourceStringUsage,
                TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantAnalyzerTitle,
                message,
                Categories.Usage,
                DiagnosticSeverity.Warning,
                true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            CreateDescriptor(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage));

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var testCaseSourceType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute);
            if (testCaseSourceType == null)
            {
                return;
            }

            var attributeNode = (AttributeSyntax)context.Node;
            var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;

            if (testCaseSourceType.ContainingAssembly.Identity == attributeSymbol?.ContainingAssembly.Identity &&
                NunitFrameworkConstants.NameOfTestCaseSourceAttribute == attributeSymbol?.ContainingType.Name)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (attributeNode.ArgumentList is AttributeArgumentListSyntax argumentList &&
                    argumentList.Arguments.Count == 1 &&
                    argumentList.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    if (HasMember(context, literal))
                    {
                        var stringConstant = literal.Token.ValueText;
                        context.ReportDiagnostic(Diagnostic.Create(
                            CreateDescriptor(string.Format(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage, stringConstant)),
                            literal.GetLocation()));
                    }
                }
            }
        }

        private static bool HasMember(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal)
        {
            if (!SyntaxFacts.IsValidIdentifier(literal.Token.ValueText))
            {
                return false;
            }


            foreach (var symbol in context.SemanticModel.LookupSymbols(literal.SpanStart, container: context.ContainingSymbol.ContainingType, name: literal.Token.ValueText))
            {
                switch (symbol.Kind)
                {
                    case SymbolKind.Field:
                    case SymbolKind.Property:
                        return true;
                }
            }

            return false;
        }
    }
}
