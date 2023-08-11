#if !NETSTANDARD1_6

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.DiagnosticSuppressors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidUninstantiatedInternalClassesSuppressor : DiagnosticSuppressor
    {
        internal static readonly SuppressionDescriptor AvoidUninstantiatedInternalTestFixtureClasses = new(
            id: AnalyzerIdentifiers.AvoidUninstantiatedInternalClasses,
            suppressedDiagnosticId: "CA1812",
            justification: "Class is a NUnit TestFixture and called by reflection");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(AvoidUninstantiatedInternalTestFixtureClasses);

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

                // Does the class have any Test related methods
                if (typeSymbol is not null &&
                    typeSymbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsTestRelatedMethod(context.Compilation)))
                {
                    context.ReportSuppression(Suppression.Create(AvoidUninstantiatedInternalTestFixtureClasses, diagnostic));
                }
            }
        }
    }
}

#endif
