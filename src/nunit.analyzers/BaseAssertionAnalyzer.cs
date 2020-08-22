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
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(this.AnalyzeInvocation, OperationKind.Invocation);
        }

        protected abstract void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation);

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            if (!(context.Operation is IInvocationOperation invocationOperation))
                return;

            var methodSymbol = invocationOperation.TargetMethod;

            if (methodSymbol == null || !methodSymbol.ContainingType.IsAssert())
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            this.AnalyzeAssertInvocation(context, invocationOperation);
        }
    }
}
