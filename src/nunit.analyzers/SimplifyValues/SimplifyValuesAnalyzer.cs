using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.SimplifyValues;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SimplifyValuesAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor simplifyValues = DiagnosticDescriptorCreator.Create(
        id: AnalyzerIdentifiers.SimplifyValues,
        title: SimplifyValuesAnalyzerConstants.SimplifyValuesTitle,
        messageFormat: SimplifyValuesAnalyzerConstants.SimplifyValuesMessage,
        category: Categories.Style,
        defaultSeverity: DiagnosticSeverity.Info,
        description: SimplifyValuesAnalyzerConstants.SimplifyValuesDescription);

    private static readonly ImmutableHashSet<string> nonCombinatorialAttributes = ImmutableHashSet.Create("SequentialAttribute", "PairwiseAttribute");

    /// <summary>
    /// Types for which NUnit's ValuesAttribute can deduce all values.
    /// </summary>
    private enum HandledType
    {
        Boolean,
        NullableBoolean,
        Enum,
        NullableEnum,
        NotHandled,
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(simplifyValues);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        var valuesType = context.Compilation.GetTypeByMetadataName(NUnitFrameworkConstants.FullNameOfTypeValuesAttribute);
        if (valuesType is null)
            return;

        context.RegisterSymbolAction(symbolContext => AnalyzeParameter(symbolContext, valuesType), SymbolKind.Parameter);
    }

    private static void AnalyzeParameter(SymbolAnalysisContext symbolContext, INamedTypeSymbol valuesType)
    {
        var parameterSymbol = symbolContext.Symbol as IParameterSymbol;
        if (parameterSymbol is null)
            return;

        var methodSymbol = parameterSymbol.ContainingSymbol as IMethodSymbol;
        if (methodSymbol is null)
            return;

        var methodAttributes = methodSymbol.GetAttributes();
        var methodHasNonCombinatorialAttribute = methodAttributes
            .Any(attribute => attribute.AttributeClass is { } attributeClass
                && StringComparer.OrdinalIgnoreCase.Equals(
                    attributeClass.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    "global::NUnit.Framework")
                && nonCombinatorialAttributes.Contains(attributeClass.Name));

        if (methodHasNonCombinatorialAttribute)
            return;

        // Now, check the type of the parameter decorated with the Values attribute and get all possible values.
        var parameterType = parameterSymbol.Type;
        var handledType = GetHandledCase(parameterType);
        if (handledType == HandledType.NotHandled)
            return;

        HashSet<object?>? allPossibleValues = handledType switch
        {
            HandledType.Boolean => new() { true, false },
            HandledType.NullableBoolean => new() { true, false, null },
            HandledType.Enum => GetAllPossibleEnumValuesOrDefault(parameterType),
            HandledType.NullableEnum => GetAllPossibleNullableEnumValuesOrDefault(parameterType),
            _ => null,
        };

        if (allPossibleValues is null)
            return;

        var valuesAttributes = parameterSymbol.GetAttributes()
            .Where(x => x.ApplicationSyntaxReference is not null
                && SymbolEqualityComparer.Default.Equals(x.AttributeClass, valuesType));
        foreach (var attribute in valuesAttributes)
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();

            HashSet<object?> argumentValues = new();
            var attributePositionalArguments = attribute.ConstructorArguments.AdjustArguments();
            for (var index = 0; index < attributePositionalArguments.Length; index++)
            {
                var argumentSyntax = attribute.GetAdjustedArgumentSyntax(
                    index,
                    attributePositionalArguments,
                    symbolContext.CancellationToken);
                if (argumentSyntax is null)
                    return;

                var constructorArgument = attributePositionalArguments[index];
                argumentValues.Add(constructorArgument.IsNull ? null : constructorArgument.Value);
            }

            // Use a set comparison, since the order doesn't matter.
            if (argumentValues.SetEquals(allPossibleValues))
            {
                var location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                if (location is not null)
                {
                    var diagnostic = Diagnostic.Create(simplifyValues, location);
                    symbolContext.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static HandledType GetHandledCase(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType == SpecialType.System_Boolean)
            return HandledType.Boolean;

        if (typeSymbol.TypeKind == TypeKind.Enum)
            return HandledType.Enum;

        if (TryGetTypeArgumentFromNullableType(typeSymbol, out var typeArgument))
        {
            if (typeArgument.SpecialType == SpecialType.System_Boolean)
                return HandledType.NullableBoolean;

            if (typeArgument.TypeKind == TypeKind.Enum)
                return HandledType.NullableEnum;
        }

        return HandledType.NotHandled;
    }

    private static bool TryGetTypeArgumentFromNullableType(ITypeSymbol typeSymbol, [NotNullWhen(true)] out ITypeSymbol? typeArgument)
    {
        if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            typeArgument = namedTypeSymbol.TypeArguments[0];
            return true;
        }

        typeArgument = null;
        return false;
    }

    private static HashSet<object?>? GetAllPossibleEnumValuesOrDefault(ITypeSymbol typeSymbol)
    {
        var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
        if (namedTypeSymbol is null)
            return null;

        return new(
            namedTypeSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Select(f => f.ConstantValue));
    }

    private static HashSet<object?>? GetAllPossibleNullableEnumValuesOrDefault(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && namedTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && namedTypeSymbol.TypeArguments.FirstOrDefault() is INamedTypeSymbol enumTypeSymbol)
        {
            var allPossibleNullableEnumValues = GetAllPossibleEnumValuesOrDefault(enumTypeSymbol);
            if (allPossibleNullableEnumValues is not null)
            {
                allPossibleNullableEnumValues.Add(null);
                return allPossibleNullableEnumValues;
            }
        }

        return null;
    }
}
