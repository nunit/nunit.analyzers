using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers
{
    public class NUnit3
    {
        public const string Id = "NUNIT_3";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            ClassicModelUsageAnalyzerConstants.Title,
            ClassicModelUsageAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);
    }
}
