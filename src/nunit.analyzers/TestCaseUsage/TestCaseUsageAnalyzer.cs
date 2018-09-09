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
    public sealed class TestCaseUsageAnalyzer
        : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor CreateDescriptor(string message) =>
            new DiagnosticDescriptor(AnalyzerIdentifiers.TestCaseUsage, TestCaseUsageAnalyzerConstants.Title,
                message, Categories.Usage, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            TestCaseUsageAnalyzer.CreateDescriptor(TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage),
            TestCaseUsageAnalyzer.CreateDescriptor(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage),
            TestCaseUsageAnalyzer.CreateDescriptor(TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage));

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                TestCaseUsageAnalyzer.AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var methodNode = context.Node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();

            if (methodNode != null)
            {
                if (!methodNode.ContainsDiagnostics)
                {
                    var testCaseType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestCaseAttribute);
                    if (testCaseType == null)
                        return;

                    var attributeNode = (AttributeSyntax)context.Node;
                    var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;

                    if (testCaseType.ContainingAssembly.Identity == attributeSymbol?.ContainingAssembly.Identity &&
                        NunitFrameworkConstants.NameOfTestCaseAttribute == attributeSymbol?.ContainingType.Name)
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodNode);
                        var methodParameters = methodSymbol.GetParameterCounts();
                        var methodRequiredParameters = methodParameters.Item1;
                        var methodOptionalParameters = methodParameters.Item2;
                        var methodParamsParameters = methodParameters.Item3;

                        var attributePositionalAndNamedArguments = attributeNode.GetArguments();
                        var attributePositionalArguments = attributePositionalAndNamedArguments.Item1;
                        var attributeNamedArguments = attributePositionalAndNamedArguments.Item2;

                        if (attributePositionalArguments.Length < methodRequiredParameters)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                TestCaseUsageAnalyzer.CreateDescriptor(
                                    TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage),
                                attributeNode.GetLocation()));
                        }
                        else if (methodParamsParameters == 0 &&
                            attributePositionalArguments.Length > methodRequiredParameters + methodOptionalParameters)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                TestCaseUsageAnalyzer.CreateDescriptor(
                                    TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage),
                                attributeNode.GetLocation()));
                        }
                        else
                        {
                            context.CancellationToken.ThrowIfCancellationRequested();
                            TestCaseUsageAnalyzer.AnalyzePositionalArgumentsAndParameters(context,
                                attributePositionalArguments, methodSymbol.Parameters);
                        }
                    }
                }
            }
        }

        private static Tuple<ITypeSymbol, string> GetParameterType(ImmutableArray<IParameterSymbol> methodParameter,
            int position)
        {
            var symbol = position >= methodParameter.Length ?
                methodParameter[methodParameter.Length - 1] : methodParameter[position];

            ITypeSymbol type;

            if (symbol.IsParams)
            {
                type = ((IArrayTypeSymbol)symbol.Type).ElementType;
            }
            else
            {
                type = symbol.Type;
            }

            return new Tuple<ITypeSymbol, string>(type, symbol.Name);
        }

        private static void AnalyzePositionalArgumentsAndParameters(SyntaxNodeAnalysisContext context,
            ImmutableArray<AttributeArgumentSyntax> attributePositionalArguments,
            ImmutableArray<IParameterSymbol> methodParameters)
        {
            var model = context.SemanticModel;

            for (var i = 0; i < attributePositionalArguments.Length; i++)
            {
                var attributeArgument = attributePositionalArguments[i];
                var methodParametersSymbol = TestCaseUsageAnalyzer.GetParameterType(methodParameters, i);
                var methodParameterType = methodParametersSymbol.Item1;
                var methodParameterName = methodParametersSymbol.Item2;

                if (!attributeArgument.CanAssignTo(methodParameterType, model))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                      TestCaseUsageAnalyzer.CreateDescriptor(
                          string.Format(TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                              i, methodParameterName)),
                      attributeArgument.GetLocation()));
                }

                context.CancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
