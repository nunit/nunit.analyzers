using System.Collections.Concurrent;
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
            ConcurrentDictionary<INamedTypeSymbol, Diagnostic> nonFixtureTypes = new(SymbolEqualityComparer.Default);

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
                        // We need to check if the compilation contains any references to the type symbol in a Source attribute
                        // Collate all the non-fixture types that we have seen so far, so we only need to check the source once for all instances.
                        nonFixtureTypes[typeSymbol] = diagnostic;
                    }
                }
            }

            if (!nonFixtureTypes.IsEmpty)
            {
                INamedTypeSymbol? testCaseSourceAttributeType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute);
                INamedTypeSymbol? testFixtureSourceAttributeType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestFixtureSourceAttribute);

                if (testCaseSourceAttributeType is null || testFixtureSourceAttributeType is null)
                {
                    // Code doesn't reference NUnit.Framework, so we can skip the rest of the analysis
                    return;
                }

                // Now look for a usage in any syntax tree in the compilation for any of the non-fixture types we have seen so far
                foreach (var syntaxTree in context.Compilation.SyntaxTrees)
                {
                    SuppressDiagnosticsIfTypeIsUsedInNUnitSourceAttribute(
                        context,
                        testCaseSourceAttributeType,
                        testFixtureSourceAttributeType,
                        nonFixtureTypes,
                        syntaxTree);

                    // If all the non-fixture types have been found in a source attribute, we can stop looking through the syntax trees
                    if (nonFixtureTypes.IsEmpty)
                        break;
                }
            }

            static void SuppressDiagnosticsIfTypeIsUsedInNUnitSourceAttribute(
                SuppressionAnalysisContext context,
                INamedTypeSymbol testCaseSourceAttributeType,
                INamedTypeSymbol testFixtureSourceAttributeType,
                ConcurrentDictionary<INamedTypeSymbol, Diagnostic> nonFixtureTypes,
                SyntaxTree syntaxTree)
            {
                SyntaxNode root = syntaxTree.GetRoot(context.CancellationToken);
                SemanticModel semanticModel = context.GetSemanticModel(syntaxTree);

                // Do not descend into statements, because we are only interested in attributes
                foreach (var attribute in root.DescendantNodes(s => s is not StatementSyntax)
                                              .OfType<AttributeSyntax>())
                {
                    // We are looking for typeof expressions as the only argument
                    if (attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Count != 1)
                        continue;

                    var firstArgument = attribute.ArgumentList.Arguments[0];
                    if (firstArgument.Expression is TypeOfExpressionSyntax typeOfExpression)
                    {
                        // Make sure that the attribute is one of the NUnit attributes (TestCaseSource, TestFixtureSource)
                        // that instantiate the class to get an enumeration of values
                        var attributeConstructor = semanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol as IMethodSymbol;
                        var attributeType = attributeConstructor?.ContainingType;
                        if (attributeType is not null &&
                            (SymbolEqualityComparer.Default.Equals(attributeType, testCaseSourceAttributeType) ||
                            SymbolEqualityComparer.Default.Equals(attributeType, testFixtureSourceAttributeType)))
                        {
                            var referencedType = semanticModel.GetSymbolInfo(typeOfExpression.Type, context.CancellationToken).Symbol as INamedTypeSymbol;
                            if (referencedType is not null &&
                                nonFixtureTypes.TryRemove(referencedType, out Diagnostic? diagnostic))
                            {
                                SuppressDiagnostic(context, diagnostic);
                            }
                        }
                    }
                }
            }

            static void SuppressDiagnostic(SuppressionAnalysisContext context, Diagnostic diagnostic)
            {
                context.ReportSuppression(Suppression.Create(AvoidUninstantiatedInternalNUnitTestClasses, diagnostic));
            }
        }
    }
}
