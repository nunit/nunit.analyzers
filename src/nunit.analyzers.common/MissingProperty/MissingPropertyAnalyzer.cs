using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

namespace NUnit.Analyzers.MissingProperty
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingPropertyAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.MissingProperty,
            title: MissingPropertyConstants.Title,
            messageFormat: MissingPropertyConstants.MessageFormat,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Error,
            description: MissingPropertyConstants.Description);

        private static readonly string[] implicitPropertyConstraints = new[]
        {
            NUnitFrameworkConstants.NameOfHasCount,
            NUnitFrameworkConstants.NameOfHasLength,
            NUnitFrameworkConstants.NameOfHasMessage,
            NUnitFrameworkConstants.NameOfHasInnerException,
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(descriptor);

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (!AssertHelper.TryGetActualAndConstraintOperations(assertOperation,
                out var actualOperation, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                // Only 'Has' allowed (or none) - e.g. 'Throws' leads to verification on exception, which is not supported here.
                var helperClassName = constraintPart.HelperClass?.Name;
                if (helperClassName is not null && helperClassName != NUnitFrameworkConstants.NameOfHas)
                {
                    return;
                }

                if (constraintPart.Prefixes.Count == 0)
                    continue;

                if (HasUnsupportedPrefixes(constraintPart))
                    return;

                // Only first prefix supported (as preceding prefixes might change validated type)
                var prefix = constraintPart.Prefixes.First();
                var propertyName = TryGetRequiredPropertyName(prefix);

                if (propertyName is null)
                    continue;

                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                if (actualType is null
                    || actualType.TypeKind == TypeKind.Error
                    || actualType.TypeKind == TypeKind.Dynamic
                    || actualType.SpecialType == SpecialType.System_Object)
                {
                    continue;
                }

                var propertyMembers = actualType.GetAllMembers().Where(m => m.Kind == SymbolKind.Property);
                if (!propertyMembers.Any(m => m.Name == propertyName))
                {
                    var properties = propertyMembers.Select(p => p.Name).Distinct().ToImmutableDictionary(p => p, p => (string?)p);

                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        prefix.Syntax.GetLocation(),
                        properties,
                        actualType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        propertyName));
                }
            }
        }

        private static string? TryGetRequiredPropertyName(IOperation prefix)
        {
            var prefixName = prefix.GetName();

            if (prefix is IPropertyReferenceOperation && implicitPropertyConstraints.Contains(prefixName))
            {
                return prefixName;
            }
            else if (prefix is IInvocationOperation invocationOperation
                && prefixName == NUnitFrameworkConstants.NameOfHasProperty
                && invocationOperation.Arguments.Length == 1)
            {
                // Get constant value from constraint argument (e.g. Has.Property("PropertyName"))
                var argument = invocationOperation.Arguments[0].Value;

                return argument.ConstantValue.Value as string;
            }

            return null;
        }

        private static bool HasUnsupportedPrefixes(ConstraintExpressionPart constraintPart)
        {
            // Disable analyzer if part has constraint prefixes other than property operators or Not operator,
            // as they might change validated type and lead to false positives (e.g. All/Some operators).
            return constraintPart.GetPrefixesNames().Any(prefix =>
                !implicitPropertyConstraints.Contains(prefix)
                && prefix != NUnitFrameworkConstants.NameOfHasProperty
                && prefix != NUnitFrameworkConstants.NameOfIsNot);
        }
    }
}
