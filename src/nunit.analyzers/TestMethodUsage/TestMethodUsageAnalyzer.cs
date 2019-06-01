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
        private const string fullyQualifiedNameOfTask = "System.Threading.Tasks.Task";
        private const string fullyQualifiedNameOfGenericTask = "System.Threading.Tasks.Task`1";

        private static DiagnosticDescriptor CreateDescriptor(string id, string message) =>
            new DiagnosticDescriptor(id, TestMethodUsageAnalyzerConstants.Title,
                message, Categories.Structure, DiagnosticSeverity.Error, true);

        private static readonly DiagnosticDescriptor expectedResultTypeMismatch = TestMethodUsageAnalyzer.CreateDescriptor(
            AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
            TestMethodUsageAnalyzerConstants.ExpectedResultTypeMismatchMessage);

        private static readonly DiagnosticDescriptor specifiedExpectedResultForVoid = TestMethodUsageAnalyzer.CreateDescriptor(
            AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage,
            TestMethodUsageAnalyzerConstants.SpecifiedExpectedResultForVoidMethodMessage);

        private static readonly DiagnosticDescriptor noExpectedResultButNonVoidReturnType = TestMethodUsageAnalyzer.CreateDescriptor(
            AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType,
            TestMethodUsageAnalyzerConstants.NoExpectedResultButNonVoidReturnType);

        private static readonly DiagnosticDescriptor asyncNoExpectedResultAndVoidReturnType = TestMethodUsageAnalyzer.CreateDescriptor(
            AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage,
            TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndVoidReturnType);

        private static readonly DiagnosticDescriptor asyncNoExpectedResultAndNonTaskReturnType = TestMethodUsageAnalyzer.CreateDescriptor(
            AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage,
            TestMethodUsageAnalyzerConstants.AsyncNoExpectedResultAndNonTaskReturnType);

        private static readonly DiagnosticDescriptor asyncExpectedResultButReturnTypeNotGenericTask =
            TestMethodUsageAnalyzer.CreateDescriptor(
                AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage,
                TestMethodUsageAnalyzerConstants.AsyncExpectedResultAndNonGenericTaskReturnType2);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(expectedResultTypeMismatch, specifiedExpectedResultForVoid, noExpectedResultButNonVoidReturnType,
                asyncNoExpectedResultAndVoidReturnType, asyncNoExpectedResultAndNonTaskReturnType,
                asyncExpectedResultButReturnTypeNotGenericTask);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

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

            if (IsTestMethodAsync(context.Compilation, methodSymbol))
            {
                var genericTaskType = context.Compilation.GetTypeByMetadataName(fullyQualifiedNameOfGenericTask);
                if (!methodReturnValueType.OriginalDefinition.Equals(genericTaskType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(asyncExpectedResultButReturnTypeNotGenericTask,
                        attributeNode.GetLocation()));
                }
                else
                {
                    var namedTypeSymbol = methodReturnValueType as INamedTypeSymbol;
                    if (namedTypeSymbol == null)
                        return;

                    var taskTypeParameter = namedTypeSymbol.TypeArguments.First();
                    ReportIfExpectedResultTypeCannotBeAssignedToReturnType(
                        ref context, expectedResultNamedArgument, taskTypeParameter);
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

            if (IsTestMethodAsync(context.Compilation, methodSymbol))
            {
                if (methodReturnValueType.SpecialType == SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(asyncNoExpectedResultAndVoidReturnType, attributeNode.GetLocation()));
                }
                else
                {
                    var isTaskType = methodReturnValueType.Equals(context.Compilation.GetTypeByMetadataName(fullyQualifiedNameOfTask));
                    if (!isTaskType)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(asyncNoExpectedResultAndNonTaskReturnType, attributeNode.GetLocation()));
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

        private static bool IsTestMethodAsync(Compilation compilation, IMethodSymbol methodSymbol)
        {
            var methodReturnValueType = methodSymbol.ReturnType;
            return methodSymbol.IsAsync || IsTaskType(compilation, methodReturnValueType);
        }

        private static bool IsTaskType(Compilation compilation, ITypeSymbol typeSymbol)
        {
            var taskTypeSymbol = compilation.GetTypeByMetadataName(fullyQualifiedNameOfTask);

            for (; typeSymbol != null; typeSymbol = typeSymbol.BaseType)
            {
                if (typeSymbol.Equals(taskTypeSymbol))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
