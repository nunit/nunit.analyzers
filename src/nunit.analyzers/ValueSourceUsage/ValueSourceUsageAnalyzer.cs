using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.SourceCommon;

namespace NUnit.Analyzers.ValueSourceUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ValueSourceUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor missingSourceDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ValueSourceIsMissing,
            title: ValueSourceUsageConstants.SourceDoesNotSpecifyAnExistingMemberTitle,
            messageFormat: ValueSourceUsageConstants.SourceDoesNotSpecifyAnExistingMemberMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ValueSourceUsageConstants.SourceDoesNotSpecifyAnExistingMemberDescription);

        private static readonly DiagnosticDescriptor considerNameOfDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ValueSourceStringUsage,
            title: ValueSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantAnalyzerTitle,
            messageFormat: ValueSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ValueSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantDescription);

        private static readonly DiagnosticDescriptor sourceNotStaticDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ValueSourceIsNotStatic,
            title: ValueSourceUsageConstants.SourceIsNotStaticTitle,
            messageFormat: ValueSourceUsageConstants.SourceIsNotStaticMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ValueSourceUsageConstants.SourceIsNotStaticDescription);

        private static readonly DiagnosticDescriptor methodExpectParameters = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ValueSourceMethodExpectParameters,
            title: ValueSourceUsageConstants.MethodExpectParametersTitle,
            messageFormat: ValueSourceUsageConstants.MethodExpectParametersMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ValueSourceUsageConstants.MethodExpectParametersDescription);

        private static readonly DiagnosticDescriptor sourceDoesNotReturnIEnumerable = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ValueSourceDoesNotReturnIEnumerable,
            title: ValueSourceUsageConstants.SourceDoesNotReturnIEnumerableTitle,
            messageFormat: ValueSourceUsageConstants.SourceDoesNotReturnIEnumerableMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: ValueSourceUsageConstants.SourceDoesNotReturnIEnumerableDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            considerNameOfDescriptor,
            missingSourceDescriptor,
            sourceNotStaticDescriptor,
            methodExpectParameters,
            sourceDoesNotReturnIEnumerable);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var typeValueSourceAttribute = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeValueSourceAttribute);
            if (typeValueSourceAttribute is null)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(syntaxContext => AnalyzeAttribute(syntaxContext, typeValueSourceAttribute), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol valueSourceType)
        {
            var attributeInfo = SourceHelpers.GetSourceAttributeInformation(
                context,
                valueSourceType,
                NUnitFrameworkConstants.NameOfValueSourceAttribute);

            if (attributeInfo is null)
            {
                return;
            }

            var stringConstant = attributeInfo.SourceName;
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

                ReportIfSymbolNotIEnumerable(context, symbol, syntaxNode);

                if (symbol is IMethodSymbol methodSymbol && methodSymbol.Parameters.Length != 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        methodExpectParameters,
                        syntaxNode.GetLocation(),
                        methodSymbol.Parameters.Length));
                }
            }
        }

        private static void ReportIfSymbolNotIEnumerable(
            SyntaxNodeAnalysisContext context,
            ISymbol symbol,
            SyntaxNode syntaxNode)
        {
            var memberType = symbol switch
            {
                IPropertySymbol property => property.Type,
                IFieldSymbol field => field.Type,
                IMethodSymbol method => method.ReturnType,
                _ => null
            };

            if (memberType is not null)
            {
                if (symbol is IMethodSymbol && memberType.IsAwaitable(out ITypeSymbol? returnType))
                    memberType = returnType;

                if (!memberType.IsIEnumerableOrIAsyncEnumerable(out var _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        sourceDoesNotReturnIEnumerable,
                        syntaxNode.GetLocation(),
                        memberType));
                }
            }
        }
    }
}
