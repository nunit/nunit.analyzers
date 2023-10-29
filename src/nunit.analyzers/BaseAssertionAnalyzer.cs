using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers
{
    public abstract class BaseAssertionAnalyzer : DiagnosticAnalyzer
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

        protected virtual bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation)
        {
            INamedTypeSymbol containingType = invocationOperation.TargetMethod.ContainingType;
            return containingType.IsAssert() || (hasClassicAssert && containingType.IsClassicAssert());
        }

        private void AnalyzeInvocation(Version nunitVersion, OperationAnalysisContext context)
        {
            if (context.Operation is not IInvocationOperation invocationOperation)
                return;

            if (!this.IsAssert(nunitVersion.Major >= 4, invocationOperation))
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            this.AnalyzeAssertInvocation(nunitVersion, context, invocationOperation);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            IEnumerable<AssemblyIdentity> referencedAssemblies = context.Compilation.ReferencedAssemblyNames;

            AssemblyIdentity? nunit = referencedAssemblies.SingleOrDefault(a => a.Name.Equals("nunit.framework", StringComparison.OrdinalIgnoreCase));

            if (nunit is null)
            {
                // Who would use NUnit.Analyzers without NUnit?
                return;
            }

            context.RegisterOperationAction((context) => this.AnalyzeInvocation(nunit.Version, context), OperationKind.Invocation);
        }
    }
}
