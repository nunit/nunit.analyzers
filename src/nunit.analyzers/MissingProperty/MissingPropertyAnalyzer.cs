using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

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
            defaultSeverity: DiagnosticSeverity.Warning,
            description: MissingPropertyConstants.Description);

        private static readonly string[] implicitPropertyConstraints = new[]
        {
            NunitFrameworkConstants.NameOfHasCount,
            NunitFrameworkConstants.NameOfHasLength,
            NunitFrameworkConstants.NameOfHasMessage,
            NunitFrameworkConstants.NameOfHasInnerException,
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
                if (helperClassName != null && helperClassName != NunitFrameworkConstants.NameOfHas)
                {
                    return;
                }

                if (constraintPart.Prefixes.Count == 0)
                    continue;

                // Only first prefix supported (as preceding prefixes might change validated type)
                var prefix = constraintPart.Prefixes.First();
                var propertyName = TryGetRequiredPropertyName(prefix);

                if (propertyName == null)
                    continue;

                var actualType = AssertHelper.GetUnwrappedActualType(actualOperation);

                if (actualType == null
                    || actualType.TypeKind == TypeKind.Error
                    || actualType.TypeKind == TypeKind.Dynamic
                    || actualType.SpecialType == SpecialType.System_Object)
                {
                    continue;
                }

                var propertyMembers = actualType.GetAllMembers().Where(m => m.Kind == SymbolKind.Property);
                if (!propertyMembers.Any(m => m.Name == propertyName))
                {
                    var properties = propertyMembers.Select(p => p.Name).Distinct().ToImmutableDictionary(p => p, null);

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
                && prefixName == NunitFrameworkConstants.NameOfHasProperty
                && invocationOperation.Arguments.Length == 1)
            {
                // Get constant value from constraint argument (e.g. Has.Property("PropertyName"))
                var argument = invocationOperation.Arguments[0].Value;

                return argument.ConstantValue.Value as string;
            }

            return null;
        }
    }
}
