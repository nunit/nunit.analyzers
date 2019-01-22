namespace NUnit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using NUnit.Analyzers.Constants;

    public class NUnit5
    {
        public const string Id = "NUNIT_5";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            ClassicModelUsageAnalyzerConstants.Title,
            ClassicModelUsageAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);
    }
}