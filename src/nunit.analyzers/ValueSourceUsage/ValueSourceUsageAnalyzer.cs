using System.Collections.Generic;
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
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(x => AnalyzeAttribute(x), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attributeInfo = SourceHelpers.GetSourceAttributeInformation(
                context,
                NunitFrameworkConstants.FullNameOfTypeValueSourceAttribute,
                NunitFrameworkConstants.NameOfValueSourceAttribute);

            if (attributeInfo == null)
            {
                return;
            }

            var stringConstant = attributeInfo.SourceName;
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
                var sourceIsAccessible = context.SemanticModel.IsAccessible(
                    syntaxNode.GetLocation().SourceSpan.Start,
                    symbol);

                if (attributeInfo.IsStringLiteral && sourceIsAccessible)
                {
                    var nameOfClassTarget = attributeInfo.SourceType.ToMinimalDisplayString(
                        context.SemanticModel,
                        syntaxNode.GetLocation().SourceSpan.Start);

                    var nameOfTarget = attributeInfo.SourceType == context.ContainingSymbol.ContainingType
                        ? stringConstant
                        : $"{nameOfClassTarget}.{stringConstant}";

                    var properties = new Dictionary<string, string>
                    {
                        { SourceCommonConstants.PropertyKeyNameOfTarget, nameOfTarget }
                    };

                    context.ReportDiagnostic(Diagnostic.Create(
                        considerNameOfDescriptor,
                        syntaxNode.GetLocation(),
                        properties.ToImmutableDictionary(),
                        nameOfTarget,
                        stringConstant));
                }

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
                        break;
                    case IFieldSymbol field:
                        ReportIfSymbolNotIEnumerable(context, syntaxNode, field.Type);
                        break;
                    case IMethodSymbol method:
                        ReportIfSymbolNotIEnumerable(context, syntaxNode, method.ReturnType);

                        if (method.Parameters.Length != 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                methodExpectParameters,
                                syntaxNode.GetLocation(),
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
    }
}
