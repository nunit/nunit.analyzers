using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
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

        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            var semanticModel = context.SemanticModel;

            if (!AssertHelper.TryGetActualAndConstraintExpressions(assertExpression, semanticModel,
                out var actualExpression, out var constraintExpression))
            {
                return;
            }

            foreach (var constraintPart in constraintExpression.ConstraintParts)
            {
                // Only 'Has' allowed (or none) - e.g. 'Throws' leads to verification on exception, which is not supported here. 
                var helperClassName = constraintPart.GetHelperClassName();
                if (helperClassName != null && helperClassName != NunitFrameworkConstants.NameOfHas)
                {
                    return;
                }

                if (constraintPart.PrefixExpressions.Count == 0)
                    continue;

                // Only first prefix supported (as preceding prefixes might change validated type)
                var prefix = constraintPart.PrefixExpressions.First();
                var propertyName = TryGetRequiredPropertyName(prefix, semanticModel);

                if (propertyName == null)
                    continue;

                var actualSymbol = semanticModel.GetTypeInfo(actualExpression).ConvertedType;

                if (actualSymbol == null)
                    continue;

                actualSymbol = AssertHelper.UnwrapActualType(actualSymbol);

                if (actualSymbol.TypeKind == TypeKind.Error
                    || actualSymbol.TypeKind == TypeKind.Dynamic
                    || actualSymbol.SpecialType == SpecialType.System_Object)
                {
                    continue;
                }

                if (!actualSymbol.GetAllMembers().Any(m => m.Kind == SymbolKind.Property && m.Name == propertyName))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        prefix.GetLocation(),
                        actualSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), propertyName));
                }
            }
        }

        private static string TryGetRequiredPropertyName(ExpressionSyntax prefix, SemanticModel semanticModel)
        {
            var prefixName = prefix.GetName();

            if (prefix is MemberAccessExpressionSyntax && implicitPropertyConstraints.Contains(prefixName))
            {
                return prefixName;
            }
            else if (prefix is InvocationExpressionSyntax invocationPrefix
                && prefixName == NunitFrameworkConstants.NameOfHasProperty
                && invocationPrefix.ArgumentList.Arguments.Count == 1)
            {
                // Get constant value from constraint argument (e.g. Has.Property("PropertyName"))
                var argument = invocationPrefix.ArgumentList.Arguments[0].Expression;
                var operation = semanticModel.GetOperation(argument);

                return operation.ConstantValue.Value as string;
            }

            return null;
        }
    }
}
