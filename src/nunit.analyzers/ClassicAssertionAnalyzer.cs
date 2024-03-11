using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers
{
    public abstract class ClassicAssertionAnalyzer : BaseAssertionAnalyzer
    {
        protected override bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation)
        {
            INamedTypeSymbol containingType = invocationOperation.TargetMethod.ContainingType;

            return containingType.IsAssert();
        }
    }
}
