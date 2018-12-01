using System.Collections.Generic;
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

                var arguments = attributeNode.ArgumentList.Arguments;

                // If the First Argument is a String Constant we're in trouble
                var firstArgument = arguments.FirstOrDefault().Expression;
                var firstArgumentIsStringConstant = firstArgument.Kind() == SyntaxKind.StringLiteralExpression;

                if (firstArgumentIsStringConstant)
                {
                    var stringConstant = ((LiteralExpressionSyntax)firstArgument).Token.ValueText;
                    context.ReportDiagnostic(Diagnostic.Create(
                        CreateDescriptor( string.Format(TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage, stringConstant)),
                        attributeNode.GetLocation(),
                        ImmutableDictionary.Create<string,string>().Add("StringConstant", stringConstant)
                        ));
                }
            }
        }
    }
}
