using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.SourceCommon;

namespace NUnit.Analyzers.TestCaseSourceUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestCaseSourceUsesStringAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor missingSourceDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceIsMissing,
            title: "The TestCaseSource argument does not specify an existing member.",
            messageFormat: "The TestCaseSource argument '{0}' does not specify an existing member.",
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            considerNameOfDescriptor,
            missingSourceDescriptor,
            sourceTypeNotIEnumerableDescriptor,
            sourceTypeNoDefaultConstructorDescriptor,
            sourceNotStaticDescriptor,
            mismatchInNumberOfParameters,
            sourceDoesNotReturnIEnumerable,
            parametersSuppliedToFieldOrProperty);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(x => AnalyzeAttribute(x), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attributeInfo = SourceHelpers.GetSourceAttributeInformation(
                context,
                NunitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute,
                NunitFrameworkConstants.NameOfTestCaseSourceAttribute);

            if (attributeInfo == null)
            {
                return;
            }

            var attributeNode = (AttributeSyntax)context.Node;
            var stringConstant = attributeInfo.SourceName;

            if (stringConstant is null && attributeNode.ArgumentList.Arguments.Count == 1)
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

            if (syntaxNode == null || stringConstant == null)
            {
                return;
            }

            var symbol = SourceHelpers.GetMember(context, attributeInfo);
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

                switch (symbol)
                {
                    case IPropertySymbol property:
                        ReportIfSymbolNotIEnumerable(context, syntaxNode, property.Type);
                        ReportIfParametersSupplied(context, syntaxNode, attributeInfo.NumberOfMethodParameters, "properties");
                        break;
                    case IFieldSymbol field:
                        ReportIfSymbolNotIEnumerable(context, syntaxNode, field.Type);
                        ReportIfParametersSupplied(context, syntaxNode, attributeInfo.NumberOfMethodParameters, "fields");
                        break;
                    case IMethodSymbol method:
                        ReportIfSymbolNotIEnumerable(context, syntaxNode, method.ReturnType);

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
            }
        }

        private static void ReportIfSymbolNotIEnumerable(
            SyntaxNodeAnalysisContext context,
            SyntaxNode syntaxNode,
            ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsIEnumerable(out var _))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    sourceDoesNotReturnIEnumerable,
                    syntaxNode.GetLocation(),
                    typeSymbol));
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
    }
}
