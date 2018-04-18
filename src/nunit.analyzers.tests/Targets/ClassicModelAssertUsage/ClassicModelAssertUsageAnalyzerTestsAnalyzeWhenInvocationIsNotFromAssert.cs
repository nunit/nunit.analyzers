namespace NUnit.Analyzers.Tests.Targets.ClassicModelAssertUsage
{
    public sealed class ClassicModelAssertUsageAnalyzerTestsAnalyzeWhenInvocationIsNotFromAssert
    {
        public void Test()
        {
            Assert.AreEqual(3, 4);
        }

        private static class Assert
        {
            public static bool AreEqual(int a, int b);
        }
    }
}
