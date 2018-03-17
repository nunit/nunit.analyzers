using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	[Shared]
	public sealed class AreNotEqualClassicModelAssertUsageCodeFix
		: ClassicModelAssertUsageCodeFix
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(AnalyzerIdentifiers.AreNotEqualUsage);

		protected override void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
		{
			arguments.Insert(2, SyntaxFactory.Argument(
				SyntaxFactory.InvocationExpression(
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							SyntaxFactory.IdentifierName(nameof(Is)),
							SyntaxFactory.IdentifierName(nameof(Is.Not))),
						SyntaxFactory.IdentifierName(nameof(Is.Not.EqualTo))))
					.WithArgumentList(SyntaxFactory.ArgumentList(
						SyntaxFactory.SingletonSeparatedList(arguments[0])))));

			// Then we have to remove the 1st argument because that's now in the "Is.EqualTo()"
			arguments.RemoveAt(0);
		}
	}
}
