using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.ValuesUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ValuesUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor parameterTypeMismatch = DiagnosticDescriptorCreator.Create(id: AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage,
                                                                                                                title: ValuesUsageAnalyzerConstants.ParameterTypeMismatchTitle,
                                                                                                                messageFormat: ValuesUsageAnalyzerConstants.ParameterTypeMismatchMessage,
                                                                                                                category: Categories.Structure,
                                                                                                                defaultSeverity: DiagnosticSeverity.Error,
                                                                                                                description: ValuesUsageAnalyzerConstants.ParameterTypeMismatchDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(parameterTypeMismatch);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var testCaseType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeValuesAttribute);
            if (testCaseType is null)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(symbolContext => AnalyzeAttribute(symbolContext, testCaseType), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol valuesType)
        {
            var blah = (AttributeSyntax)context.Node;
            var attributeSymbol = context.SemanticModel.GetSymbolInfo(blah).Symbol;
            if (valuesType.ContainingAssembly.Identity != attributeSymbol?.ContainingAssembly.Identity ||
                attributeSymbol.ContainingType.Name != NUnitFrameworkConstants.NameOfValuesAttribute)
            {
                return;
            }

            // TODO: Extract type of argument which this attribute is on.

            context.CancellationToken.ThrowIfCancellationRequested();

            if (blah.ArgumentList is null)
            {
                return;
            }

            var arguments = blah.ArgumentList.Arguments;
            if (arguments.Count == 0)
            {
                return;
            }

            foreach (var argument in arguments)
            {
                var expression = argument.Expression;

                // TODO: Extract type of expression.
                // TODO: Compare type of expression with type of argument.
            }
        }
    }
}
