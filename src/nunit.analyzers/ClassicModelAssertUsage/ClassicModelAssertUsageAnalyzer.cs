using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClassicModelAssertUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly ImmutableDictionary<string, DiagnosticDescriptor> name =
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ClassicModelAssertUsageAnalyzer.name.Values.ToImmutableArray();

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            if (ClassicModelAssertUsageAnalyzer.name.ContainsKey(methodSymbol.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ClassicModelAssertUsageAnalyzer.name[methodSymbol.Name],
                    assertExpression.GetLocation(),
                    ClassicModelAssertUsageAnalyzer.GetProperties(methodSymbol)));
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
