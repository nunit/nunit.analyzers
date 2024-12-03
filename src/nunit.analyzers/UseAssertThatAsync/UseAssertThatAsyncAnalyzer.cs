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
    private static readonly string[] firstParameterCandidates =
    {
        NUnitFrameworkConstants.NameOfActualParameter,
        NUnitFrameworkConstants.NameOfConditionParameter,
    };

    private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
        id: AnalyzerIdentifiers.UseAssertThatAsync,
        title: UseAssertThatAsyncConstants.Title,
        messageFormat: UseAssertThatAsyncConstants.Message,
        category: Categories.Assertion,
        defaultSeverity: DiagnosticSeverity.Info,
        description: UseAssertThatAsyncConstants.Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

    protected override void AnalyzeAssertInvocation(Version nunitVersion, OperationAnalysisContext context, IInvocationOperation assertOperation)
    {
        // Assert.ThatAsync was introduced in NUnit 4
        if (nunitVersion.Major < 4)
            return;

        if (assertOperation.TargetMethod.Name != NUnitFrameworkConstants.NameOfAssertThat)
            return;

        var arguments = assertOperation.Arguments
            .Where(a => a.ArgumentKind == ArgumentKind.Explicit) // filter out arguments that were not explicitly passed in
            .ToArray();

        // The first parameter is usually the "actual" parameter, but sometimes it's the "condition" parameter.
        // Since the order is not guaranteed, let's just call it "actualArgument" here.
        var actualArgument = arguments.SingleOrDefault(a => firstParameterCandidates.Contains(a.Parameter?.Name))
            ?? arguments[0];
        if (actualArgument.Syntax is not ArgumentSyntax argumentSyntax || argumentSyntax.Expression is not AwaitExpressionSyntax awaitExpression)
            return;

        // Currently, Assert.ThatAsync does not support the Func<string> getExceptionMessage parameter.
        // Therefore, do not touch overloads of Assert.That that has it.
        var funcStringSymbol = context.Compilation.GetTypeByMetadataName("System.Func`1")?
            .Construct(context.Compilation.GetSpecialType(SpecialType.System_String));
        foreach (var argument in assertOperation.Arguments.Where(a => a != actualArgument))
        {
            if (SymbolEqualityComparer.Default.Equals(argument.Value.Type, funcStringSymbol))
            {
                return;
            }
        }

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
