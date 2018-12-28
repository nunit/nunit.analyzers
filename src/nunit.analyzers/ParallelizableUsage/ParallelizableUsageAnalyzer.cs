using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ParallelizableUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParallelizableUsageAnalyzer : DiagnosticAnalyzer
    {
        internal const string AssemblyAttributeTargetSpecifier = "assembly";

        private static DiagnosticDescriptor CreateDescriptor(string id, string message, DiagnosticSeverity severity) =>
            new DiagnosticDescriptor(id, ParallelizableUsageAnalyzerConstants.Title,
                message, Categories.Usage, severity, true);

        private static readonly DiagnosticDescriptor scopeSelfNoEffectOnAssemblyUsage =
            ParallelizableUsageAnalyzer.CreateDescriptor(
                AnalyzerIdentifiers.ParallelScopeSelfNoEffectOnAssemblyUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeSelfNoEffectOnAssemblyMessage,
                DiagnosticSeverity.Warning);

        private static readonly DiagnosticDescriptor scopeChildrenOnNonParameterizedTest =
            ParallelizableUsageAnalyzer.CreateDescriptor(
                AnalyzerIdentifiers.ParallelScopeChildrenOnNonParameterizedTestMethodUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeChildrenOnNonParameterizedTestMethodMessage,
                DiagnosticSeverity.Error);

        private static readonly DiagnosticDescriptor scopeFixturesOnTest =
            ParallelizableUsageAnalyzer.CreateDescriptor(
                AnalyzerIdentifiers.ParallelScopeFixturesOnTestMethodUsage,
                ParallelizableUsageAnalyzerConstants.ParallelScopeFixturesOnTestMethodMessage,
                DiagnosticSeverity.Error);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            scopeSelfNoEffectOnAssemblyUsage,
            scopeChildrenOnNonParameterizedTest,
            scopeFixturesOnTest);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(ParallelizableUsageAnalyzer.AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var parallelizableAttributeType = context.SemanticModel.Compilation.GetTypeByMetadataName(
                NunitFrameworkConstants.FullNameOfTypeParallelizableAttribute);
            if (parallelizableAttributeType == null)
                return;

            var attributeNode = (AttributeSyntax)context.Node;
            var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;

            if (parallelizableAttributeType.ContainingAssembly.Identity != attributeSymbol?.ContainingAssembly.Identity ||
                NunitFrameworkConstants.NameOfParallelizableAttribute != attributeSymbol?.ContainingType.Name)
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            var possibleEnumValue = GetOptionalEnumValue(context, attributeNode);
            if (possibleEnumValue == null)
                return;

            int enumValue = possibleEnumValue.Value;
            var attributeListSyntax = attributeNode.Parent as AttributeListSyntax;
            if (attributeListSyntax == null)
                return;

            if (HasExactFlag(enumValue, ParallelizableUsageAnalyzerConstants.ParallelScope.Self))
            {
                // Specifying ParallelScope.Self on an assembly level attribute has no effect
                var atAssemblyLevel = attributeListSyntax.Target?.Identifier.ValueText == AssemblyAttributeTargetSpecifier;
                if (atAssemblyLevel)
                {
                    context.ReportDiagnostic(Diagnostic.Create(scopeSelfNoEffectOnAssemblyUsage,
                        attributeNode.GetLocation()));
                }
            }
            else if (HasFlag(enumValue, ParallelizableUsageAnalyzerConstants.ParallelScope.Children))
            {
                // One may not specify ParallelScope.Children on a non-parameterized test method
                if (IsNonParameterizedTestMethod(context, attributeListSyntax.Parent as MethodDeclarationSyntax))
                {
                    context.ReportDiagnostic(Diagnostic.Create(scopeChildrenOnNonParameterizedTest,
                        attributeNode.GetLocation()));
                }
            }
            else if (HasFlag(enumValue, ParallelizableUsageAnalyzerConstants.ParallelScope.Fixtures))
            {
                // One may not specify ParallelScope.Fixtures on a test method
                if (attributeListSyntax.Parent is MethodDeclarationSyntax)
                {
                    context.ReportDiagnostic(Diagnostic.Create(scopeFixturesOnTest,
                        attributeNode.GetLocation()));
                }
            }
        }

        private static int? GetOptionalEnumValue(SyntaxNodeAnalysisContext context, AttributeSyntax attributeNode)
        {
            var attributePositionalAndNamedArguments = attributeNode.GetArguments();
            var attributePositionalArguments = attributePositionalAndNamedArguments.Item1;
            var noExplicitEnumArgument = attributePositionalArguments.Length == 0;
            if (noExplicitEnumArgument)
            {
                return ParallelizableUsageAnalyzerConstants.ParallelScope.Self;
            }
            else
            {
                var arg = attributePositionalArguments[0];
                var constantValue = context.SemanticModel.GetConstantValue(arg.Expression);
                if (constantValue.HasValue)
                {
                    return constantValue.Value as int?;
                }
            }

            return null;
        }

        private static bool IsNonParameterizedTestMethod(SyntaxNodeAnalysisContext context,
            MethodDeclarationSyntax methodDeclarationSyntax)
        {
            if (methodDeclarationSyntax == null)
                return false;

            // The method is only a parametric method if (see DefaultTestCaseBuilder.BuildFrom)
            // * it has parameters
            // * is marked with one or more attributes deriving from ITestBuilder
            // * the attributes defines tests (difficult to access without evaluating the code)
            bool noParameters = methodDeclarationSyntax.ParameterList.Parameters.Count == 0;

            var allAttributes = methodDeclarationSyntax.AttributeLists.SelectMany(al => al.Attributes);
            bool noITestBuilders = !allAttributes.Where(a => DerivesFromITestBuilder(context, a)).Any();
            return noParameters && noITestBuilders;
        }

        private static bool DerivesFromITestBuilder(SyntaxNodeAnalysisContext context, AttributeSyntax attribute)
        {
            var parallelizableAttributeType = context.SemanticModel.Compilation.GetTypeByMetadataName(
                NunitFrameworkConstants.FullNameOfTypeITestBuilder);
            if (parallelizableAttributeType == null)
                return false;

            var attributeType = context.SemanticModel.GetTypeInfo(attribute).Type;

            if (attributeType == null)
                return false;

            return attributeType.AllInterfaces.Any(i => i.Equals(parallelizableAttributeType));
        }

        private static bool HasFlag(int enumValue, int flag)
            => (enumValue & flag) == flag;

        private static bool HasExactFlag(int enumValue, int flag)
            => enumValue == flag;
    }
}
