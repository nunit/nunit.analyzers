using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.UpdateStringFormatSpecification
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UpdateStringFormatSpecificationAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor expectedResultTypeMismatch = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodExpectedResultTypeMismatchUsage,
            title: UpdateStringFormatSpecificationConstants.ExpectedResultTypeMismatchTitle,
            messageFormat: UpdateStringFormatSpecificationConstants.ExpectedResultTypeMismatchMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: UpdateStringFormatSpecificationConstants.ExpectedResultTypeMismatchDescription);

        private static readonly DiagnosticDescriptor specifiedExpectedResultForVoid = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodSpecifiedExpectedResultForVoidUsage,
            title: UpdateStringFormatSpecificationConstants.SpecifiedExpectedResultForVoidMethodTitle,
            messageFormat: UpdateStringFormatSpecificationConstants.SpecifiedExpectedResultForVoidMethodMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: UpdateStringFormatSpecificationConstants.SpecifiedExpectedResultForVoidMethodDescription);

        private static readonly DiagnosticDescriptor noExpectedResultButNonVoidReturnType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodNoExpectedResultButNonVoidReturnType,
            title: UpdateStringFormatSpecificationConstants.NoExpectedResultButNonVoidReturnTypeTitle,
            messageFormat: UpdateStringFormatSpecificationConstants.NoExpectedResultButNonVoidReturnTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: UpdateStringFormatSpecificationConstants.NoExpectedResultButNonVoidReturnTypeDescription);

        private static readonly DiagnosticDescriptor asyncNoExpectedResultAndVoidReturnType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndVoidReturnTypeUsage,
            title: UpdateStringFormatSpecificationConstants.AsyncNoExpectedResultAndVoidReturnTypeTitle,
            messageFormat: UpdateStringFormatSpecificationConstants.AsyncNoExpectedResultAndVoidReturnTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: UpdateStringFormatSpecificationConstants.AsyncNoExpectedResultAndVoidReturnTypeDescription);

        private static readonly DiagnosticDescriptor asyncNoExpectedResultAndNonTaskReturnType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodAsyncNoExpectedResultAndNonTaskReturnTypeUsage,
            title: UpdateStringFormatSpecificationConstants.AsyncNoExpectedResultAndNonTaskReturnTypeTitle,
            messageFormat: UpdateStringFormatSpecificationConstants.AsyncNoExpectedResultAndNonTaskReturnTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: UpdateStringFormatSpecificationConstants.AsyncNoExpectedResultAndNonTaskReturnTypeDescription);

        private static readonly DiagnosticDescriptor asyncExpectedResultButReturnTypeNotGenericTask =
            DiagnosticDescriptorCreator.Create(
                id: AnalyzerIdentifiers.TestMethodAsyncExpectedResultAndNonGenricTaskReturnTypeUsage,
                title: UpdateStringFormatSpecificationConstants.AsyncExpectedResultAndNonGenericTaskReturnTypeTitle,
                messageFormat: UpdateStringFormatSpecificationConstants.AsyncExpectedResultAndNonGenericTaskReturnTypeMessage,
                category: Categories.Structure,
                defaultSeverity: DiagnosticSeverity.Error,
                description: UpdateStringFormatSpecificationConstants.AsyncExpectedResultAndNonGenericTaskReturnTypeDescription);

        private static readonly DiagnosticDescriptor simpleTestHasParameters =
            DiagnosticDescriptorCreator.Create(
                id: AnalyzerIdentifiers.SimpleTestMethodHasParameters,
                title: UpdateStringFormatSpecificationConstants.SimpleTestMethodHasParametersTitle,
                messageFormat: UpdateStringFormatSpecificationConstants.SimpleTestMethodHasParametersMessage,
                category: Categories.Structure,
                defaultSeverity: DiagnosticSeverity.Error,
                description: UpdateStringFormatSpecificationConstants.SimpleTestMethodHasParametersDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(expectedResultTypeMismatch, specifiedExpectedResultForVoid, noExpectedResultButNonVoidReturnType,
                asyncNoExpectedResultAndVoidReturnType, asyncNoExpectedResultAndNonTaskReturnType,
                asyncExpectedResultButReturnTypeNotGenericTask,
                simpleTestHasParameters);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol? testCaseType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestCaseAttribute);
            INamedTypeSymbol? testType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestAttribute);

            if (testCaseType is null || testType is null)
                return;

            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, testCaseType, testType), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol testCaseType, INamedTypeSymbol testType)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

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
            if (attribute.NamedArguments.TryGetValue(NUnitFrameworkConstants.NameOfExpectedResult,
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

                    if (expectedResultLocation is not null)
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

                if (location is not null)
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
            return attributeData.GetNamedArgumentSyntax(NUnitFrameworkConstants.NameOfExpectedResult)?.GetLocation();
        }
    }
}
