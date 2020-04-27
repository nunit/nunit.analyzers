using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.TestCaseSourceUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestCaseSourceUsesStringAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor missingSourceDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceIsMissing,
            title: "TestCaseSource argument does not specify an existing member.",
            messageFormat: "TestCaseSource argument '{0}' does not specify an existing member.",
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: "TestCaseSource argument does not specify an existing member. This will lead to an error at run-time.");

        private static readonly DiagnosticDescriptor considerNameOfDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceStringUsage,
            title: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantAnalyzerTitle,
            messageFormat: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: TestCaseSourceUsageConstants.ConsiderNameOfInsteadOfStringConstantDescription);

        private static readonly DiagnosticDescriptor sourceTypeNotIEnumerableDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceSourceTypeNotIEnumerable,
            title: TestCaseSourceUsageConstants.SourceTypeNotIEnumerableTitle,
            messageFormat: TestCaseSourceUsageConstants.SourceTypeNotIEnumerableMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.SourceTypeNotIEnumerableDescription);

        private static readonly DiagnosticDescriptor sourceTypeNoDefaultConstructorDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceSourceTypeNoDefaultConstructor,
            title: TestCaseSourceUsageConstants.SourceTypeNoDefaultConstructorTitle,
            messageFormat: TestCaseSourceUsageConstants.SourceTypeNoDefaultConstructorMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.SourceTypeNoDefaultConstructorDescription);

        private static readonly DiagnosticDescriptor sourceNotStaticDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceSourceIsNotStatic,
            title: TestCaseSourceUsageConstants.SourceIsNotStaticTitle,
            messageFormat: TestCaseSourceUsageConstants.SourceIsNotStaticMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.SourceIsNotStaticDescription);

        private static readonly DiagnosticDescriptor mismatchInNumberOfParameters = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TestCaseSourceMismatchInNumberOfParameters,
            title: TestCaseSourceUsageConstants.MismatchInNumberOfParametersTitle,
            messageFormat: TestCaseSourceUsageConstants.MismatchInNumberOfParametersMessage,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            description: TestCaseSourceUsageConstants.MismatchInNumberOfParametersDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            considerNameOfDescriptor,
            missingSourceDescriptor,
            sourceTypeNotIEnumerableDescriptor,
            sourceTypeNoDefaultConstructorDescriptor,
            sourceNotStaticDescriptor,
            mismatchInNumberOfParameters);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(x => AnalyzeAttribute(x), SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var testCaseSourceType = context.SemanticModel.Compilation.GetTypeByMetadataName(NunitFrameworkConstants.FullNameOfTypeTestCaseSourceAttribute);
            if (testCaseSourceType == null)
            {
                return;
            }

            var attributeNode = (AttributeSyntax)context.Node;
            var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeNode).Symbol;

            if (testCaseSourceType.ContainingAssembly.Identity == attributeSymbol?.ContainingAssembly.Identity &&
                NunitFrameworkConstants.NameOfTestCaseSourceAttribute == attributeSymbol?.ContainingType.Name)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var attributeInfo = ExtractInfoFromAttribute(context, attributeNode);

                if (attributeInfo == null)
                {
                    return;
                }

                var stringConstant = attributeInfo.SourceName;
                var syntaxNode = attributeInfo.SyntaxNode;

                if (stringConstant is null && attributeNode.ArgumentList.Arguments.Count == 1)
                {
                    // The Type argument in this form represents the class that provides test cases.
                    // It must have a default constructor and implement IEnumerable.
                    var sourceType = attributeInfo.SourceType;
                    bool typeImplementsIEnumerable = sourceType.IsIEnumerable(out var _);
                    bool typeHasDefaultConstructor = sourceType.Constructors.Any(c => c.Parameters.IsEmpty);
                    if (!typeImplementsIEnumerable)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            sourceTypeNotIEnumerableDescriptor,
                            attributeNode.ArgumentList.Arguments[0].GetLocation(),
                            sourceType.Name));
                    }
                    else if (!typeHasDefaultConstructor)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            sourceTypeNoDefaultConstructorDescriptor,
                            attributeNode.ArgumentList.Arguments[0].GetLocation(),
                            sourceType.Name));
                    }

                    return;
                }

                var symbol = GetMember(context, attributeInfo);
                if (symbol is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                       missingSourceDescriptor,
                       syntaxNode.GetLocation(),
                       stringConstant));
                }
                else
                {
                    if (attributeInfo.IsStringLiteral)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            considerNameOfDescriptor,
                            syntaxNode.GetLocation(),
                            stringConstant));
                    }

                    if (!symbol.IsStatic)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            sourceNotStaticDescriptor,
                            syntaxNode.GetLocation(),
                            stringConstant));
                    }

                    switch (symbol)
                    {
                        case IPropertySymbol property:
                            ReportIfSymbolNotIEnumerable(property.Type);
                            ReportIfParametersSupplied(attributeInfo.NumberOfMethodParameters);
                            break;
                        case IFieldSymbol field:
                            ReportIfSymbolNotIEnumerable(field.Type);
                            ReportIfParametersSupplied(attributeInfo.NumberOfMethodParameters);
                            break;
                        case IMethodSymbol method:
                            ReportIfSymbolNotIEnumerable(method.ReturnType);

                            if (method.Parameters.Length != attributeInfo.NumberOfMethodParameters)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(
                                    mismatchInNumberOfParameters,
                                    syntaxNode.GetLocation(),
                                    attributeInfo.NumberOfMethodParameters ?? 0,
                                    method.Parameters.Length));
                            }

                            break;
                    }
                }
            }
        }

        private static void ReportIfSymbolNotIEnumerable(ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsIEnumerable(out var _))
            {
                // TODO Report not IEnumerable
            }
        }

        private static void ReportIfParametersSupplied(int? numberOfMethodParameters)
        {
            if (numberOfMethodParameters > 0)
            {
                // TODO Report parameters passed to field or property
            }
        }

        private static SourceAttributeInformation ExtractInfoFromAttribute(
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
            if (firstArgumentExpression is TypeOfExpressionSyntax typeofSyntax)
            {
                var sourceType = context.SemanticModel.GetSymbolInfo(typeofSyntax.Type).Symbol as INamedTypeSymbol;
                return ExtractElementsInAttribute(context, sourceType, positionalArguments, 1);
            }
            else
            {
                var sourceType = context.ContainingSymbol.ContainingType;
                return ExtractElementsInAttribute(context, sourceType, positionalArguments, 0);
            }
        }

        private static SourceAttributeInformation ExtractElementsInAttribute(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol sourceType,
            ImmutableArray<AttributeArgumentSyntax> positionalArguments,
            int sourceNameIndex)
        {
            if (sourceType == null)
            {
                return null;
            }

            SyntaxNode syntaxNode = null;
            string sourceName = null;
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
                numMethodParams = GetNumberOfParametersToMethod(positionalArguments[1]);
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
            Optional<object> possibleConstant = context.SemanticModel.GetConstantValue(argumentSyntax?.Expression);

            if (possibleConstant.HasValue && possibleConstant.Value is string stringConstant)
            {
                SyntaxNode syntaxNode = argumentSyntax?.Expression;
                bool isStringLiteral = syntaxNode is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression);

                return (syntaxNode, stringConstant, isStringLiteral);
            }

            return null;
        }

        private static int? GetNumberOfParametersToMethod(AttributeArgumentSyntax attributeArgumentSyntax)
        {
            var lastExpression = attributeArgumentSyntax?.Expression as ArrayCreationExpressionSyntax;
            return lastExpression?.Initializer.Expressions.Count;
        }

        private static ISymbol GetMember(SyntaxNodeAnalysisContext context, SourceAttributeInformation attributeInformation)
        {
            if (!SyntaxFacts.IsValidIdentifier(attributeInformation.SourceName))
            {
                return null;
            }

            var symbols = context.SemanticModel.LookupSymbols(
                attributeInformation.SyntaxNode.SpanStart,
                container: attributeInformation.SourceType,
                name: attributeInformation.SourceName);

            foreach (var symbol in symbols)
            {
                switch (symbol.Kind)
                {
                    case SymbolKind.Field:
                    case SymbolKind.Property:
                    case SymbolKind.Method:
                        return symbol;
                }
            }

            return null;
        }
    }
}
