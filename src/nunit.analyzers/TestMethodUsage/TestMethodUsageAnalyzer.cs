using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.TestMethodUsage
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
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            var testCaseType = context.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestCaseAttribute);
            var testType = context.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestAttribute);

            if (testCaseType == null || testType == null)
                return;

            var methodAttributes = methodSymbol.GetAttributes();

            foreach (var attribute in methodAttributes)
            {
                if (attribute.AttributeClass is null)
                    continue;

                var isTestCaseAttribute = SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, testCaseType);
                var isTestAttribute = SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, testType);

                if (isTestCaseAttribute
                    || (isTestAttribute && !HasITestBuilderAttribute(context.Compilation, methodAttributes)))
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    AnalyzeExpectedResult(context, attribute, methodSymbol);
                }

                var isSimpleTestBulderAttribute = attribute.DerivesFromISimpleTestBuilder(context.Compilation);

                if (isSimpleTestBulderAttribute)
                {
                    var parameters = methodSymbol.Parameters;
                    var testMethodParameters = parameters.Length;
                    var hasITestBuilderAttribute = HasITestBuilderAttribute(context.Compilation, methodAttributes);
                    var parametersMarkedWithIParameterDataSourceAttribute =
                        parameters.Count(p => HasIParameterDataSourceAttribute(context.Compilation, p.GetAttributes()));

                    if (testMethodParameters > 0 &&
                        !hasITestBuilderAttribute &&
                        parametersMarkedWithIParameterDataSourceAttribute < testMethodParameters)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            simpleTestHasParameters,
                            attribute.ApplicationSyntaxReference.GetLocation(),
                            testMethodParameters,
                            parametersMarkedWithIParameterDataSourceAttribute));
                    }
                }
            }
        }

        private static bool HasITestBuilderAttribute(Compilation compilation, ImmutableArray<AttributeData> attributes)
        {
            return attributes.Any(a => a.DerivesFromITestBuilder(compilation));
        }

        private static bool HasIParameterDataSourceAttribute(Compilation compilation, ImmutableArray<AttributeData> attributes)
        {
            return attributes.Any(a => a.DerivesFromIParameterDataSource(compilation));
        }

        private static void AnalyzeExpectedResult(SymbolAnalysisContext context,
            AttributeData attribute, IMethodSymbol methodSymbol)
        {
            if (attribute.NamedArguments.TryGetValue(NunitFrameworkConstants.NameOfExpectedResult,
                out var expectedResultNamedArgument))
            {
                ExpectedResultSupplied(context, methodSymbol, attribute, expectedResultNamedArgument);
            }
            else
            {
                NoExpectedResultSupplied(context, methodSymbol, attribute);
            }
        }

        private static void ExpectedResultSupplied(
            SymbolAnalysisContext context,
            IMethodSymbol methodSymbol,
            AttributeData attributeData,
            TypedConstant expectedResultNamedArgument)
        {
            var methodReturnValueType = methodSymbol.ReturnType;

            if (methodReturnValueType.IsAwaitable(out var awaitReturnType))
            {
                if (awaitReturnType.SpecialType == SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        asyncExpectedResultButReturnTypeNotGenericTask,
                        attributeData.ApplicationSyntaxReference.GetLocation(),
                        methodReturnValueType.ToDisplayString()));
                }
                else
                {
                    ReportIfExpectedResultTypeCannotBeAssignedToReturnType(
                        ref context, attributeData, expectedResultNamedArgument, awaitReturnType);
                }
            }
            else
            {
                if (methodReturnValueType.SpecialType == SpecialType.System_Void)
                {
                    var expectedResultLocation = GetExpectedArgumentLocation(attributeData);

                    if (expectedResultLocation != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            specifiedExpectedResultForVoid,
                            expectedResultLocation));
                    }
                }
                else
                {
                    ReportIfExpectedResultTypeCannotBeAssignedToReturnType(
                        ref context, attributeData, expectedResultNamedArgument, methodReturnValueType);
                }
            }
        }

        private static void ReportIfExpectedResultTypeCannotBeAssignedToReturnType(
            ref SymbolAnalysisContext context,
            AttributeData attributeData,
            TypedConstant expectedResultNamedArgument,
            ITypeSymbol typeSymbol)
        {
            if (typeSymbol.IsTypeParameterAndDeclaredOnMethod())
                return;

            if (!expectedResultNamedArgument.CanAssignTo(typeSymbol, context.Compilation))
            {
                var location = GetExpectedArgumentLocation(attributeData);

                if (location != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        expectedResultTypeMismatch,
                        location,
                        typeSymbol.MetadataName));
                }
            }
        }

        private static void NoExpectedResultSupplied(
            SymbolAnalysisContext context,
            IMethodSymbol methodSymbol,
            AttributeData attributeData)
        {
            var methodReturnValueType = methodSymbol.ReturnType;

            if (methodSymbol.IsAsync
                && methodReturnValueType.SpecialType == SpecialType.System_Void)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    asyncNoExpectedResultAndVoidReturnType,
                    attributeData.ApplicationSyntaxReference.GetLocation()));
            }
            else if (methodReturnValueType.IsAwaitable(out var awaitReturnType))
            {
                if (awaitReturnType.SpecialType != SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        asyncNoExpectedResultAndNonTaskReturnType,
                        attributeData.ApplicationSyntaxReference.GetLocation(),
                        methodReturnValueType.ToDisplayString()));
                }
            }
            else
            {
                if (methodReturnValueType.SpecialType != SpecialType.System_Void)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        noExpectedResultButNonVoidReturnType,
                        attributeData.ApplicationSyntaxReference.GetLocation(),
                        methodReturnValueType.ToDisplayString()));
                }
            }
        }

        private static Location? GetExpectedArgumentLocation(AttributeData attributeData)
        {
            return attributeData.GetNamedArgumentSyntax(NunitFrameworkConstants.NameOfExpectedResult)?.GetLocation();
        }
    }
}
