using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.DiagnosticSuppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidUninstantiatedInternalClassSuppressor : DiagnosticSuppressor
    {
        internal static readonly SuppressionDescriptor AvoidUninstantiatedInternalNUnitTestClasses = new(
            id: AnalyzerIdentifiers.AvoidUninstantiatedInternalClasses,
            suppressedDiagnosticId: "CA1812",
            justification: "Class is an NUnit TestFixture or TestData source and is instantiated using reflection");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(AvoidUninstantiatedInternalNUnitTestClasses);

        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                SyntaxTree? sourceTree = diagnostic.Location.SourceTree;

                if (sourceTree is null)
                {
                    continue;
                }

                SyntaxNode node = sourceTree.GetRoot(context.CancellationToken)
                                            .FindNode(diagnostic.Location.SourceSpan);

                if (node is not ClassDeclarationSyntax classDeclaration)
                {
                    continue;
                }

                SemanticModel semanticModel = context.GetSemanticModel(sourceTree);
                INamedTypeSymbol? typeSymbol = (INamedTypeSymbol?)semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);

                if (typeSymbol is not null)
                {
                    if (typeSymbol.IsTestFixture(context.Compilation))
                    {
                        SuppressDiagnostic(context, diagnostic);
                    }
                    else
                    {
                        // Check is the compilation contains any references to the type symbol in a Source attribute
                        // Like TestCaseSource(typeof(XXX)), TestFixtureSource(typeof(XXX)), and ValueSource(typeof(XXX))
                        var usageToLookFor = $"Source(typeof({typeSymbol.Name})";

                        // First look for a usage in the same syntax tree (aka the same source file)
                        if (SyntaxTreeContainsUsage(sourceTree, usageToLookFor))
                        {
                            SuppressDiagnostic(context, diagnostic);
                            continue;
                        }

                        // Then look for a usage in any other syntax tree (aka any other source file)
                        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
                        {
                            if (syntaxTree == sourceTree)
                            {
                                // We already looked for a usage in the this syntax tree, so we can skip it
                                continue;
                            }

                            if (SyntaxTreeContainsUsage(syntaxTree, usageToLookFor))
                            {
                                SuppressDiagnostic(context, diagnostic);
                                break;
                            }
                        }
                    }
                }
            }

            static bool SyntaxTreeContainsUsage(SyntaxTree syntaxTree, string usageToLookFor)
            {
                string source = syntaxTree.ToString();
                return source.Contains(usageToLookFor);
            }

            static void SuppressDiagnostic(SuppressionAnalysisContext context, Diagnostic diagnostic)
            {
                context.ReportSuppression(Suppression.Create(AvoidUninstantiatedInternalNUnitTestClasses, diagnostic));
            }
        }
    }
}
