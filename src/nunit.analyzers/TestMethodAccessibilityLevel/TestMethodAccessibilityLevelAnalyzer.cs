using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.TestMethodAccessibilityLevel
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestMethodAccessibilityLevelAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor testMethodIsNotPublic = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestMethodIsNotPublic,
            title: TestMethodAccessibilityLevelConstants.TestMethodIsNotPublicTitle,
            messageFormat: TestMethodAccessibilityLevelConstants.TestMethodIsNotPublicMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestMethodAccessibilityLevelConstants.TestMethodIsNotPublicDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(testMethodIsNotPublic);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodNode = context.Node as MethodDeclarationSyntax;

            if (methodNode != null && !methodNode.ContainsDiagnostics)
            {
                var testCaseType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestCaseAttribute);
                var testType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestAttribute);

                if (testCaseType == null || testType == null)
                    return;

                if (IsTestMethod(context.SemanticModel, methodNode.AttributeLists) &&
                    !methodNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        testMethodIsNotPublic,
                        methodNode.Identifier.GetLocation()));
                }
            }
        }

        private static bool IsTestMethod(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var allAttributes = attributeLists.SelectMany(al => al.Attributes);
            return allAttributes.Any(a =>
                a.DerivesFromITestBuilder(semanticModel) || a.DerivesFromISimpleTestBuilder(semanticModel));
        }
    }
}
