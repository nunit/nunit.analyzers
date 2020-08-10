using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using static NUnit.Analyzers.Constants.NunitFrameworkConstants;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClassicModelAssertUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor isTrueDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsTrueUsage,
            title: ClassicModelUsageAnalyzerConstants.IsTrueTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsTrueMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsTrueDescription);

        private static readonly DiagnosticDescriptor trueDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.TrueUsage,
            title: ClassicModelUsageAnalyzerConstants.TrueTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.TrueMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.TrueDescription);

        private static readonly DiagnosticDescriptor isFalseDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsFalseUsage,
            title: ClassicModelUsageAnalyzerConstants.IsFalseTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsFalseMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsFalseDescription);

        private static readonly DiagnosticDescriptor falseDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.FalseUsage,
            title: ClassicModelUsageAnalyzerConstants.FalseTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.FalseMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.FalseDescription);

        private static readonly DiagnosticDescriptor areEqualDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.AreEqualUsage,
            title: ClassicModelUsageAnalyzerConstants.AreEqualTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.AreEqualMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.AreEqualDescription);

        private static readonly DiagnosticDescriptor areNotEqualDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.AreNotEqualUsage,
            title: ClassicModelUsageAnalyzerConstants.AreNotEqualTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.AreNotEqualMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.AreNotEqualDescription);

        private static readonly DiagnosticDescriptor areSameDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.AreSameUsage,
            title: ClassicModelUsageAnalyzerConstants.AreSameTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.AreSameMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.AreSameDescription);

        private static readonly DiagnosticDescriptor isNullDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsNullUsage,
            title: ClassicModelUsageAnalyzerConstants.IsNullTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsNullMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsNullDescription);

        private static readonly DiagnosticDescriptor nullDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.NullUsage,
            title: ClassicModelUsageAnalyzerConstants.NullTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.NullMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.NullDescription);

        private static readonly DiagnosticDescriptor isNotNullDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsNotNullUsage,
            title: ClassicModelUsageAnalyzerConstants.IsNotNullTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsNotNullMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsNotNullDescription);

        private static readonly DiagnosticDescriptor notNullDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.NotNullUsage,
            title: ClassicModelUsageAnalyzerConstants.NotNullTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.NotNullMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.NotNullDescription);

        private static readonly DiagnosticDescriptor greaterDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.GreaterUsage,
            title: ClassicModelUsageAnalyzerConstants.GreaterTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.GreaterMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.GreaterDescription);

        private static readonly DiagnosticDescriptor greaterOrEqualDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.GreaterOrEqualUsage,
            title: ClassicModelUsageAnalyzerConstants.GreaterOrEqualTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.GreaterOrEqualMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.GreaterOrEqualDescription);

        private static readonly DiagnosticDescriptor lessDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.LessUsage,
            title: ClassicModelUsageAnalyzerConstants.LessTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.LessMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.LessDescription);

        private static readonly DiagnosticDescriptor lessOrEqualDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.LessOrEqualUsage,
            title: ClassicModelUsageAnalyzerConstants.LessOrEqualTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.LessOrEqualMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.LessOrEqualDescription);

        private static readonly DiagnosticDescriptor areNotSameDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.AreNotSameUsage,
            title: ClassicModelUsageAnalyzerConstants.AreNotSameTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.AreNotSameMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: ClassicModelUsageAnalyzerConstants.AreNotSameDescription);

        private static readonly DiagnosticDescriptor zeroDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ZeroUsage,
            title: ClassicModelUsageAnalyzerConstants.ZeroTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.ZeroMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.ZeroDescription);

        private static readonly DiagnosticDescriptor notZeroDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.NotZeroUsage,
            title: ClassicModelUsageAnalyzerConstants.NotZeroTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.NotZeroMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.NotZeroDescription);

        private static readonly DiagnosticDescriptor isNaNDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsNaNUsage,
            title: ClassicModelUsageAnalyzerConstants.IsNaNTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsNaNMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsNaNDescription);

        private static readonly DiagnosticDescriptor isEmptyDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsEmptyUsage,
            title: ClassicModelUsageAnalyzerConstants.IsEmptyTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsEmptyMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsEmptyDescription);

        private static readonly DiagnosticDescriptor isNotEmptyDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsNotEmptyUsage,
            title: ClassicModelUsageAnalyzerConstants.IsNotEmptyTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsNotEmptyMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsNotEmptyDescription);

        private static readonly DiagnosticDescriptor containsDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.ContainsUsage,
            title: ClassicModelUsageAnalyzerConstants.ContainsTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.ContainsMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.ContainsDescription);

        private static readonly DiagnosticDescriptor isInstanceOfDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsInstanceOfUsage,
            title: ClassicModelUsageAnalyzerConstants.IsInstanceOfTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsInstanceOfMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsInstanceOfDescription);

        private static readonly DiagnosticDescriptor isNotInstanceOfDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsNotInstanceOfUsage,
            title: ClassicModelUsageAnalyzerConstants.IsNotInstanceOfTitle,
            messageFormat: ClassicModelUsageAnalyzerConstants.IsNotInstanceOfMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: ClassicModelUsageAnalyzerConstants.IsNotInstanceOfDescription);

        private static readonly ImmutableDictionary<string, DiagnosticDescriptor> name =
          new Dictionary<string, DiagnosticDescriptor>
          {
              { NameOfAssertIsTrue, isTrueDescriptor },
              { NameOfAssertTrue, trueDescriptor },
              { NameOfAssertIsFalse, isFalseDescriptor },
              { NameOfAssertFalse, falseDescriptor },
              { NameOfAssertAreEqual, areEqualDescriptor },
              { NameOfAssertAreNotEqual, areNotEqualDescriptor },
              { NameOfAssertAreSame, areSameDescriptor },
              { NameOfAssertIsNull, isNullDescriptor },
              { NameOfAssertNull, nullDescriptor },
              { NameOfAssertIsNotNull, isNotNullDescriptor },
              { NameOfAssertNotNull, notNullDescriptor },
              { NameOfAssertGreater, greaterDescriptor },
              { NameOfAssertGreaterOrEqual, greaterOrEqualDescriptor },
              { NameOfAssertLess, lessDescriptor },
              { NameOfAssertLessOrEqual, lessOrEqualDescriptor },
              { NameOfAssertAreNotSame, areNotSameDescriptor },
              { NameOfAssertZero, zeroDescriptor },
              { NameOfAssertNotZero, notZeroDescriptor },
              { NameOfAssertIsNaN, isNaNDescriptor },
              { NameOfAssertIsEmpty, isEmptyDescriptor },
              { NameOfAssertIsNotEmpty, isNotEmptyDescriptor },
              { NameOfAssertContains, containsDescriptor },
              { NameOfAssertIsInstanceOf, isInstanceOfDescriptor },
              { NameOfAssertIsNotInstanceOf, isNotInstanceOfDescriptor },
          }.ToImmutableDictionary();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ClassicModelAssertUsageAnalyzer.name.Values.ToImmutableArray();

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            var methodSymbol = assertOperation.TargetMethod;

            if (ClassicModelAssertUsageAnalyzer.name.ContainsKey(methodSymbol.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ClassicModelAssertUsageAnalyzer.name[methodSymbol.Name],
                    assertOperation.Syntax.GetLocation(),
                    ClassicModelAssertUsageAnalyzer.GetProperties(methodSymbol)));
            }
        }

        private static ImmutableDictionary<string, string> GetProperties(IMethodSymbol invocationSymbol)
        {
            return new Dictionary<string, string>
            {
                { AnalyzerPropertyKeys.ModelName, invocationSymbol.Name },
                { AnalyzerPropertyKeys.HasToleranceValue,
                    (invocationSymbol.Name == NameOfAssertAreEqual &&
                        invocationSymbol.Parameters.Length >= 3 &&
                        invocationSymbol.Parameters[2].Type.SpecialType == SpecialType.System_Double).ToString() },
                { AnalyzerPropertyKeys.IsGenericMethod, invocationSymbol.IsGenericMethod.ToString() },
            }.ToImmutableDictionary();
        }
    }
}
