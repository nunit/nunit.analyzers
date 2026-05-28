using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.TaskReturnShouldBeUsed
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TaskReturnShouldBeUsedAnalyzer : BaseAssertionAnalyzer<TaskTypes>
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TaskReturnShouldBeUsed,
            title: TaskReturnShouldBeUsedAnalyzerConstants.Title,
            messageFormat: TaskReturnShouldBeUsedAnalyzerConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TaskReturnShouldBeUsedAnalyzerConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(descriptor);

        protected override TaskTypes? GetAdditionalInfoAtCompilationStart(Compilation compilation)
        {
            INamedTypeSymbol? taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            INamedTypeSymbol? taskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

            if (taskType is null || taskOfTType is null)
            {
                // If we can't find Task or Task<T> we won't be able to analyze anything, so we return null and skip the analysis.
                return null;
            }

            return new TaskTypes(taskType, taskOfTType);
        }

        protected override void AnalyzeAssertInvocation(Version nunitVersion, TaskTypes? info, OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            // NUnit < 4 doesn't have assertion APIs returning Task/Task<T>, so we don't need to check anything.
            if (info is null || nunitVersion.Major < 4)
                return;

            // Check if the method returns 'Task' (or Task<T>).
            if (assertOperation.TargetMethod.ReturnType is not INamedTypeSymbol operationReturnType ||
                info.IsTaskOrTaskOfT(operationReturnType) is false ||
                assertOperation.Parent is IAwaitOperation
                                       or IInvocationOperation
                                       or IPropertyReferenceOperation
                                       or IAssignmentOperation
                                       or IReturnOperation)
            {
                // The method either does not return Task/Task<T>,
                // or the invocation is already being awaited,
                // or the invocation is having .Wait() or .Result called on it.
                // or the invocation is being assigned to a variable of type Task/Task<T>.
                // either way we don't need to report a diagnostic.
                return;
            }

            // Cases like:
            //      Exception? exception = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
            // or:
            //      Exception? exception;
            //      exception = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
            // Will trigger an IConversionOperation with a not-available conversion and hence a compile time error.
            // We also want to raise our diagnostic so the user can apply the code fix to await the method call

            if (assertOperation.Parent is IVariableInitializerOperation variableInitializer)
            {
                // We are trying to detect cases like:
                // 'var' declarations are a bit tricky as they could be either
                //      var exception = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
                //      var task = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
                if (variableInitializer.Parent is IVariableDeclaratorOperation variableDeclarator &&
                    variableDeclarator.Parent is IVariableDeclarationOperation variableDeclaration &&
                    variableDeclaration.Syntax is VariableDeclarationSyntax variableDeclarationSyntax)
                {
                    // If the variable is declared with an explicit Task/Task<T> type we don't need to do anything.
                    // If declared as Exception? we don't get here because of the not-available conversion error mentioned above.
                    if (!variableDeclarationSyntax.Type.IsVar)
                        return;

                    // We don't want to report diagnostics for unlikely cases like:
                    //      var task = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
                    //      Exception? exception = await task;
                    AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(variableDeclarationSyntax.SyntaxTree);
                    if (!options.TryGetValue("dotnet_diagnostic.NUnit2059.raise_for_var_declaration_containing", out string? include))
                    {
                        // This is an heuristic to fix cases like:
                        // var ex = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
                        //      var exceptionTask = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
                        //      Exception? exception = await exceptionTask;
                        include = "ex";
                    }

                    if (!options.TryGetValue("dotnet_diagnostic.NUnit2059.do_not_raise_for_var_declaration_containing", out string? exclude))
                    {
                        // This is an heuristic to fix cases like:
                        //      var exceptionTask = Assert.ThrowsAsync<Exception>(() => SomeMethodAsync());
                        //      Exception? exception = await exceptionTask;
                        exclude = "task";
                    }

                    if (!variableDeclarator.Symbol.Name.Contains(include, StringComparison.OrdinalIgnoreCase) ||
                        variableDeclarator.Symbol.Name.Contains(exclude, StringComparison.OrdinalIgnoreCase))
                    {
                        // Either the variable does not include the include string (e.g. "ex")
                        // or it includes the exclude string (e.g. "task"),
                        // in both cases we don't report a diagnostic.
                        return;
                    }
                }
            }

            var builder = ImmutableDictionary.CreateBuilder<string, string?>();

            // Add some properties to the diagnostic to be used in the code fix provider.
            // As the code fix provider only deals with Syntax (i.e. text) it cannot determine types.
            if (context.ContainingSymbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.IsAsync)
                {
                    // Implies Task(<T>) return type because we don't allow async void
                    builder.Add(AnalyzerPropertyKeys.IsAsync, "true");
                }
                else if (methodSymbol.ReturnType is INamedTypeSymbol methodReturnType &&
                    info.IsTaskOrTaskOfT(methodReturnType))
                {
                    builder.Add(AnalyzerPropertyKeys.IsAsync, "false");
                }
            }

            if (info.IsTaskOfT(operationReturnType))
            {
                // If we can't await we need to know if we need to append .Wait() or .Result;
                builder.Add(AnalyzerPropertyKeys.IsTaskT, "true");
            }

            context.ReportDiagnostic(Diagnostic.Create(
                descriptor,
                assertOperation.Syntax.GetLocation(),
                builder.ToImmutable(),
                assertOperation.TargetMethod.Name));
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public sealed class TaskTypes
    {
        internal TaskTypes(INamedTypeSymbol task, INamedTypeSymbol taskOfT)
        {
            this.Task = task;
            this.TaskOfT = taskOfT;
        }

        public INamedTypeSymbol Task { get; }
        public INamedTypeSymbol TaskOfT { get; }

        public bool IsTask(INamedTypeSymbol returnType)
            => SymbolEqualityComparer.Default.Equals(returnType, this.Task);
        public bool IsTaskOfT(INamedTypeSymbol returnType)
            => returnType.IsGenericType && SymbolEqualityComparer.Default.Equals(returnType.ConstructedFrom, this.TaskOfT);
        public bool IsTaskOrTaskOfT(INamedTypeSymbol returnType)
            => this.IsTask(returnType) || this.IsTaskOfT(returnType);
    }
#pragma warning restore SA1402 // File may only contain a single type
}
