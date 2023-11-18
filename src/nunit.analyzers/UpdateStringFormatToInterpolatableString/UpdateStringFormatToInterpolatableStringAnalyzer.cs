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
            category: Categories.Assertion,
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
            if (nunitVersion.Major < 4)
            {
                AnalyzeNUnit3AssertInvocation(context, assertOperation);
            }
            else
            {
                AnalyzeNUnit4AssertInvocation(context, assertOperation);
            }
        }

        /// <summary>
        /// This looks to see if the `params` overload is called.
        /// </summary>
        private static void AnalyzeNUnit3AssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            int lastParameterIndex = assertOperation.TargetMethod.Parameters.Length - 1;
            if (lastParameterIndex > 0 && assertOperation.TargetMethod.Parameters[lastParameterIndex].IsParams)
            {
                IArgumentOperation lastArgument = assertOperation.Arguments[lastParameterIndex];
                if (IsNonEmptyParamsArrayArgument(lastArgument))
                {
                    string methodName = assertOperation.TargetMethod.Name;

                    if (ObsoleteParamsMethods.Contains(methodName))
                    {
                        ReportDiagnostic(context, assertOperation, methodName, lastParameterIndex - 1);
                    }
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

        /// <summary>
        /// This looks to see if the `CallerMemberExpression` parameters are explicitly specified.
        /// </summary>
        private static void AnalyzeNUnit4AssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            string methodName = assertOperation.TargetMethod.Name;

            if (methodName != NUnitFrameworkConstants.NameOfAssertThat)
            {
                // Only Assert.That has CallerMemberExpression that could be accidentally used as format paramaters
                return;
            }

            // Find the 'message' parameter.
            ImmutableArray<IParameterSymbol> parameters = assertOperation.TargetMethod.Parameters;
            int formatParameterIndex = 1;
            for (;  formatParameterIndex < parameters.Length; formatParameterIndex++)
            {
                IParameterSymbol parameter = parameters[formatParameterIndex];
                if (parameter.IsOptional)
                {
                    if (parameter.Name == "message")
                        break;

                    // Overload with FormattableString or Func<string> overload
                    return;
                }
            }

            ImmutableArray<IArgumentOperation> arguments = assertOperation.Arguments;
            if (arguments.Length > formatParameterIndex && arguments[formatParameterIndex + 1].ArgumentKind == ArgumentKind.Explicit)
            {
                // The argument after the message is explicitly specified
                // Most likely the user thought it was using a format specification with a parameter.
                // Or it copied code from some NUnit 3.x source into an NUNit 4 project.
                ReportDiagnostic(context, assertOperation, methodName, formatParameterIndex);
            }
        }

        private static void ReportDiagnostic(
            OperationAnalysisContext context,
            IInvocationOperation assertOperation,
            string methodName,
            int minimumNumberOfArguments)
        {
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
}
