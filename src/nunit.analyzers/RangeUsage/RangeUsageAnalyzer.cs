using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.RangeUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RangeUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor stepIsZero = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.RangeUsesZeroStep,
            title: RangeUsageConstants.StepMustNotBeZeroTitle,
            messageFormat: RangeUsageConstants.StepMustNotBeZeroMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: RangeUsageConstants.StepMustNotBeZeroDescription);

        private static readonly DiagnosticDescriptor invalidIncrementingRange = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.RangeInvalidIncrementing,
            title: RangeUsageConstants.InvalidIncrementingRangeTitle,
            messageFormat: RangeUsageConstants.InvalidIncrementingRangeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: RangeUsageConstants.InvalidIncrementingRangeDescription);

        private static readonly DiagnosticDescriptor invalidDecremantingRange = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.RangeInvalidDecrementing,
            title: RangeUsageConstants.InvalidDecrementingRangeTitle,
            messageFormat: RangeUsageConstants.InvalidDecrementingRangeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: RangeUsageConstants.InvalidDecrementingRangeDescription);

        private static readonly DiagnosticDescriptor mismatchParameterType = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.AttributeValueMismatchedParameterType,
            title: RangeUsageConstants.MismatchParameterTypeTitle,
            messageFormat: RangeUsageConstants.MismatchParameterTypeMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: RangeUsageConstants.MismatchParameterTypeDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            stepIsZero,
            invalidIncrementingRange,
            invalidDecremantingRange,
            mismatchParameterType);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var typeRangeAttribute = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeRangeAttribute);
            if (typeRangeAttribute is null)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(syntaxContext => AnalyzeAttribute(syntaxContext, typeRangeAttribute), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol rangeAttributeType)
        {
            // Quick check:
            //  Verify it is an attribute on a parameter and has either 2 or 3 parameters.
            if (context.Node is not AttributeSyntax attributeNode ||
                attributeNode.ArgumentList is null ||
                attributeNode.ArgumentList.Arguments.Count is < 2 or > 3 ||
                attributeNode.Parent?.Parent is not ParameterSyntax parameterNode)
            {
                return;
            }

            // Verify it is the Range attribute
            if (!attributeNode.Name.ToString().Contains("Range") ||
                !SymbolEqualityComparer.Default.Equals(context.SemanticModel.GetTypeInfo(attributeNode).Type, rangeAttributeType))
            {
                return;
            }

            // Use the semantic model to get the attribute arguments.
            // The Syntax path would require use to evaluate constant expressions as it returns tokens not values.
            IParameterSymbol? parameter = context.SemanticModel.GetDeclaredSymbol(parameterNode);
            AttributeData? attribute = parameter?
                .GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, rangeAttributeType));

            if (parameter is null || attribute is null)
            {
                return;
            }

            TypedConstant from = attribute.ConstructorArguments[0];
            TypedConstant to = attribute.ConstructorArguments[1];
            TypedConstant? step = attribute.ConstructorArguments.Length > 2 ? attribute.ConstructorArguments[2] : null;

            if (parameter.Type is not null && from.Type is not null &&
                !SymbolEqualityComparer.Default.Equals(parameter.Type, from.Type) &&
                !NUnitParamAttributeTypeConversions.CanConvert(from.Type, parameter.Type))
            {
                // Report diagnostic for mismatched parameter type
                context.ReportDiagnostic(Diagnostic.Create(
                    mismatchParameterType,
                    attributeNode.GetLocation(),
                    from.Type,
                    parameter.Type));
            }

            object? fromValue = from.Value;
            object? toValue = to.Value;
            object? stepValue = step?.Value;

            if (stepValue is null)
            {
                // In most cases, no step will automatically determine direction,
                // except when 'from' and 'to' are unsigned.
                // In that case 'step' is always '1'.
                if (fromValue is uint or ulong && toValue is uint or ulong)
                {
                    stepValue = 1U;
                }
                else
                {
                    return; // Step will be determined by the direction of 'from' and 'to', no need to analyze
                }
            }

            if (stepValue is not double stepAsDouble)
            {
                stepAsDouble = Convert.ToDouble(stepValue, CultureInfo.InvariantCulture);
            }

            if (stepAsDouble == 0.0)
            {
                // Report diagnostic for zero step
                context.ReportDiagnostic(Diagnostic.Create(
                    stepIsZero,
                    attributeNode.GetLocation()));
                return;
            }

            if (fromValue is not double fromAsDouble)
            {
                fromAsDouble = Convert.ToDouble(fromValue, CultureInfo.InvariantCulture);
            }

            if (toValue is not double toAsDouble)
            {
                toAsDouble = Convert.ToDouble(toValue, CultureInfo.InvariantCulture);
            }

            if (stepAsDouble < 0.0)
            {
                if (fromAsDouble < toAsDouble)
                {
                    // Report diagnostic for invalid decrementing range
                    context.ReportDiagnostic(Diagnostic.Create(
                        invalidDecremantingRange,
                        attributeNode.GetLocation()));
                }
            }
            else if (stepAsDouble > 0.0)
            {
                if (fromAsDouble > toAsDouble)
                {
                    // Report diagnostic for invalid incrementing range
                    context.ReportDiagnostic(Diagnostic.Create(
                        invalidIncrementingRange,
                        attributeNode.GetLocation()));
                }
            }
        }
    }
}
