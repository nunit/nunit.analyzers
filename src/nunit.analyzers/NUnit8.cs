using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers
{
    public class NUnit8
    {
        public const string Id = "NUNIT_8";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantAnalyzerTitle,
            "Consider using nameof({0}) instead of \"{0}\"",
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);
    }
}
