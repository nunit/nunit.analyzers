using System;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    public sealed class TestCaseUsageAnalyzerTestsAnalyzeWhenAttributeIsNotInNUnit
    {
        [TestCase]
        public void ATest() { }

        private sealed class TestCaseAttribute
            : Attribute
        { }
    }
}
