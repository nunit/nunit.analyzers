using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using static NUnit.Analyzers.Extensions.ITypeSymbolExtensions;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClassicModelAssertUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableDictionary<string, DiagnosticDescriptor> name =
          new Dictionary<string, DiagnosticDescriptor>
          {
              { "False", NUnit1.Descriptor },
              { "IsFalse", NUnit2.Descriptor },
              { "IsTrue", NUnit3.Descriptor },
              { "True", NUnit4.Descriptor },
              { "AreEqual", NUnit5.Descriptor },
              { "AreNotEqual", NUnit6.Descriptor }
          }.ToImmutableDictionary();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ClassicModelAssertUsageAnalyzer.name.Values.ToImmutableArray();

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

                    if (ClassicModelAssertUsageAnalyzer.name.ContainsKey(invocationSymbol.Name))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            ClassicModelAssertUsageAnalyzer.name[invocationSymbol.Name],
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
                    (invocationSymbol.Name == "AreEqual" &&
                        invocationSymbol.Parameters.Length >= 3 &&
                        invocationSymbol.Parameters[2].Type.SpecialType == SpecialType.System_Double).ToString() }
            }.ToImmutableDictionary();
        }
    }
}
