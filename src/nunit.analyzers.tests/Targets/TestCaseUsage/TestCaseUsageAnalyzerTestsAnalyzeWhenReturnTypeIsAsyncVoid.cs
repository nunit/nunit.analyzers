using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.Targets.TestCaseUsage
{
    class TestCaseUsageAnalyzerTestsAnalyzeWhenReturnTypeIsAsyncVoid
    {
        [Test]
        public async void AsyncVoid()
        {
            await Task.Delay(1); // To avoid warning message
        }
    }
}
