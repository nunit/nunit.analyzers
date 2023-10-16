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

        protected abstract void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation);

        protected virtual bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation)
        {
            INamedTypeSymbol containingType = invocationOperation.TargetMethod.ContainingType;
            return containingType.IsAssert() || (hasClassicAssert && containingType.IsClassicAssert());
        }

        private void AnalyzeInvocation(bool hasClassicAssert, OperationAnalysisContext context)
        {
            if (context.Operation is not IInvocationOperation invocationOperation)
                return;

            if (!this.IsAssert(hasClassicAssert, invocationOperation))
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            this.AnalyzeAssertInvocation(context, invocationOperation);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            IEnumerable<AssemblyIdentity> referencedAssemblies = context.Compilation.ReferencedAssemblyNames;

            AssemblyIdentity? nunit = referencedAssemblies.SingleOrDefault(a => a.Name.Equals("nunit.framework", StringComparison.OrdinalIgnoreCase));

            if (nunit is null)
            {
                return;
            }

            bool hasClassicAssert = nunit.Version.Major >= 4;

            context.RegisterOperationAction((context) => this.AnalyzeInvocation(hasClassicAssert, context), OperationKind.Invocation);
        }
    }
}
