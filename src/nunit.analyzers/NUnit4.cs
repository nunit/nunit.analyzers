namespace NUnit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using NUnit.Analyzers.Constants;

    public class NUnit4
    {
        public const string Id = "NUNIT_4";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            ClassicModelUsageAnalyzerConstants.Title,
            ClassicModelUsageAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);
    }
}