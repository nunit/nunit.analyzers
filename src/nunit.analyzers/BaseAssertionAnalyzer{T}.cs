using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers
{
    public abstract class BaseAssertionAnalyzer<T> : DiagnosticAnalyzer
        where T : notnull
    {
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(this.AnalyzeCompilationStart);
        }

        protected virtual void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            throw new NotImplementedException($"You must override one of the {nameof(AnalyzeAssertInvocation)} overloads.");
        }

        protected virtual void AnalyzeAssertInvocation(Version nunitVersion, OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            this.AnalyzeAssertInvocation(context, assertOperation);
        }

        protected virtual void AnalyzeAssertInvocation(Version nunitVersion, T? info, OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            this.AnalyzeAssertInvocation(nunitVersion, context, assertOperation);
        }

        protected virtual bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation)
        {
            INamedTypeSymbol containingType = invocationOperation.TargetMethod.ContainingType;
            return containingType.IsAssert() || (hasClassicAssert && containingType.IsClassicAssert());
        }

        protected virtual T? GetAdditionalInfoAtCompilationStart(Compilation compilation) => default(T);

        private void AnalyzeInvocation(Version nunitVersion, T? info, OperationAnalysisContext context)
        {
            if (context.Operation is not IInvocationOperation invocationOperation)
                return;

            if (!this.IsAssert(nunitVersion.Major >= 4, invocationOperation))
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            this.AnalyzeAssertInvocation(nunitVersion, info, context, invocationOperation);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            var nunitAssembly = GetNUnitAssembly(context.Compilation);
            if (nunitAssembly is null)
            {
                // Who would use NUnit.Analyzers without NUnit?
                return;
            }

            var nunitVersion = GetAssemblyFileVersion(context.Compilation, nunitAssembly)
                ?? nunitAssembly.Identity.Version;

            T? info = this.GetAdditionalInfoAtCompilationStart(context.Compilation);

            context.RegisterOperationAction(context => this.AnalyzeInvocation(nunitVersion, info, context), OperationKind.Invocation);
        }

        private static Version? GetAssemblyFileVersion(Compilation compilation, IAssemblySymbol assembly)
        {
            var fileVersionAttributeType = compilation.GetTypeByMetadataName(
                "System.Reflection.AssemblyFileVersionAttribute");

            if (fileVersionAttributeType is null)
                return null;

            var fileVersionAttribute = assembly
                .GetAttributes()
                .FirstOrDefault(attribute =>
                    SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, fileVersionAttributeType));

            if (fileVersionAttribute is null ||
                fileVersionAttribute.ConstructorArguments.Length != 1 ||
                fileVersionAttribute.ConstructorArguments[0].Value is not string versionText)
            {
                return null;
            }

            return Version.TryParse(versionText, out Version? version)
                ? version
                : null;
        }

        private static IAssemblySymbol? GetNUnitAssembly(Compilation compilation)
        {
            return compilation.References
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .FirstOrDefault(assembly =>
                    assembly.Name.Equals(NUnitFrameworkConstants.NUnitFrameworkAssemblyName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
