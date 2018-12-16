using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.TestCaseUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestMethodUsageAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor CreateDescriptor(string id, string message) =>
            new DiagnosticDescriptor(id, TestMethodUsageAnalyzerConstants.Title,
                message, Categories.Usage, DiagnosticSeverity.Error, true);

        private static readonly DiagnosticDescriptor expectedResultTypeMismatch = TestMethodUsageAnalyzer.CreateDescriptor(
                AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
                TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage);

        private static readonly DiagnosticDescriptor specifiedExpectedResultForVoid = TestMethodUsageAnalyzer.CreateDescriptor(
                AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage,
                TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(expectedResultTypeMismatch, specifiedExpectedResultForVoid);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                TestMethodUsageAnalyzer.AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var methodNode = context.Node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();

            if (methodNode != null)
            {
                if (!methodNode.ContainsDiagnostics)
                {
                    var testCaseType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestCaseAttribute);
                    var testType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestAttribute);

                    if (testCaseType == null && testType == null)
                        return;

                    var attributeNode = (AttributeSyntax)context.Node;
                    var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;

                    var isTestCaseAttribute = IsAttribute(testCaseType, NunitFrameworkConstants.NameOfTestCaseAttribute, attributeSymbol);
                    var isTestAttribute = IsAttribute(testType, NunitFrameworkConstants.NameOfTestAttribute, attributeSymbol);

                    if (isTestCaseAttribute || isTestAttribute)
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodNode);
                        var attributePositionalAndNamedArguments = attributeNode.GetArguments();
                        var attributeNamedArguments = attributePositionalAndNamedArguments.Item2;

                        context.CancellationToken.ThrowIfCancellationRequested();
                        TestMethodUsageAnalyzer.AnalyzeExpectedResult(context,
                            attributeNamedArguments, methodSymbol);
                    }
                }
            }
        }

        private static bool IsAttribute(INamedTypeSymbol nunitType, string nunitTypeName, ISymbol attributeSymbol) =>
            nunitType.ContainingAssembly.Identity == attributeSymbol?.ContainingAssembly.Identity &&
            nunitTypeName == attributeSymbol?.ContainingType.Name;

        private static void AnalyzeExpectedResult(SyntaxNodeAnalysisContext context,
            ImmutableArray<AttributeArgumentSyntax> attributeNamedArguments, IMethodSymbol methodSymbol)
        {
            var expectedResultNamedArgument = attributeNamedArguments.SingleOrDefault(
                _ => _.DescendantTokens().Any(__ => __.Text == NunitFrameworkConstants.NameOfExpectedResult));

            if (expectedResultNamedArgument != null)
            {
                var methodReturnValueType = methodSymbol.ReturnType;

                if (methodReturnValueType.SpecialType == SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(specifiedExpectedResultForVoid,
                        expectedResultNamedArgument.GetLocation()));
                }
                else
                {
                    if (!expectedResultNamedArgument.CanAssignTo(methodReturnValueType, context.SemanticModel))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(expectedResultTypeMismatch,
                            expectedResultNamedArgument.GetLocation(), methodReturnValueType.MetadataName));
                    }
                }
            }
        }
    }
}
