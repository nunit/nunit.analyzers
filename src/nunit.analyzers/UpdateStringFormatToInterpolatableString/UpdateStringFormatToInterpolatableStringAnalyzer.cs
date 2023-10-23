using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.UpdateStringFormatToInterpolatableString
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UpdateStringFormatToInterpolatableStringAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly string[] ObsoleteParamsMethods =
        {
            NUnitFrameworkConstants.NameOfAssertPass,
            NUnitFrameworkConstants.NameOfAssertFail,
            NUnitFrameworkConstants.NameOfAssertWarn,
            NUnitFrameworkConstants.NameOfAssertIgnore,
            NUnitFrameworkConstants.NameOfAssertInconclusive,
            NUnitFrameworkConstants.NameOfAssertThat,
        };

        private static readonly DiagnosticDescriptor updateStringFormatToInterpolatableString = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.UpdateStringFormatToInterpolatableString,
            title: UpdateStringFormatToInterpolatableStringConstants.UpdateStringFormatToInterpolatableStringTitle,
            messageFormat: UpdateStringFormatToInterpolatableStringConstants.UpdateStringFormatToInterpolatableStringMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: UpdateStringFormatToInterpolatableStringConstants.UpdateStringFormatToInterpolatableStringDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(updateStringFormatToInterpolatableString);

        protected override bool IsAssert(bool hasClassicAssert, IInvocationOperation invocationOperation)
        {
            return invocationOperation.TargetMethod.ContainingType.IsAssert();
        }

        protected override void AnalyzeAssertInvocation(Version nunitVersion, OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (nunitVersion.Major >= 4)
            {
                // Too late, this won't work as the method with the `params` parameter doesn't exists
                // and won't be resolved by the compiler.
                return;
            }

            int lastParameterIndex = assertOperation.TargetMethod.Parameters.Length - 1;
            if (lastParameterIndex > 0 && assertOperation.TargetMethod.Parameters[lastParameterIndex].IsParams)
            {
                IArgumentOperation lastArgument = assertOperation.Arguments[lastParameterIndex];
                if (IsNonEmptyParamsArrayArgument(lastArgument))
                {
                    string methodName = assertOperation.TargetMethod.Name;

                    if (!ObsoleteParamsMethods.Contains(methodName))
                    {
                        return;
                    }

                    int minimumNumberOfArguments = lastParameterIndex - 1;

                    context.ReportDiagnostic(Diagnostic.Create(
                        updateStringFormatToInterpolatableString,
                        assertOperation.Syntax.GetLocation(),
                        new Dictionary<string, string?>
                        {
                            [AnalyzerPropertyKeys.ModelName] = methodName,
                            [AnalyzerPropertyKeys.MinimumNumberOfArguments] = minimumNumberOfArguments.ToString(CultureInfo.InvariantCulture),
                        }.ToImmutableDictionary()));
                }
            }

            static bool IsNonEmptyParamsArrayArgument(IArgumentOperation argument)
            {
                if (argument.ArgumentKind == ArgumentKind.ParamArray)
                {
                    var value = (IArrayCreationOperation)argument.Value;
                    return value.Initializer is not null && value.Initializer.ElementValues.Length > 0;
                }

                // If it is a reference to an array variable, it is also no good.
                return argument.ArgumentKind == ArgumentKind.Explicit;
            }
        }
    }
}
