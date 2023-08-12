using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

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
            var valuesType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeValuesAttribute);
            if (valuesType is null)
            {
                return;
            }

            // TODO: Which approach is correct?
            // context.RegisterSyntaxNodeAction(symbolContext => AnalyzeAttribute(symbolContext, valuesType), SyntaxKind.Attribute);
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
                for (var index = 0; index < attribute.ConstructorArguments.Length; index++)
                {
                    var constructorArgument = attribute.ConstructorArguments[index];
                    var argumentTypeMatchesParameterType = constructorArgument.CanAssignTo(parameterSymbol.Type,
                                                                                           symbolContext.Compilation,
                                                                                           allowImplicitConversion: true,
                                                                                           allowEnumToUnderlyingTypeConversion: true);
                    if (argumentTypeMatchesParameterType)
                    {
                        continue;
                    }

                    var attributeArgumentSyntax = attribute.GetConstructorArgumentSyntax(index,
                                                                                         symbolContext.CancellationToken);
                    if (attributeArgumentSyntax is null)
                    {
                        continue;
                    }

                    var diagnostic = Diagnostic.Create(parameterTypeMismatch,
                                                       attributeArgumentSyntax.GetLocation(),
                                                       index,
                                                       constructorArgument.Type?.ToDisplayString() ?? "<null>",
                                                       parameterSymbol.Name,
                                                       parameterSymbol.Type);
                    symbolContext.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol valuesType)
        {
            var blah = (AttributeSyntax)context.Node;

            // var foo = context.
            var attributeSymbol = context.SemanticModel.GetSymbolInfo(blah).Symbol;
            if (valuesType.ContainingAssembly.Identity != attributeSymbol?.ContainingAssembly.Identity ||
                attributeSymbol.ContainingType.Name != NUnitFrameworkConstants.NameOfValuesAttribute)
            {
                return;
            }

            // TODO: Extract type of argument which this attribute is on.

            context.CancellationToken.ThrowIfCancellationRequested();

            // var fooBar = valuesType.Co
            // TODO: remove !s.
            var parameterType = ((ParameterSyntax)blah.Parent!.Parent!).Type!;
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
