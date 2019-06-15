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
        private static readonly DiagnosticDescriptor missingSourceDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceIsMissing,
            title: "TestCaseSource argument does not specify an existing member.",
            messageFormat: "TestCaseSource argument '{0}' does not specify an existing member.",
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: "TestCaseSource argument does not specify an existing member. This will lead to an error at run-time.");

        private static readonly DiagnosticDescriptor considerNameOfDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceStringUsage,
            title: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantAnalyzerTitle,
            messageFormat: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            considerNameOfDescriptor,
            missingSourceDescriptor);

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
                            considerNameOfDescriptor,
                            literal.GetLocation(),
                            stringConstant));
                    }
                    else
                    {
                        var literalText = literal.Token.ValueText;
                        context.ReportDiagnostic(Diagnostic.Create(missingSourceDescriptor, literal.GetLocation(), literalText));
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
