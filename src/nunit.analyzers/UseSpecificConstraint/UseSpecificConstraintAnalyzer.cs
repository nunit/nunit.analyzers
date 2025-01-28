using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.UseSpecificConstraint
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseSpecificConstraintAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor simplifyConstraint = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.UseSpecificConstraint,
            title: UseSpecificConstraintConstants.UseSpecificConstraintTitle,
            messageFormat: UseSpecificConstraintConstants.UseSpecificConstraintMessage,
            category: Categories.Style,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: UseSpecificConstraintConstants.UseSpecificConstraintDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(simplifyConstraint);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(this.AnalyzeCompilationStart);
        }

        private static void AnalyzeInvocation(Version nunitVersion, SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            if (invocationExpression.ArgumentList.Arguments.Count == 1 &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                memberAccessExpression.Name.Identifier.Text == NUnitFrameworkConstants.NameOfIsEqualTo)
            {
                ExpressionSyntax argument = invocationExpression.ArgumentList.Arguments[0].Expression;
                string? constraint = null;

                if (argument is LiteralExpressionSyntax literalExpression)
                {
                    constraint = literalExpression.Kind() switch
                    {
                        SyntaxKind.NullLiteralExpression => NUnitFrameworkConstants.NameOfIsNull,
                        SyntaxKind.FalseLiteralExpression => NUnitFrameworkConstants.NameOfIsFalse,
                        SyntaxKind.TrueLiteralExpression => NUnitFrameworkConstants.NameOfIsTrue,
                        _ => null,
                    };

                    if (constraint is null && nunitVersion.Major >= 4)
                    {
                        constraint = literalExpression.Kind() switch
                        {
                            SyntaxKind.DefaultLiteralExpression => NUnitV4FrameworkConstants.NameOfIsDefault,
                            _ => constraint,
                        };
                    }
                }
                else if (argument is DefaultExpressionSyntax defaultExpression)
                {
                    if (defaultExpression.Type is PredefinedTypeSyntax predefinedType)
                    {
                        if (predefinedType.Keyword.IsKind(SyntaxKind.ObjectKeyword) ||
                            predefinedType.Keyword.IsKind(SyntaxKind.StringKeyword))
                        {
                            constraint = NUnitFrameworkConstants.NameOfIsNull;
                        }
                        else if (nunitVersion.Major >= 4)
                        {
                            constraint = NUnitV4FrameworkConstants.NameOfIsDefault;
                        }
                        else if (predefinedType.Keyword.IsKind(SyntaxKind.BoolKeyword))
                        {
                            constraint = NUnitFrameworkConstants.NameOfIsFalse;
                        }
                    }
                }

                if (constraint is not null)
                {
                    var diagnostic = Diagnostic.Create(simplifyConstraint, invocationExpression.GetLocation(),
                        new Dictionary<string, string?>
                        {
                            [AnalyzerPropertyKeys.SpecificConstraint] = constraint,
                        }.ToImmutableDictionary(),
                        argument.ToString(), constraint);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            IEnumerable<AssemblyIdentity> referencedAssemblies = context.Compilation.ReferencedAssemblyNames;

            AssemblyIdentity? nunit = referencedAssemblies.FirstOrDefault(a =>
                a.Name.Equals(NUnitFrameworkConstants.NUnitFrameworkAssemblyName, StringComparison.OrdinalIgnoreCase));

            if (nunit is null)
            {
                // Who would use NUnit.Analyzers without NUnit?
                return;
            }

            context.RegisterSyntaxNodeAction((context) => AnalyzeInvocation(nunit.Version, context), SyntaxKind.InvocationExpression);
        }
    }
}
