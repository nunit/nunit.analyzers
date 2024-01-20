using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace NUnit.Analyzers.Helpers
{
    internal static class DiagnosticExtensions
    {
        private static readonly PropertyInfo? argumentsProperty = typeof(Diagnostic).GetProperty("Arguments", BindingFlags.Instance | BindingFlags.NonPublic);

        public static object[] Arguments(this Diagnostic diagnostic)
        {
            return (object[]?)argumentsProperty?.GetValue(diagnostic) ?? Array.Empty<object>();
        }
    }
}
