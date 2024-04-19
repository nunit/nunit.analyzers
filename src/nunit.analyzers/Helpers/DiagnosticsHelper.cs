using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Helpers
{
    internal static class DiagnosticsHelper
    {
        public static bool LastArgumentIsNonParamsArray(ImmutableArray<IArgumentOperation> arguments)
        {
            // Find out if the 'params' argument is an existing array and not one created from a params creation.
            return arguments[arguments.Length - 1].ArgumentKind != ArgumentKind.ParamArray;
        }

        public static ImmutableDictionary<string, string?> GetProperties(string methodName, ImmutableArray<IArgumentOperation> arguments)
        {
            bool argsIsArray = LastArgumentIsNonParamsArray(arguments);
            return new Dictionary<string, string?>
            {
                [AnalyzerPropertyKeys.ModelName] = methodName,
                [AnalyzerPropertyKeys.ArgsIsArray] = argsIsArray ? "ArgsIsArray" : string.Empty,
            }.ToImmutableDictionary();
        }
    }
}
