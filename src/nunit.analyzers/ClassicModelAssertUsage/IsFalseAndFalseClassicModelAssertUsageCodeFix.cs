using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	[Shared]
	public sealed class IsFalseAndFalseClassicModelAssertUsageCodeFix
		: ClassicModelAssertUsageCodeFix
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
			AnalyzerIdentifiers.IsFalseUsage,
			AnalyzerIdentifiers.FalseUsage);

		protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
		{
			arguments.Insert(1, SyntaxFactory.Argument(
				SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIs),
					SyntaxFactory.IdentifierName(NunitFrameworkConstants.NameOfIsFalse))));
		}
	}
}
