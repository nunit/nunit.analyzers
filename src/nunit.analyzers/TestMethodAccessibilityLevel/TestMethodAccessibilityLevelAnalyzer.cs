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
                if (IsTestMethod(context.SemanticModel, methodNode.AttributeLists))
                {
                    if (!IsPublic(methodNode.Modifiers))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            testMethodIsNotPublic,
                            methodNode.Identifier.GetLocation()));
                    }
                }
                else if (IsSetUpTearDownMethod(context.SemanticModel, methodNode.AttributeLists))
                {
                    if (!IsPublicOrFamily(methodNode.Modifiers))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            testMethodIsNotPublic,
                            methodNode.Identifier.GetLocation()));
                    }
                }
            }
        }

        private static bool IsTestMethod(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var allAttributes = attributeLists.SelectMany(al => al.Attributes);
            return allAttributes.Any(a => a.IsTestMethodAttribute(semanticModel));
        }

        private static bool IsSetUpTearDownMethod(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
        {
            var allAttributes = attributeLists.SelectMany(al => al.Attributes);
            return allAttributes.Any(a => a.IsSetUpOrTearDownMethodAttribute(semanticModel));
        }

        private static bool IsPublic(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // 'MethodInfo.IsFamily' which means protected, but neither 'protected internal' nor 'private protected'
        // See: https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.isfamily?view=netcore-3.1
        private static bool IsPublicOrFamily(SyntaxTokenList modifiers)
        {
            bool protectedSeen = false;
            foreach (var modifier in modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PublicKeyword))
                {
                    return true;
                }
                else if (modifier.IsKind(SyntaxKind.ProtectedKeyword))
                {
                    protectedSeen = true;
                }
                else if (modifier.IsKind(SyntaxKind.PrivateKeyword) || modifier.IsKind(SyntaxKind.InternalKeyword))
                {
                    return false;
                }
            }

            return protectedSeen;
        }
    }
}
