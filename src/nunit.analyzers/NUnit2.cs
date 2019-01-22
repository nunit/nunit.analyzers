using Microsoft.CodeAnalysis;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers
{
    public class NUnit2
    {
        public const string Id = "NUNIT_2";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            Id,
            "Find Classic Assertion Usage",
            "Consider changing from the classic model for assertions to the constraint model instead.",
            Categories.Usage,
            DiagnosticSeverity.Warning,
            true);
    }
}
