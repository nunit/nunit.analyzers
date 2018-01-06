using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
				{ nameof(Assert.IsTrue), ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.IsTrueUsage) },
				{ nameof(Assert.True), ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.TrueUsage) },
				{ nameof(Assert.IsFalse), ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.IsFalseUsage) },
				{ nameof(Assert.False), ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.FalseUsage) },
				{ nameof(Assert.AreEqual), ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.AreEqualUsage) },
				{ nameof(Assert.AreNotEqual), ClassicModelAssertUsageAnalyzer.CreateDescriptor(AnalyzerIdentifiers.AreNotEqualUsage) }
			}.ToImmutableDictionary();

		private static DiagnosticDescriptor CreateDescriptor(string identifier) =>
			new DiagnosticDescriptor(identifier, ClassicModelUsageAnalyzerConstants.Title,
				ClassicModelUsageAnalyzerConstants.Message, Categories.Usage,
				DiagnosticSeverity.Warning, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ClassicModelAssertUsageAnalyzer.Names.Values.ToImmutableArray();
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(
				ClassicModelAssertUsageAnalyzer.AnalyzeInvocation, SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
		{
			if(!context.Node.Ancestors().OfType<MethodDeclarationSyntax>().Single().ContainsDiagnostics)
			{
				var invocationNode = (InvocationExpressionSyntax)context.Node;
				var invocationSymbol = context.SemanticModel.GetSymbolInfo(
					invocationNode.Expression).Symbol as IMethodSymbol;

				if (invocationSymbol != null && invocationSymbol.ContainingType.IsAssert())
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
						(invocationSymbol.Name == nameof(Assert.AreEqual) &&
							invocationSymbol.Parameters.Length >= 3 &&
							invocationSymbol.Parameters[2].Type.SpecialType == SpecialType.System_Double).ToString() }
				}.ToImmutableDictionary();
		}
	}
}
