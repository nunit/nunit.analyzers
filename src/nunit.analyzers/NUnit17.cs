using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers
{
    public class NUnit17
    {
        public const string Id = "NUNIT_17";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            "TestCaseSource argument does not specify an existing member.",
            "TestCaseSource argument does not specify an existing member.",
            Categories.Usage,
            DiagnosticSeverity.Error,
            true);
    }
}
