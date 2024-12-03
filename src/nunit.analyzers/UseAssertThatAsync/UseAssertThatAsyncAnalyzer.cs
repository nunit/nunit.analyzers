using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseAssertThatAsync;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseAssertThatAsyncAnalyzer : BaseAssertionAnalyzer
{
    private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
        id: AnalyzerIdentifiers.UseAssertThatAsync,
        title: UseAssertThatAsyncConstants.Title,
        messageFormat: UseAssertThatAsyncConstants.Message,
        category: Categories.Assertion,
        defaultSeverity: DiagnosticSeverity.Info,
        description: UseAssertThatAsyncConstants.Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(descriptor);

    protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
    {
        if (assertOperation.TargetMethod.Name != NUnitFrameworkConstants.NameOfAssertThat)
            return;

        var arguments = assertOperation.Arguments;

        // For now, just flag the actual argumentSyntax that uses await inline
        // TODO: Also exclude .ConfigureAwait(false)
        var expectedArgument = arguments.Single(a => a.Parameter?.Name == NUnitFrameworkConstants.NameOfActualParameter);
        if (expectedArgument.Syntax is not ArgumentSyntax argumentSyntax || argumentSyntax.Expression is not AwaitExpressionSyntax awaitExpression)
            return;

        // Verify that the awaited expression is a Task<T>-returning method
        var awaitedSymbol = context.Operation.SemanticModel?.GetSymbolInfo(awaitExpression.Expression).Symbol;
        if (awaitedSymbol is not IMethodSymbol methodSymbol || !methodSymbol.ReturnType.Name.Contains("Task")) // TODO:
            return;

        context.ReportDiagnostic(Diagnostic.Create(descriptor, assertOperation.Syntax.GetLocation()));
    }
}
