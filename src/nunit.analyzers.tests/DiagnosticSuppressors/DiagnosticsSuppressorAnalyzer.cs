using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

#nullable enable

namespace NUnit.Analyzers.Tests.DiagnosticSuppressors
{
    public static class DiagnosticsSuppressorAnalyzer
    {
        public static Task EnsureNotSuppressed(DiagnosticSuppressor suppressor, string testCode)
        {
            return EnsureSuppressed(suppressor, null, testCode);
        }

        public static async Task EnsureSuppressed(DiagnosticSuppressor suppressor, SuppressionDescriptor? suppressionDescriptor, string testCode)
        {
            if (suppressionDescriptor != null)
            {
                Assert.That(suppressor.SupportedSuppressions, Does.Contain(suppressionDescriptor), "Supported Suppression");
            }

            Compilation compilation = TestHelpers.CreateCompilation(testCode);

            ImmutableArray<Diagnostic> compilationErrors = compilation.GetDiagnostics();

            ImmutableArray<Diagnostic> nonHiddenErrors =
                compilationErrors.Where(d => d.Severity != DiagnosticSeverity.Hidden)
                                 .ToImmutableArray();

            ImmutableArray<Diagnostic> suppressibleErrors =
                nonHiddenErrors.Where(d => suppressor.SupportedSuppressions.Any(s => s.SuppressedDiagnosticId == d.Id))
                               .ToImmutableArray();

            Assert.That(nonHiddenErrors, Is.EquivalentTo(suppressibleErrors), "Non suppressible errors");
            Assert.That(suppressibleErrors, Is.Not.Empty, "No errors to suppress");

            var withAnalyzer = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(suppressor));

            ImmutableArray<Diagnostic> analyzerErrors =
                await withAnalyzer.GetAllDiagnosticsAsync()
                                  .ConfigureAwait(false);

            nonHiddenErrors = analyzerErrors.Where(d => d.Severity != DiagnosticSeverity.Hidden)
                                            .ToImmutableArray();

            Assert.That(nonHiddenErrors, Is.Not.Empty);

            Assert.Multiple(() =>
            {
                foreach (var error in nonHiddenErrors)
                {
                    if (suppressionDescriptor is null)
                    {
                        Assert.That(error.IsSuppressed, Is.False, "IsSuppressed: " + error);
                    }
                    else
                    {
                        Assert.That(error.IsSuppressed, Is.True, "IsSuppressed: " + error);
                        if (error.IsSuppressed)
                        {
                            AssertProgrammaticSuppression(error, suppressionDescriptor);
                        }
                    }
                }
            });
        }

        private static void AssertProgrammaticSuppression(Diagnostic error, SuppressionDescriptor suppressionDescriptor)
        {
            // We cannot check for the correct suppression nicely.
            // DiagnosticWithProgrammaticSuppression is a private class.
            // ProgrammaticSuppressionInfo is an internal class.
            // Resorting to reflection ...
            Type errorType = error.GetType();
            Assert.That(errorType.Name, Is.EqualTo("DiagnosticWithProgrammaticSuppression"));

            var programmaticSuppressionInfoProperty = errorType.GetProperty("ProgrammaticSuppressionInfo",
                                                                            BindingFlags.Instance | BindingFlags.NonPublic);
            if (programmaticSuppressionInfoProperty == null)
            {
                Assert.Fail("Expected a property with the name 'ProgrammaticSuppressionInfo'");
                return;
            }

            var programmaticSuppressionInfo = programmaticSuppressionInfoProperty.GetValue(error);

            if (programmaticSuppressionInfo != null)
            {
                Type programmaticSuppressionInfoType = programmaticSuppressionInfo.GetType();
                Assert.That(programmaticSuppressionInfoType.Name, Is.EqualTo("ProgrammaticSuppressionInfo"));

                var suppressionsProperty = programmaticSuppressionInfoType.GetProperty("Suppressions");
                if (suppressionsProperty != null)
                {
                    var suppressions = suppressionsProperty.GetValue(programmaticSuppressionInfo);

                    if (suppressions is ImmutableHashSet<(string Id, LocalizableString Justification)> suppressionsHashSet)
                    {
                        Assert.That(suppressionsHashSet, Does.Contain((suppressionDescriptor.Id, suppressionDescriptor.Justification)));
                    }
                }
            }
        }
    }
}
