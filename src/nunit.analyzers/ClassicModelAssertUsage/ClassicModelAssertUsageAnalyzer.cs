using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;
using static NUnit.Analyzers.Extensions.ITypeSymbolExtensions;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClassicModelAssertUsageAnalyzer
        : DiagnosticAnalyzer
    {
        private static readonly ImmutableDictionary<string, DiagnosticDescriptor> Names =
          new Dictionary<string, DiagnosticDescriptor>
          {
              { NameOfAssertIsTrue, ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.IsTrueUsage) },
              { NameOfAssertTrue, ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.TrueUsage) },
              { NameOfAssertIsFalse, ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.IsFalseUsage) },
              { NameOfAssertFalse, ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.FalseUsage) },
              { NameOfAssertAreEqual, ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.AreEqualUsage) },
              { NameOfAssertAreNotEqual, ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.AreNotEqualUsage) }
          }.ToImmutableDictionary();

        private static DiagnosticDescriptor CreateDescriptor(string identifier) =>
            new DiagnosticDescriptor(identifier, ClassicModelUsageAnalyzerConstants.Title,
                ClassicModelUsageAnalyzerConstants.Message, Categories.Usage,
                DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ClassicModelAssertUsageAnalyzer.Names.Values.ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                ClassicModelAssertUsageAnalyzer.AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var methodNode = context.Node.Ancestors().OfType<MethodDeclarationSyntax>().SingleOrDefault();
            if (methodNode != null && !methodNode.ContainsDiagnostics)
            {
                var invocationNode = (InvocationExpressionSyntax)context.Node;

                var symbol = context.SemanticModel.GetSymbolInfo(invocationNode.Expression).Symbol;

                if (symbol is IMethodSymbol invocationSymbol && invocationSymbol.ContainingType.IsAssert())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (ClassicModelAssertUsageAnalyzer.Names.ContainsKey(invocationSymbol.Name))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            ClassicModelAssertUsageAnalyzer.Names[invocationSymbol.Name],
                            invocationNode.GetLocation(),
                            ClassicModelAssertUsageAnalyzer.GetProperties(invocationSymbol)));
                    }
                }
            }
        }

        private static ImmutableDictionary<string, string> GetProperties(IMethodSymbol invocationSymbol)
        {
            return new Dictionary<string, string>
            {
                { AnalyzerPropertyKeys.ModelName, invocationSymbol.Name },
                { AnalyzerPropertyKeys.HasToleranceValue,
                    (invocationSymbol.Name == NameOfAssertAreEqual &&
                        invocationSymbol.Parameters.Length >= 3 &&
                        invocationSymbol.Parameters[2].Type.SpecialType == SpecialType.System_Double).ToString() }
            }.ToImmutableDictionary();
        }
    }
}
