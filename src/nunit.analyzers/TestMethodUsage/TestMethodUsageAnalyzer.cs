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

        private static readonly DiagnosticDescriptor noExpectedResultButNonVoidReturnType = TestMethodUsageAnalyzer.CreateDescriptor(
            AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType,
            TestMethodUsageAnalyzerConstants.NoExpectedResultButNonVoidReturnType);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(expectedResultTypeMismatch, specifiedExpectedResultForVoid, noExpectedResultButNonVoidReturnType);

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

                        context.CancellationToken.ThrowIfCancellationRequested();
                        TestMethodUsageAnalyzer.AnalyzeExpectedResult(context, attributeNode, methodSymbol);
                    }
                }
            }
        }

        private static bool IsAttribute(INamedTypeSymbol nunitType, string nunitTypeName, ISymbol attributeSymbol) =>
            nunitType.ContainingAssembly.Identity == attributeSymbol?.ContainingAssembly.Identity &&
            nunitTypeName == attributeSymbol?.ContainingType.Name;

        private static void AnalyzeExpectedResult(SyntaxNodeAnalysisContext context,
            AttributeSyntax attributeNode, IMethodSymbol methodSymbol)
        {
            var attributePositionalAndNamedArguments = attributeNode.GetArguments();
            var attributeNamedArguments = attributePositionalAndNamedArguments.Item2;

            var methodReturnValueType = methodSymbol.ReturnType;

            var expectedResultNamedArgument = attributeNamedArguments.SingleOrDefault(
                _ => _.DescendantTokens().Any(__ => __.Text == NunitFrameworkConstants.NameOfExpectedResult));

            if (expectedResultNamedArgument != null)
            {
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
            else
            {
                if (methodReturnValueType.SpecialType != SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(noExpectedResultButNonVoidReturnType,
                        attributeNode.GetLocation()));
                }
            }
        }
    }
}
