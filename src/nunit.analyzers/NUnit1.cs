using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers
{
    public class NUnit1
    {
        public const string Id = "NUNIT_1";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            "Find Classic Assertion Usage",
            "Consider changing from the classic model for assertions to the constraint model instead.",
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);
    }
}
