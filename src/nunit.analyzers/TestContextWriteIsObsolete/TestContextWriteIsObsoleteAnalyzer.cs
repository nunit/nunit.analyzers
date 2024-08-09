using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.TestContextWriteIsObsolete
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestContextWriteIsObsoleteAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestContextWriteIsObsolete,
            title: TestContextWriteIsObsoleteAnalyzerConstants.Title,
            messageFormat: TestContextWriteIsObsoleteAnalyzerConstants.Message,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: TestContextWriteIsObsoleteAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol? testContextType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestContext);
            if (testContextType is null)
            {
                return;
            }

            context.RegisterOperationAction(context => AnalyzeInvocation(testContextType, context), OperationKind.Invocation);
        }

        private static void AnalyzeInvocation(INamedTypeSymbol testContextType, OperationAnalysisContext context)
        {
            if (context.Operation is not IInvocationOperation invocationOperation)
                return;

            // TestContext.Write methods are static methods
            if (invocationOperation.Instance is not null)
                return;

            IMethodSymbol targetMethod = invocationOperation.TargetMethod;

            if (!targetMethod.ReturnsVoid)
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            if (!SymbolEqualityComparer.Default.Equals(targetMethod.ContainingType, testContextType))
                return;

            if (targetMethod.Name is NUnitFrameworkConstants.NameOfWrite or
                                     NUnitFrameworkConstants.NameOfWriteLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    invocationOperation.Syntax.GetLocation()));
            }
        }
    }
}
