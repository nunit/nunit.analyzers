using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.NonTestMethodAccessibilityLevel
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NonTestMethodAccessibilityLevelAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor nonTestMethodIsPublic = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.NonTestMethodIsPublic,
            title: NonTestMethodAccessibilityLevelConstants.NonTestMethodIsPublicTitle,
            messageFormat: NonTestMethodAccessibilityLevelConstants.NonTestMethodIsPublicMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: false,
            description: NonTestMethodAccessibilityLevelConstants.NonTestMethodIsPublicDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(nonTestMethodIsPublic);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var classNode = context.Node as ClassDeclarationSyntax;

            if (classNode is null)
            {
                return;
            }

            // Is this even a TestFixture, i.e.: Does it has any test methods.
            // If not public methods are fine.
            bool hasTestMethods = false;

            var publicNonTestMethods = new List<MethodDeclarationSyntax>();
            foreach (var method in classNode.Members.OfType<MethodDeclarationSyntax>())
            {
                if (IsTestMethod(context.SemanticModel, method.AttributeLists))
                    hasTestMethods = true;
                else if (IsPublicOrInternalMethod(method))
                    publicNonTestMethods.Add(method);
            }

            if (hasTestMethods)
            {
                foreach (var method in publicNonTestMethods)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        nonTestMethodIsPublic,
                        method.Identifier.GetLocation()));
                }
            }
        }

        private static bool IsTestMethod(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var allAttributes = attributeLists.SelectMany(al => al.Attributes);
            return allAttributes.Any(a =>
                a.DerivesFromITestBuilder(semanticModel) || a.DerivesFromISimpleTestBuilder(semanticModel));
        }

        private static bool IsPublicOrInternalMethod(MethodDeclarationSyntax method)
        {
            return method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.InternalKeyword));
        }
    }
}
