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
                        // Like TestCaseSource(typeof(XXX)), TestFixtureSource(typeof(XXX))

                        // First look for a usage in the same syntax tree (aka the same source file)
                        if (SyntaxTreeContainsUsage(context, sourceTree, typeSymbol))
                        {
                            SuppressDiagnostic(context, diagnostic);
                            continue;
                        }

                        // Then look for a usage in any other syntax tree (aka any other source file)
                        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
                        {
                            if (syntaxTree == sourceTree)
                            {
                                // We already looked for a usage in this syntax tree, so we can skip it
                                continue;
                            }

                            if (SyntaxTreeContainsUsage(context, syntaxTree, typeSymbol))
                            {
                                SuppressDiagnostic(context, diagnostic);
                                break;
                            }
                        }
                    }
                }
            }

            static bool SyntaxTreeContainsUsage(SuppressionAnalysisContext context, SyntaxTree syntaxTree, INamedTypeSymbol typeSymbol)
            {
                SyntaxNode root = syntaxTree.GetRoot(context.CancellationToken);
                SemanticModel semanticModel = context.GetSemanticModel(syntaxTree);

                // Do not descend into statements, because we are only interested in attributes
                foreach (var attribute in root.DescendantNodes(s => s is not StatementSyntax)
                                              .OfType<AttributeSyntax>())
                {
                    // We are looking for typeof expressions as the only parameter in attributes on method declarations
                    if (attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Count != 1)
                        continue;

                    var firstArgument = attribute.ArgumentList.Arguments[0];
                    if (firstArgument.Expression is TypeOfExpressionSyntax typeOfExpression)
                    {
                        var symbolInfo = semanticModel.GetSymbolInfo(typeOfExpression.Type, context.CancellationToken);
                        if (symbolInfo.Symbol is INamedTypeSymbol namedTypeSymbol &&
                            SymbolEqualityComparer.Default.Equals(namedTypeSymbol, typeSymbol))
                        {
                            // Make sure that the attribute is one of the NUnit attributes (TestCaseSource, TestFixtureSource)
                            // that instantiate the class to get an enumeration of values
                            var attributeConstructor = semanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol as IMethodSymbol;
                            var attributeType = attributeConstructor?.ContainingType;
                            if (attributeType is not null &&
                                attributeType.ContainingAssembly.Name is NUnitFrameworkConstants.NUnitFrameworkAssemblyName &&
                                attributeType.Name is NUnitFrameworkConstants.NameOfTestCaseSourceAttribute
                                                   or NUnitFrameworkConstants.NameOfTestFixtureSourceAttribute)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            static void SuppressDiagnostic(SuppressionAnalysisContext context, Diagnostic diagnostic)
            {
                context.ReportSuppression(Suppression.Create(AvoidUninstantiatedInternalNUnitTestClasses, diagnostic));
            }
        }
    }
}
