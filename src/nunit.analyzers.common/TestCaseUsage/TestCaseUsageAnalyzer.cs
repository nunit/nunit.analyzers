using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.TestCaseUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestCaseUsageAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor notEnoughArguments = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseNotEnoughArgumentsUsage,
            title: TestCaseUsageAnalyzerConstants.NotEnoughArgumentsTitle,
            messageFormat: TestCaseUsageAnalyzerConstants.NotEnoughArgumentsMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseUsageAnalyzerConstants.NotEnoughArgumentsDescription);

        private static readonly DiagnosticDescriptor parameterTypeMismatch = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseParameterTypeMismatchUsage,
            title: TestCaseUsageAnalyzerConstants.ParameterTypeMismatchTitle,
            messageFormat: TestCaseUsageAnalyzerConstants.ParameterTypeMismatchMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseUsageAnalyzerConstants.ParameterTypeMismatchDescription);

        private static readonly DiagnosticDescriptor tooManyArguments = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseTooManyArgumentsUsage,
            title: TestCaseUsageAnalyzerConstants.TooManyArgumentsTitle,
            messageFormat: TestCaseUsageAnalyzerConstants.TooManyArgumentsMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseUsageAnalyzerConstants.TooManyArgumentsDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            notEnoughArguments,
            parameterTypeMismatch,
            tooManyArguments);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(TestCaseUsageAnalyzer.AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            var attributes = methodSymbol.GetAttributes();
            if (attributes.Length == 0)
                return;

            var testCaseType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeTestCaseAttribute);
            if (testCaseType is null)
                return;

            var testCaseAttributes = methodSymbol.GetAttributes()
                .Where(a => a.ApplicationSyntaxReference is not null
                    && SymbolEqualityComparer.Default.Equals(a.AttributeClass, testCaseType));

            foreach (var attribute in testCaseAttributes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var (methodRequiredParameters, methodOptionalParameters, methodParamsParameters) =
                    methodSymbol.GetParameterCounts();

                var attributePositionalArguments = AdjustArguments(attribute.ConstructorArguments);

                // From NUnit.Framework.TestCaseAttribute.GetParametersForTestCase
                // Special handling when sole method parameter is an object[].
                // If more than one argument - or one of a different type - is provided
                // then the argument is wrapped within a object[], as the only element,
                // hence the resulting code will always be valid.
                if (IsSoleParameterAnObjectArray(methodSymbol))
                {
                    if (attributePositionalArguments.Length > 1 ||
                        (attributePositionalArguments.Length == 1 && !IsTypeAnObjectArray(attributePositionalArguments[0].Type)))
                    {
                        return;
                    }
                }

                if (attributePositionalArguments.Length < methodRequiredParameters)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        notEnoughArguments,
                        attribute.ApplicationSyntaxReference.GetLocation(),
                        methodRequiredParameters,
                        attributePositionalArguments.Length));
                }
                else if (methodParamsParameters == 0 &&
                    attributePositionalArguments.Length > methodRequiredParameters + methodOptionalParameters)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        tooManyArguments,
                        attribute.ApplicationSyntaxReference.GetLocation(),
                        methodRequiredParameters + methodOptionalParameters,
                        attributePositionalArguments.Length));
                }
                else
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    TestCaseUsageAnalyzer.AnalyzePositionalArgumentsAndParameters(
                        context,
                        attribute,
                        attributePositionalArguments,
                        methodSymbol.Parameters);
                }
            }
        }

        private static ImmutableArray<TypedConstant> AdjustArguments(ImmutableArray<TypedConstant> attributePositionalArguments)
        {
            if (attributePositionalArguments.Length == 1
                && attributePositionalArguments[0] is { Kind: TypedConstantKind.Array } arrayArgument
                && arrayArgument.Type is IArrayTypeSymbol arrayType
                && arrayType.ElementType.SpecialType == SpecialType.System_Object)
            {
                // params overload, need to check array values instead

                if (arrayArgument.IsNull)
                {
                    // If null is provided - analyze it as single null value, not as null array
                    return ImmutableArray.Create(default(TypedConstant));
                }
                else
                {
                    return arrayArgument.Values;
                }
            }

            return attributePositionalArguments;
        }

        private static bool IsSoleParameterAnObjectArray(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Parameters.Length != 1)
                return false;

            var parameterType = methodSymbol.Parameters[0].Type;
            return IsTypeAnObjectArray(parameterType);
        }

        private static bool IsTypeAnObjectArray(ITypeSymbol? typeSymbol)
        {
            return typeSymbol is not null && typeSymbol.TypeKind == TypeKind.Array &&
                ((IArrayTypeSymbol)typeSymbol).ElementType.SpecialType == SpecialType.System_Object;
        }

        private static (ITypeSymbol type, string name, ITypeSymbol? paramsType) GetParameterType(
            ImmutableArray<IParameterSymbol> methodParameter,
            int position)
        {
            var symbol = position >= methodParameter.Length ?
                methodParameter[methodParameter.Length - 1] : methodParameter[position];

            ITypeSymbol type = symbol.Type;
            ITypeSymbol? paramsType = null;

            if (symbol.IsParams)
            {
                paramsType = ((IArrayTypeSymbol)symbol.Type).ElementType;
            }

            return (type, symbol.Name, paramsType);
        }

        private static void AnalyzePositionalArgumentsAndParameters(
            SymbolAnalysisContext context,
            AttributeData attribute,
            ImmutableArray<TypedConstant> attributePositionalArguments,
            ImmutableArray<IParameterSymbol> methodParameters)
        {
            for (var i = 0; i < attributePositionalArguments.Length; i++)
            {
                var attributeArgument = attributePositionalArguments[i];
                var (methodParameterType, methodParameterName, methodParameterParamsType) =
                    TestCaseUsageAnalyzer.GetParameterType(methodParameters, i);

                if (methodParameterType.IsTypeParameterAndDeclaredOnMethod())
                    continue;

                ITypeSymbol? argumentType = attributeArgument.Type;

                var argumentTypeMatchesParameterType = attributeArgument.CanAssignTo(
                    methodParameterType,
                    context.Compilation,
                    allowImplicitConversion: true,
                    allowEnumToUnderlyingTypeConversion: true);

                if (methodParameterParamsType is null && argumentTypeMatchesParameterType)
                    continue;

                if (methodParameterParamsType is not null)
                {
                    var argumentTypeMatchesElementType = attributeArgument.CanAssignTo(
                        methodParameterParamsType,
                        context.Compilation,
                        allowImplicitConversion: true,
                        allowEnumToUnderlyingTypeConversion: true);

                    if (argumentTypeMatchesElementType ||
                        (argumentTypeMatchesParameterType && (argumentType is not null || !methodParameterParamsType.IsValueType)))
                    {
                        continue;
                    }
                }

                var attributeArgumentSyntax = attribute.GetConstructorArgumentSyntax(i, context.CancellationToken);

                if (attributeArgumentSyntax is null)
                    continue;

                context.ReportDiagnostic(Diagnostic.Create(parameterTypeMismatch,
                    attributeArgumentSyntax.GetLocation(),
                    i,
                    argumentType?.ToDisplayString() ?? "<null>",
                    methodParameterName,
                    methodParameterType));

                context.CancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
