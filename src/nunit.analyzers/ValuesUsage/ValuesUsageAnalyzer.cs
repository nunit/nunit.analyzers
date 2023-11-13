using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ValuesUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ValuesUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor parameterTypeMismatch = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ValuesParameterTypeMismatchUsage,
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
            var valuesType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeValuesAttribute);
            if (valuesType is null)
            {
                return;
            }

            context.RegisterSymbolAction(symbolContext => AnalyzeParameter(symbolContext, valuesType), SymbolKind.Parameter);
        }

        private static void AnalyzeParameter(SymbolAnalysisContext symbolContext, INamedTypeSymbol valuesType)
        {
            var parameterSymbol = (IParameterSymbol)symbolContext.Symbol;
            var attributes = parameterSymbol.GetAttributes();
            if (attributes.Length == 0)
            {
                return;
            }

            foreach (var attribute in attributes.Where(x => x.ApplicationSyntaxReference is not null
                                                       && SymbolEqualityComparer.Default.Equals(x.AttributeClass, valuesType)))
            {
                symbolContext.CancellationToken.ThrowIfCancellationRequested();

                var attributePositionalArguments = attribute.ConstructorArguments.AdjustArguments();

                for (var index = 0; index < attributePositionalArguments.Length; index++)
                {
                    var constructorArgument = attributePositionalArguments[index];

                    // If the compiler detects an illegal constant, we shouldn't check it.
                    // Unfortunately the constant 'null' is marked as Error with a null type.
                    if (constructorArgument.Kind == TypedConstantKind.Error && constructorArgument.Type is not null)
                    {
                        continue;
                    }

                    var argumentSyntax = attribute.GetAdjustedArgumentSyntax(index,
                                                                             attributePositionalArguments,
                                                                             symbolContext.CancellationToken);

                    if (argumentSyntax is null)
                    {
                        continue;
                    }

                    var argumentTypeMatchesParameterType = constructorArgument.CanAssignTo(parameterSymbol.Type,
                                                                                           symbolContext.Compilation,
                                                                                           allowImplicitConversion: true,
                                                                                           allowEnumToUnderlyingTypeConversion: true,
                                                                                           suppressNullableWarning: argumentSyntax.IsSuppressNullableWarning());
                    if (argumentTypeMatchesParameterType)
                    {
                        continue;
                    }

                    var diagnostic = Diagnostic.Create(parameterTypeMismatch,
                                                       argumentSyntax.GetLocation(),
                                                       index,
                                                       constructorArgument.Type?.ToDisplayString() ?? "<null>",
                                                       parameterSymbol.Name,
                                                       parameterSymbol.Type);
                    symbolContext.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
