using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers
{
    public abstract class BaseAssertionAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(this.AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is InvocationExpressionSyntax invocationSyntax))
                return;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationSyntax, context.CancellationToken).Symbol as IMethodSymbol;

            if (methodSymbol == null || !methodSymbol.ContainingType.IsAssert())
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            this.AnalyzeAssertInvocation(context, invocationSyntax, methodSymbol);
        }

        protected abstract void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol);
    }
}
