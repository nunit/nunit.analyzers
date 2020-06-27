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
        private static readonly DiagnosticDescriptor expectedResultTypeMismatch = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
            title: TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchTitle,
            messageFormat: TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchDescription);

        private static readonly DiagnosticDescriptor specifiedExpectedResultForVoid = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage,
            title: TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodTitle,
            messageFormat: TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodDescription);


        private static readonly DiagnosticDescriptor noExpectedResultButNonVoidReturnType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType,
            title: TestMethodUsageAnalyzerConstants.NoExpectedResultButNonVoidReturnTypeTitle,
            messageFormat: TestMethodUsageAnalyzerConstants.NoExpectedResultButNonVoidReturnTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestMethodUsageAnalyzerConstants.NoExpectedResultButNonVoidReturnTypeDescription);

        private static readonly DiagnosticDescriptor asyncNoExpectedResultAndVoidReturnType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage,
            title: TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndVoidReturnTypeTitle,
            messageFormat: TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndVoidReturnTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndVoidReturnTypeDescription);

        private static readonly DiagnosticDescriptor asyncNoExpectedResultAndNonTaskReturnType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage,
            title: TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndNonTaskReturnTypeTitle,
            messageFormat: TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndNonTaskReturnTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndNonTaskReturnTypeDescription);

        private static readonly DiagnosticDescriptor asyncExpectedResultButReturnTypeNotGenericTask =
            DiagnosticDescriptorCreator.Create(
                id: AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage,
                title: TestMethodUsageAnalyzerConstants.AsyncExpectedResultAndNonGenericTaskReturnTypeTitle,
                messageFormat: TestMethodUsageAnalyzerConstants.AsyncExpectedResultAndNonGenericTaskReturnTypeMessage,
                category: Categories.Structure,
                defaultSeverity: DiagnosticSeverity.Error,
                description: TestMethodUsageAnalyzerConstants.AsyncExpectedResultAndNonGenericTaskReturnTypeDescription);

        private static readonly DiagnosticDescriptor simpleTestHasParameters =
            DiagnosticDescriptorCreator.Create(
                id: AnalyzerIdentifiers.SimpleTestMethodHasParameters,
                title: TestMethodUsageAnalyzerConstants.SimpleTestMethodHasParametersTitle,
                messageFormat: TestMethodUsageAnalyzerConstants.SimpleTestMethodHasParametersMessage,
                category: Categories.Structure,
                defaultSeverity: DiagnosticSeverity.Error,
                description: TestMethodUsageAnalyzerConstants.SimpleTestMethodHasParametersDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(expectedResultTypeMismatch, specifiedExpectedResultForVoid, noExpectedResultButNonVoidReturnType,
                asyncNoExpectedResultAndVoidReturnType, asyncNoExpectedResultAndNonTaskReturnType,
                asyncExpectedResultButReturnTypeNotGenericTask,
                simpleTestHasParameters);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(
                TestMethodUsageAnalyzer.AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var declarationNode = GetMethod(context.Node);
            if (declarationNode is MethodDeclarationSyntax methodNode)
            {
                if (!methodNode.ContainsDiagnostics)
                {
                    var testCaseType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestCaseAttribute);
                    var testType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestAttribute);

                    if (testCaseType == null || testType == null)
                        return;

                    var attributeNode = (AttributeSyntax)context.Node;
                    var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;

                    var isTestCaseAttribute = IsAttribute(testCaseType, NunitFrameworkConstants.NameOfTestCaseAttribute, attributeSymbol);
                    var isTestAttribute = IsAttribute(testType, NunitFrameworkConstants.NameOfTestAttribute, attributeSymbol);

                    if (isTestCaseAttribute || (isTestAttribute && !HasITestBuilderAttribute(context.SemanticModel, methodNode.AttributeLists)))
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodNode);

                        context.CancellationToken.ThrowIfCancellationRequested();
                        TestMethodUsageAnalyzer.AnalyzeExpectedResult(context, attributeNode, methodSymbol);
                    }

                    var testMethodParameters = methodNode.ParameterList.Parameters.Count;
                    var hasISimpleTestBuilderAttribute = HasISimpleTestBuilderAttribute(context.SemanticModel, methodNode.AttributeLists);
                    var hasITestBuilderAttribute = HasITestBuilderAttribute(context.SemanticModel, methodNode.AttributeLists);
                    var parametersMarkedWithIParameterDataSourceAttribute = ParametersMarkedWithIParameterDataSourceAttribute(
                        context.SemanticModel, methodNode);

                    if (testMethodParameters > 0 &&
                        hasISimpleTestBuilderAttribute &&
                        !hasITestBuilderAttribute &&
                        parametersMarkedWithIParameterDataSourceAttribute < testMethodParameters)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                simpleTestHasParameters,
                                attributeNode.GetLocation(),
                                testMethodParameters,
                                parametersMarkedWithIParameterDataSourceAttribute));
                    }
                }
            }
        }

        private static SyntaxNode? GetMethod(SyntaxNode node)
        {
            foreach (var ancestor in node.Ancestors())
            {
                if (ancestor.IsKind(SyntaxKind.ParameterList))
                    return null;
                if (ancestor.IsKind(SyntaxKind.MethodDeclaration))
                    return ancestor;
            }
            return null;
        }

        private static bool IsAttribute(INamedTypeSymbol nunitType, string nunitTypeName, ISymbol attributeSymbol) =>
            nunitType.ContainingAssembly.Identity == attributeSymbol?.ContainingAssembly.Identity &&
            nunitTypeName == attributeSymbol?.ContainingType.Name;

        private static bool HasITestBuilderAttribute(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var allAttributes = attributeLists.SelectMany(al => al.Attributes);
            return allAttributes.Any(a => a.DerivesFromITestBuilder(semanticModel));
        }

        private static bool HasISimpleTestBuilderAttribute(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var allAttributes = attributeLists.SelectMany(al => al.Attributes);
            return allAttributes.Any(a => a.DerivesFromISimpleTestBuilder(semanticModel));
        }

        private static int ParametersMarkedWithIParameterDataSourceAttribute(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var parameters = methodDeclarationSyntax.ParameterList.Parameters;
            return parameters.Count(p => HasIParameterDataSourceAttribute(semanticModel, p.AttributeLists));
        }

        private static bool HasIParameterDataSourceAttribute(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var allAttributes = attributeLists.SelectMany(al => al.Attributes);
            return allAttributes.Any(a => a.DerivesFromIParameterDataSource(semanticModel));
        }

        private static void AnalyzeExpectedResult(SyntaxNodeAnalysisContext context,
            AttributeSyntax attributeNode, IMethodSymbol methodSymbol)
        {
            var (_, attributeNamedArguments) = attributeNode.GetArguments();

            var expectedResultNamedArgument = attributeNamedArguments.SingleOrDefault(
                _ => _.DescendantTokens().Any(__ => __.Text == NunitFrameworkConstants.NameOfExpectedResult));

            if (expectedResultNamedArgument != null)
            {
                ExpectedResultSupplied(context, methodSymbol, attributeNode, expectedResultNamedArgument);
            }
            else
            {
                NoExpectedResultSupplied(context, methodSymbol, attributeNode);
            }
        }

        private static void ExpectedResultSupplied(
            SyntaxNodeAnalysisContext context,
            IMethodSymbol methodSymbol,
            AttributeSyntax attributeNode,
            AttributeArgumentSyntax expectedResultNamedArgument)
        {
            var methodReturnValueType = methodSymbol.ReturnType;

            if (methodReturnValueType.IsAwaitable(out var awaitReturnType))
            {
                if (awaitReturnType.SpecialType == SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(asyncExpectedResultButReturnTypeNotGenericTask,
                        attributeNode.GetLocation(), methodReturnValueType.ToDisplayString()));
                }
                else
                {
                    ReportIfExpectedResultTypeCannotBeAssignedToReturnType(
                        ref context, expectedResultNamedArgument, awaitReturnType);
                }
            }
            else
            {
                if (methodReturnValueType.SpecialType == SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(specifiedExpectedResultForVoid,
                        expectedResultNamedArgument.GetLocation()));
                }
                else
                {
                    ReportIfExpectedResultTypeCannotBeAssignedToReturnType(
                        ref context, expectedResultNamedArgument, methodReturnValueType);
                }
            }
        }

        private static void ReportIfExpectedResultTypeCannotBeAssignedToReturnType(
            ref SyntaxNodeAnalysisContext context,
            AttributeArgumentSyntax expectedResultNamedArgument,
            ITypeSymbol typeSymbol)
        {
            if (typeSymbol.IsTypeParameterAndDeclaredOnMethod())
                return;

            if (!expectedResultNamedArgument.CanAssignTo(typeSymbol, context.SemanticModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(expectedResultTypeMismatch,
                    expectedResultNamedArgument.GetLocation(), typeSymbol.MetadataName));
            }
        }

        private static void NoExpectedResultSupplied(
            SyntaxNodeAnalysisContext context,
            IMethodSymbol methodSymbol,
            AttributeSyntax attributeNode)
        {
            var methodReturnValueType = methodSymbol.ReturnType;

            if (methodSymbol.IsAsync
                && methodReturnValueType.SpecialType == SpecialType.System_Void)
            {
                context.ReportDiagnostic(Diagnostic.Create(asyncNoExpectedResultAndVoidReturnType, attributeNode.GetLocation()));
            }
            else if (methodReturnValueType.IsAwaitable(out var awaitReturnType))
            {
                if (awaitReturnType.SpecialType != SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(asyncNoExpectedResultAndNonTaskReturnType,
                        attributeNode.GetLocation(), methodReturnValueType.ToDisplayString()));
                }
            }
            else
            {
                if (methodReturnValueType.SpecialType != SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(noExpectedResultButNonVoidReturnType,
                        attributeNode.GetLocation(), methodReturnValueType.ToDisplayString()));
                }
            }
        }
    }
}
