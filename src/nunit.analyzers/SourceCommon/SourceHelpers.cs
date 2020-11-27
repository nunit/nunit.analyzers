using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.SourceCommon
{
    internal static class SourceHelpers
    {
        public static void ReportToUseNameOfIfApplicable(
            SyntaxNodeAnalysisContext context,
            SyntaxNode syntaxNode,
            SourceAttributeInformation attributeInfo,
            ISymbol symbol,
            string stringConstant,
            DiagnosticDescriptor considerNameOfDescriptor)
        {
            var sourceIsAccessible = context.SemanticModel.IsAccessible(
                syntaxNode.GetLocation().SourceSpan.Start,
                symbol);

            if (attributeInfo.IsStringLiteral && sourceIsAccessible)
            {
                var nameOfClassTarget = attributeInfo.SourceType.ToMinimalDisplayString(
                    context.SemanticModel,
                    syntaxNode.GetLocation().SourceSpan.Start);

                var nameOfTarget = SymbolEqualityComparer.Default.Equals(attributeInfo.SourceType, context.ContainingSymbol?.ContainingType)
                    ? stringConstant
                    : $"{nameOfClassTarget}.{stringConstant}";

                var properties = new Dictionary<string, string?>
                    {
                        { SourceCommonConstants.PropertyKeyNameOfTarget, nameOfTarget }
                    };

                context.ReportDiagnostic(Diagnostic.Create(
                    considerNameOfDescriptor,
                    syntaxNode.GetLocation(),
                    properties.ToImmutableDictionary(),
                    nameOfTarget,
                    stringConstant));
            }
        }

        public static SourceAttributeInformation? GetSourceAttributeInformation(
            SyntaxNodeAnalysisContext context,
            string fullyQualifiedMetadataName,
            string typeName)
        {
            var valueSourceType = context.SemanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
            if (valueSourceType == null)
            {
                return null;
            }

            var attributeNode = (AttributeSyntax)context.Node;
            var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;

            if (valueSourceType.ContainingAssembly.Identity == attributeSymbol?.ContainingAssembly.Identity &&
                typeName == attributeSymbol?.ContainingType.Name)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                return ExtractInfoFromAttribute(context, attributeNode);
            }

            return null;
        }

        public static ISymbol? GetMember(SourceAttributeInformation attributeInformation)
        {
            if (attributeInformation.SourceName is null || !SyntaxFacts.IsValidIdentifier(attributeInformation.SourceName))
            {
                return null;
            }

            return attributeInformation.SourceType.GetMember(attributeInformation.SourceName);
        }

        public static ISymbol? GetMember(this INamedTypeSymbol typeSymbol, string name)
        {
            ISymbol? symbol = typeSymbol
                .GetMembers(name)
                .FirstOrDefault(m => m.Kind == SymbolKind.Field
                    || m.Kind == SymbolKind.Property
                    || m.Kind == SymbolKind.Method);

            if (symbol is null && typeSymbol.BaseType != null)
            {
                ISymbol? baseSymbol = GetMember(typeSymbol.BaseType, name);

                if (baseSymbol?.DeclaredAccessibility != Accessibility.Private)
                {
                    symbol = baseSymbol;
                }
            }

            return symbol;
        }

        public static SourceAttributeInformation? ExtractInfoFromAttribute(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attributeSyntax)
        {
            var (positionalArguments, _) = attributeSyntax.GetArguments();

            if (positionalArguments.Length < 1)
            {
                return null;
            }

            var firstArgumentExpression = positionalArguments[0]?.Expression;
            if (firstArgumentExpression == null)
            {
                return null;
            }

            // TestCaseSourceAttribute has the following constructors:
            // * TestCaseSourceAttribute(Type sourceType)
            // * TestCaseSourceAttribute(Type sourceType, string sourceName)
            // * TestCaseSourceAttribute(Type sourceType, string sourceName, object?[]? methodParams)
            // * TestCaseSourceAttribute(string sourceName)
            // * TestCaseSourceAttribute(string sourceName, object?[]? methodParams)
            // and ValueSource has:
            // * ValueSource(Type sourceType, string sourceName)
            // * ValueSource(string sourceName)
            if (firstArgumentExpression is TypeOfExpressionSyntax typeofSyntax)
            {
                var sourceType = context.SemanticModel.GetSymbolInfo(typeofSyntax.Type).Symbol as INamedTypeSymbol;
                return ExtractElementsInAttribute(context, sourceType, positionalArguments, 1);
            }
            else
            {
                var sourceType = context.ContainingSymbol?.ContainingType;
                return ExtractElementsInAttribute(context, sourceType, positionalArguments, 0);
            }
        }

        private static SourceAttributeInformation? ExtractElementsInAttribute(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol? sourceType,
            ImmutableArray<AttributeArgumentSyntax> positionalArguments,
            int sourceNameIndex)
        {
            if (sourceType is null)
            {
                return null;
            }

            SyntaxNode? syntaxNode = null;
            string? sourceName = null;
            bool isStringLiteral = false;
            if (positionalArguments.Length > sourceNameIndex)
            {
                var syntaxNameAndType = GetSyntaxStringConstantAndType(context, positionalArguments, sourceNameIndex);

                if (syntaxNameAndType == null)
                {
                    return null;
                }

                (syntaxNode, sourceName, isStringLiteral) = syntaxNameAndType.Value;
            }

            int? numMethodParams = null;
            if (positionalArguments.Length > sourceNameIndex + 1)
            {
                numMethodParams = GetNumberOfParametersToMethod(positionalArguments[sourceNameIndex + 1]);
            }

            return new SourceAttributeInformation(sourceType, sourceName, syntaxNode, isStringLiteral, numMethodParams);
        }

        private static (SyntaxNode syntaxNode, string sourceName, bool isLiteral)? GetSyntaxStringConstantAndType(
            SyntaxNodeAnalysisContext context,
            ImmutableArray<AttributeArgumentSyntax> arguments,
            int index)
        {
            if (index >= arguments.Length)
            {
                return null;
            }

            var argumentSyntax = arguments[index];

            if (argumentSyntax == null)
            {
                return null;
            }

            Optional<object?> possibleConstant = context.SemanticModel.GetConstantValue(argumentSyntax.Expression);

            if (possibleConstant.HasValue && possibleConstant.Value is string stringConstant)
            {
                SyntaxNode syntaxNode = argumentSyntax.Expression;
                bool isStringLiteral = syntaxNode is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression);

                return (syntaxNode, stringConstant, isStringLiteral);
            }

            return null;
        }

        private static int? GetNumberOfParametersToMethod(AttributeArgumentSyntax attributeArgumentSyntax)
        {
            var lastExpression = attributeArgumentSyntax?.Expression as ArrayCreationExpressionSyntax;
            return lastExpression?.Initializer?.Expressions.Count;
        }
    }
}
