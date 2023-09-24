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
    public sealed class TypesThatOwnDisposableFieldsShouldBeDisposableSuppressor : DiagnosticSuppressor
    {
        internal static readonly SuppressionDescriptor TypesThatOwnDisposableFieldsShouldHaveATearDown = new(
            id: AnalyzerIdentifiers.TypesThatOwnDisposableFieldsShouldBeDisposable,
            suppressedDiagnosticId: "CA1001",
            justification: "Field should be Disposed in TearDown or OneTimeTearDown method");

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } =
            ImmutableArray.Create(TypesThatOwnDisposableFieldsShouldHaveATearDown);

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
                    // Is the class set up for a InstancePerTestCase
                    AttributeData? fixtureLifeCycleAttribute = typeSymbol.GetAllAttributes().FirstOrDefault(x => x.IsFixtureLifeCycleAttribute(context.Compilation));
                    if (fixtureLifeCycleAttribute is not null &&
                        fixtureLifeCycleAttribute.ConstructorArguments.Length == 1 &&
                        fixtureLifeCycleAttribute.ConstructorArguments[0] is TypedConstant typeConstant &&
                        typeConstant.Kind == TypedConstantKind.Enum &&
                        typeConstant.Type.IsType(NUnitFrameworkConstants.FullNameOfLifeCycle, context.Compilation) &&
                        typeConstant.Value is 1 /* LifeCycle.InstancePerTestCase */)
                    {
                        // If a TestFixture used InstancePerTestCase, it should be IDisposable
                        if (diagnostic.Descriptor.Id == TypesThatOwnDisposableFieldsShouldHaveATearDown.SuppressedDiagnosticId)
                            continue;
                    }

                    if (typeSymbol.IsTestFixture(context.Compilation))
                    {
                        context.ReportSuppression(Suppression.Create(TypesThatOwnDisposableFieldsShouldHaveATearDown, diagnostic));
                    }
                }
            }
        }
    }
}

#endif
