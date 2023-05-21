using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.SourceCommon;

namespace NUnit.Analyzers.TestCaseSourceUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestCaseSourceUsesStringAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor missingSourceDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceIsMissing,
            title: "The TestCaseSource argument does not specify an existing member",
            messageFormat: "The TestCaseSource argument '{0}' does not specify an existing member",
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: "The TestCaseSource argument does not specify an existing member. This will lead to an error at run-time.");

        private static readonly DiagnosticDescriptor considerNameOfDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceStringUsage,
            title: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantAnalyzerTitle,
            messageFormat: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantDescription);

        private static readonly DiagnosticDescriptor sourceTypeNotIEnumerableDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceSourceTypeNotIEnumerable,
            title: TestCaseSourceUsageConstants.SourceTypeNotIEnumerableTitle,
            messageFormat: TestCaseSourceUsageConstants.SourceTypeNotIEnumerableMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.SourceTypeNotIEnumerableDescription);

        private static readonly DiagnosticDescriptor sourceTypeNoDefaultConstructorDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceSourceTypeNoDefaultConstructor,
            title: TestCaseSourceUsageConstants.SourceTypeNoDefaultConstructorTitle,
            messageFormat: TestCaseSourceUsageConstants.SourceTypeNoDefaultConstructorMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.SourceTypeNoDefaultConstructorDescription);

        private static readonly DiagnosticDescriptor sourceNotStaticDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceSourceIsNotStatic,
            title: TestCaseSourceUsageConstants.SourceIsNotStaticTitle,
            messageFormat: TestCaseSourceUsageConstants.SourceIsNotStaticMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.SourceIsNotStaticDescription);

        private static readonly DiagnosticDescriptor mismatchInNumberOfParameters = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters,
            title: TestCaseSourceUsageConstants.MismatchInNumberOfParametersTitle,
            messageFormat: TestCaseSourceUsageConstants.MismatchInNumberOfParametersMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.MismatchInNumberOfParametersDescription);

        private static readonly DiagnosticDescriptor sourceDoesNotReturnIEnumerable = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceDoesNotReturnIEnumerable,
            title: TestCaseSourceUsageConstants.SourceDoesNotReturnIEnumerableTitle,
            messageFormat: TestCaseSourceUsageConstants.SourceDoesNotReturnIEnumerableMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.SourceDoesNotReturnIEnumerableDescription);

        private static readonly DiagnosticDescriptor parametersSuppliedToFieldOrProperty = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceSuppliesParametersToFieldOrProperty,
            title: TestCaseSourceUsageConstants.TestCaseSourceSuppliesParametersTitle,
            messageFormat: TestCaseSourceUsageConstants.TestCaseSourceSuppliesParametersMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.TestCaseSourceSuppliesParametersDescription);

        private static readonly DiagnosticDescriptor mismatchInNumberOfTestMethodParameters = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfTestMethodParameters,
            title: TestCaseSourceUsageConstants.MismatchInNumberOfTestMethodParametersTitle,
            messageFormat: TestCaseSourceUsageConstants.MismatchInNumberOfTestMethodParametersMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.MismatchInNumberOfTestMethodParametersDescription);

        private static readonly DiagnosticDescriptor mismatchWithTestMethodParameterType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceMismatchWithTestMethodParameterType,
            title: TestCaseSourceUsageConstants.MismatchWithTestMethodParameterTypeTitle,
            messageFormat: TestCaseSourceUsageConstants.MismatchWithTestMethodParameterTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.MismatchWithTestMethodParameterTypeDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            considerNameOfDescriptor,
            missingSourceDescriptor,
            sourceTypeNotIEnumerableDescriptor,
            sourceTypeNoDefaultConstructorDescriptor,
            sourceNotStaticDescriptor,
            mismatchInNumberOfParameters,
            sourceDoesNotReturnIEnumerable,
            parametersSuppliedToFieldOrProperty,
            mismatchInNumberOfTestMethodParameters,
            mismatchWithTestMethodParameterType);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);

        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)

        {
            var testCaseSourceAttribute = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute);
            if (testCaseSourceAttribute is null)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(syntaxContext => AnalyzeAttribute(syntaxContext, testCaseSourceAttribute), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol testCaseSourceAttribute)
        {
            var attributeInfo = SourceHelpers.GetSourceAttributeInformation(
                context,
                testCaseSourceAttribute,
                NUnitFrameworkConstants.NameOfTestCaseSourceAttribute);

            if (attributeInfo is null)
            {
                return;
            }

            var attributeNode = (AttributeSyntax)context.Node;
            var stringConstant = attributeInfo.SourceName;

            if (stringConstant is null && attributeNode.ArgumentList?.Arguments.Count == 1)
            {
                // The Type argument in this form represents the class that provides test cases.
                // It must have a default constructor and implement IEnumerable.
                var sourceType = attributeInfo.SourceType;
                bool typeImplementsIEnumerable = sourceType.IsIEnumerable(out _);
                bool typeHasDefaultConstructor = sourceType.Constructors.Any(c => c.Parameters.IsEmpty);
                if (!typeImplementsIEnumerable)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        sourceTypeNotIEnumerableDescriptor,
                        attributeNode.ArgumentList.Arguments[0].GetLocation(),
                        sourceType.Name));
                }
                else if (!typeHasDefaultConstructor)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        sourceTypeNoDefaultConstructorDescriptor,
                        attributeNode.ArgumentList.Arguments[0].GetLocation(),
                        sourceType.Name));
                }

                return;
            }

            var syntaxNode = attributeInfo.SyntaxNode;

            if (syntaxNode is null || stringConstant is null)
            {
                return;
            }

            var symbol = SourceHelpers.GetMember(attributeInfo);
            if (symbol is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    missingSourceDescriptor,
                    syntaxNode.GetLocation(),
                    stringConstant));
            }
            else
            {
                SourceHelpers.ReportToUseNameOfIfApplicable(
                    context,
                    syntaxNode,
                    attributeInfo,
                    symbol,
                    stringConstant,
                    considerNameOfDescriptor);

                if (!symbol.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        sourceNotStaticDescriptor,
                        syntaxNode.GetLocation(),
                        stringConstant));
                }

                ITypeSymbol? elementType = null;

                switch (symbol)
                {
                    case IPropertySymbol property:
                        elementType = ReportIfSymbolNotIEnumerable(context, syntaxNode, property.Type);
                        ReportIfParametersSupplied(context, syntaxNode, attributeInfo.NumberOfMethodParameters, "properties");
                        break;
                    case IFieldSymbol field:
                        elementType = ReportIfSymbolNotIEnumerable(context, syntaxNode, field.Type);
                        ReportIfParametersSupplied(context, syntaxNode, attributeInfo.NumberOfMethodParameters, "fields");
                        break;
                    case IMethodSymbol method:
                        elementType = ReportIfSymbolNotIEnumerable(context, syntaxNode, method.ReturnType);

                        var methodParametersFromAttribute = attributeInfo.NumberOfMethodParameters ?? 0;
                        if (method.Parameters.Length != methodParametersFromAttribute)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                mismatchInNumberOfParameters,
                                syntaxNode.GetLocation(),
                                methodParametersFromAttribute,
                                method.Parameters.Length));
                        }

                        break;
                }

                if (elementType is not null)
                {
                    MethodDeclarationSyntax? testMethodDeclaration = attributeNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                    if (testMethodDeclaration is not null)
                    {
                        IMethodSymbol? testMethod = context.SemanticModel.GetDeclaredSymbol(testMethodDeclaration);
                        if (testMethod is not null)
                        {
                            var (methodRequiredParameters, methodOptionalParameters, methodParamsParameters) = testMethod.GetParameterCounts();

                            if (elementType.SpecialType != SpecialType.System_String && (elementType.SpecialType == SpecialType.System_Object || elementType.IsIEnumerable(out _) ||
                                IsOrDerivesFrom(elementType, context.SemanticModel.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestCaseParameters))))
                            {
                                // We only know that there is 1 or (likely) more parameters.
                                // The object could hide an array, possibly with a variable number of elements: TestCaseData.Argument.
                                // Potentially we could examine the body of the TestCaseSource to see if we can determine the exact amount.
                                // For complex method that is certainly beyond the scope of this.
                                if (methodRequiredParameters + methodOptionalParameters < 1)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(
                                            mismatchInNumberOfTestMethodParameters,
                                            syntaxNode.GetLocation(),
                                            ">0",
                                            methodRequiredParameters + methodOptionalParameters));
                                }
                            }
                            else
                            {
                                if (methodRequiredParameters + methodOptionalParameters != 1)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(
                                            mismatchInNumberOfTestMethodParameters,
                                            syntaxNode.GetLocation(),
                                            1,
                                            methodRequiredParameters + methodOptionalParameters));
                                }
                                else
                                {
                                    IParameterSymbol testMethodParameter = testMethod.Parameters.First();

                                    if (!NUnitEqualityComparerHelper.CanBeEqual(elementType, testMethodParameter.Type, context.Compilation))
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(
                                                mismatchWithTestMethodParameterType,
                                                syntaxNode.GetLocation(),
                                                elementType,
                                                testMethodParameter.Type,
                                                testMethodParameter.Name));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static ITypeSymbol? ReportIfSymbolNotIEnumerable(
            SyntaxNodeAnalysisContext context,
            SyntaxNode syntaxNode,
            ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsIEnumerable(out ITypeSymbol? elementType))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    sourceDoesNotReturnIEnumerable,
                    syntaxNode.GetLocation(),
                    typeSymbol));
                return null;
            }
            else
            {
                return elementType ?? context.Compilation.GetSpecialType(SpecialType.System_Object);
            }
        }

        private static void ReportIfParametersSupplied(
            SyntaxNodeAnalysisContext context,
            SyntaxNode syntaxNode,
            int? numberOfMethodParameters,
            string kind)
        {
            if (numberOfMethodParameters > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    parametersSuppliedToFieldOrProperty,
                    syntaxNode.GetLocation(),
                    numberOfMethodParameters,
                    kind));
            }
        }

        private static bool IsOrDerivesFrom(ITypeSymbol type, ITypeSymbol? baseType)
        {
            if (baseType is null)
            {
                return false;
            }

            ITypeSymbol? typeAtHand = type;
            do
            {
                if (SymbolEqualityComparer.Default.Equals(typeAtHand, baseType))
                {
                    return true;
                }

                typeAtHand = typeAtHand.BaseType;
            }
            while (typeAtHand is not null);

            return false;
        }
    }
}
