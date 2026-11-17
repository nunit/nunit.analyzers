using Microsoft.CodeAnalysis.Testing;

namespace NUnit.Analyzers.Tests.Verifiers
{
    internal static class TestExtensions
    {
        /// <summary>
        /// Adds an .editorconfig with <paramref name="analyzerConfig"/> applied to all C# files.
        /// </summary>
        /// <example><c>test.TestState.AddAnalyzerConfig("dotnet_diagnostic.NUnit3003.search_all_code_for_use_as_data_source = true");</c>.</example>
        public static void AddAnalyzerConfig(this SolutionState state, string analyzerConfig)
        {
            state.AnalyzerConfigFiles.Add(("/.editorconfig", $"""
                root = true

                [*.cs]
                {analyzerConfig}

                """));
        }
    }
}
