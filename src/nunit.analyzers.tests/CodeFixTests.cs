using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NUnit.Analyzers.Tests
{
	public abstract class CodeFixTests
	{
		protected async Task VerifyGetFixes<TAnalyzer, TFix>(string codeFile, int expectedActionCount,
			string expectedTitle, ImmutableArray<string> expectedNewTexts)
			where TAnalyzer : DiagnosticAnalyzer, new()
			where TFix : CodeFixProvider, new()
		{
			var code = File.ReadAllText(codeFile);
			var document = TestHelpers.Create(code);
			var tree = await document.GetSyntaxTreeAsync();
			var diagnostics = await TestHelpers.GetDiagnosticsAsync(code, new TAnalyzer());
			var sourceSpan = diagnostics[0].Location.SourceSpan;

			var actions = new List<CodeAction>();
			var codeActionRegistration = new Action<CodeAction, ImmutableArray<Diagnostic>>(
				(a, _) => { actions.Add(a); });

			var fix = new TFix();
			var codeFixContext = new CodeFixContext(document, diagnostics[0],
				codeActionRegistration, new CancellationToken(false));
			await fix.RegisterCodeFixesAsync(codeFixContext);

			Assert.That(actions.Count, Is.EqualTo(expectedActionCount), nameof(actions.Count));

			await TestHelpers.VerifyActionAsync(actions,
				expectedTitle, document, tree, expectedNewTexts);
		}
	}
}