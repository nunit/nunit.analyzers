using Microsoft.CodeAnalysis.Diagnostics;

namespace NUnit.Analyzers.DiagnosticSuppressors
{
    /// <summary>
    /// Purely exists as a base class to find our <see cref="DiagnosticSuppressor"/> for documentation verification.
    /// </summary>
    public abstract class BaseNUnitDiagnosticSuppressor : DiagnosticSuppressor
    {
    }
}
