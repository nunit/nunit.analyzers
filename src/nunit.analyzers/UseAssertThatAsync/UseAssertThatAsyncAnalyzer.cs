using System;
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

    protected override void AnalyzeAssertInvocation(Version nunitVersion, OperationAnalysisContext context, IInvocationOperation assertOperation)
    {
        // Assert.ThatAsync was introduced in NUnit 4
        if (nunitVersion.Major < 4)
            return;

        if (assertOperation.TargetMethod.Name != NUnitFrameworkConstants.NameOfAssertThat)
            return;

        var arguments = assertOperation.Arguments;

        var actualArgument = arguments.SingleOrDefault(a => a.Parameter?.Name == NUnitFrameworkConstants.NameOfActualParameter)
            ?? arguments[0];
        if (actualArgument.Syntax is not ArgumentSyntax argumentSyntax || argumentSyntax.Expression is not AwaitExpressionSyntax awaitExpression)
            return;

        // Verify that the awaited expression is generic
        var awaitedSymbol = context.Operation.SemanticModel?.GetSymbolInfo(awaitExpression.Expression).Symbol;
        if (awaitedSymbol is IMethodSymbol methodSymbol
            && methodSymbol.ReturnType is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(descriptor, assertOperation.Syntax.GetLocation()));
        }
    }
}
