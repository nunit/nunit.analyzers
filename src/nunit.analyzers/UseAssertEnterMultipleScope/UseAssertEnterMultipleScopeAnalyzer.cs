using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseAssertEnterMultipleScope
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseAssertEnterMultipleScopeAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly Version firstNUnitVersionWithEnterMultipleScope = new(4, 2);

        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.UseAssertEnterMultipleScope,
            title: UseAssertEnterMultipleScopeConstants.Title,
            messageFormat: UseAssertEnterMultipleScopeConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: UseAssertEnterMultipleScopeConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        protected override bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation) =>
            base.IsAssert(false, invocationOperation);

        protected override void AnalyzeAssertInvocation(Version nunitVersion, OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (nunitVersion < firstNUnitVersionWithEnterMultipleScope)
            {
                return;
            }

            if (assertOperation.TargetMethod.Name is NUnitFrameworkConstants.NameOfMultiple or NUnitV4FrameworkConstants.NameOfMultipleAsync)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    assertOperation.Syntax.GetLocation()));
            }
        }
    }
}
