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
        private static readonly DiagnosticDescriptor MissingSourceDescriptor = new DiagnosticDescriptor(
            AnalyzerIdentifiers.TestCaseSourceIsMissing,
            "TestCaseSource argument does not specify an existing member.",
            "TestCaseSource argument does not specify an existing member.",
            Categories.Structure,
            DiagnosticSeverity.Error,
            true);

        private static DiagnosticDescriptor CreateDescriptor(string message) =>
            new DiagnosticDescriptor(
                AnalyzerIdentifiers.TestCaseSourceStringUsage,
                TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantAnalyzerTitle,
                message,
                Categories.Structure,
                DiagnosticSeverity.Warning,
                true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            CreateDescriptor(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage),
            MissingSourceDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(x => AnalyzeAttribute(x), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
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
                    argumentList.Arguments[0]?.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    if (HasMember(context, literal))
                    {
                        var stringConstant = literal.Token.ValueText;
                        context.ReportDiagnostic(Diagnostic.Create(
                            CreateDescriptor(string.Format(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage, stringConstant)),
                            literal.GetLocation()));
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(MissingSourceDescriptor, literal.GetLocation()));
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
                    case SymbolKind.Method:
                        return true;
                }
            }

            return false;
        }
    }
}
