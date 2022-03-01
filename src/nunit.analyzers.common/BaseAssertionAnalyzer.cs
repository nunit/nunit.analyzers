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
            context.RegisterOperationAction(this.AnalyzeInvocation, OperationKind.Invocation);
        }

        protected abstract void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation);

        protected static bool IsAssert(IOperation operation)
        {
            return operation is IExpressionStatementOperation expressionOperation &&
                expressionOperation.Operation is IInvocationOperation invocationOperation &&
                IsAssert(invocationOperation);
        }

        protected static bool IsAssert(IInvocationOperation invocationOperation)
        {
            return invocationOperation.TargetMethod.ContainingType.IsAssert();
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            if (context.Operation is not IInvocationOperation invocationOperation)
                return;

            if (!IsAssert(invocationOperation))
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            this.AnalyzeAssertInvocation(context, invocationOperation);
        }
    }
}
