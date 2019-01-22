namespace NUnit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using NUnit.Analyzers.Constants;

    public class NUnit6
    {
        public const string Id = "NUNIT_6";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            ClassicModelUsageAnalyzerConstants.Title,
            ClassicModelUsageAnalyzerConstants.Message,
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);
    }
}