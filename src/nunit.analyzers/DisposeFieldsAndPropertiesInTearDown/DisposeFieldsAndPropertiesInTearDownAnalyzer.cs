using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.DisposeFieldsInTearDown
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposeFieldsAndPropertiesInTearDownAnalyzer : DiagnosticAnalyzer
    {
        // Methods that are considered to be Dispoing an instance.
        private static readonly ImmutableHashSet<string> DisposeMethods = ImmutableHashSet.Create(
            "Dispose",
            "DisposeAsync",
            "Close",
            "CloseAsync");

        // Types that even though they are IDisposable, don't need to be Disposed.
        private static readonly ImmutableHashSet<string> DisposableTypeNotRequiringToBeDisposed = ImmutableHashSet.Create(
            "System.Threading.Tasks.Task",
            "System.IO.MemoryStream",
            "System.IO.StringReader");

        private static readonly DiagnosticDescriptor fieldIsNotDisposedInTearDown = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.FieldIsNotDisposedInTearDown,
            title: DisposeFieldsAndPropertiesInTearDownConstants.FieldOrPropertyIsNotDisposedInTearDownTitle,
            messageFormat: DisposeFieldsAndPropertiesInTearDownConstants.FieldOrPropertyIsNotDisposedInTearDownMessageFormat,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DisposeFieldsAndPropertiesInTearDownConstants.FieldOrPropertyIsNotDisposedInTearDownDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(fieldIsNotDisposedInTearDown);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeDisposableFields, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeDisposableFields(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            SemanticModel? model = context.SemanticModel;

            var typeSymbol = model.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
            if (typeSymbol is null)
            {
                return;
            }

            if (typeSymbol.IsDisposable())
            {
                // If the type is Disposable, the CA2000 Analyzer will conflict, so bail out.
                return;
            }

            if (!typeSymbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsTestRelatedMethod(context.Compilation)))
            {
                // Not a TestFixture, CA1812 should have picked this up.
                return;
            }

            var fieldDeclarations = classDeclaration.Members
                                                    .OfType<FieldDeclarationSyntax>()
                                                    .Select(x => x.Declaration)
                                                    .SelectMany(x => x.Variables);

            var propertyDeclarations = classDeclaration.Members
                                                       .OfType<PropertyDeclarationSyntax>()
                                                       .Where(x => x.AccessorList is not null);

            HashSet<string> symbolsWithDisposableInitializers = new();

            Dictionary<string, SyntaxNode> symbols = new();
            foreach (var field in fieldDeclarations)
            {
                symbols.Add(field.Identifier.Text, field);
                if (field.Initializer is not null && NeedsDisposal(model, field.Initializer.Value))
                {
                    symbolsWithDisposableInitializers.Add(field.Identifier.Text);
                }
            }

            foreach (var property in propertyDeclarations)
            {
                symbols.Add(property.Identifier.Text, property);
                if (property.Initializer is not null && NeedsDisposal(model, property.Initializer.Value))
                    symbolsWithDisposableInitializers.Add(property.Identifier.Text);
            }

            HashSet<string> symbolNames = new HashSet<string>(symbols.Keys);

            ImmutableArray<ISymbol> members = typeSymbol.GetMembers();
            var methods = members.OfType<IMethodSymbol>().Where(m => !m.IsStatic).ToArray();
            var oneTimeTearDownMethods = methods.Where(m => HasAttribute(m, NUnitFrameworkConstants.NameOfOneTimeTearDownAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var oneTimeSetUpMethods = methods.Where(m => m.MethodKind == MethodKind.Constructor || HasAttribute(m, NUnitFrameworkConstants.NameOfOneTimeSetUpAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var setUpMethods = methods.Where(m => HasAttribute(m, NUnitFrameworkConstants.NameOfSetUpAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var tearDownMethods = methods.Where(m => HasAttribute(m, NUnitFrameworkConstants.NameOfTearDownAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

            var setUpAndTearDownMethods = oneTimeSetUpMethods.Union(oneTimeTearDownMethods).Union(setUpMethods).Union(tearDownMethods);
            var otherMethods = methods.Where(m => m.DeclaredAccessibility != Accessibility.Private && !setUpAndTearDownMethods.Contains(m));

            // Fields assigned in a OneTimeSetUp method must be disposed in a OneTimeTearDown method
            AnalyzeAssignedButNotDisposed(context, model, typeSymbol, symbols, symbolNames,
                NUnitFrameworkConstants.NameOfOneTimeTearDownAttribute, oneTimeSetUpMethods, oneTimeTearDownMethods, symbolsWithDisposableInitializers);

            // Fields assigned in a SetUp method must be disposed in a TearDown method
            AnalyzeAssignedButNotDisposed(context, model, typeSymbol, symbols, symbolNames,
                NUnitFrameworkConstants.NameOfTearDownAttribute, setUpMethods, tearDownMethods);

            // Fields assignd in any method, must be (conditionally) disposed in TearDown method.
            // If the field is disposed in the method itself, why is it a field?
            AnalyzeAssignedButNotDisposed(context, model, typeSymbol, symbols, symbolNames,
                NUnitFrameworkConstants.NameOfTearDownAttribute, otherMethods, tearDownMethods);
        }

        private static bool HasAttribute(IMethodSymbol method, string attributeName)
        {
            // Look for attribute not only on the method itself but
            // also on the base method in case this is an override.
            IEnumerable<AttributeData> attributes = Enumerable.Empty<AttributeData>();
            for (IMethodSymbol? declaredMethod = method;
                declaredMethod is not null;
                declaredMethod = declaredMethod.OverriddenMethod)
            {
                attributes = attributes.Concat(declaredMethod.GetAttributes());
            }

            return attributes.Any(x => x.AttributeClass?.Name == attributeName);
        }

        private static void AnalyzeAssignedButNotDisposed(
            SyntaxNodeAnalysisContext context,
            SemanticModel model,
            INamedTypeSymbol type,
            Dictionary<string, SyntaxNode> symbols,
            HashSet<string> names,
            string where,
            IEnumerable<IMethodSymbol> setUpMethods,
            IEnumerable<IMethodSymbol> tearDownMethods,
            HashSet<string>? assignedWithInitializers = null)
        {
            var assignedInSetUpMethods = AssignedIn(model, type, names, setUpMethods);
            var disposedInTearDownMethods = DisposedIn(model, type, names, tearDownMethods);

            if (assignedWithInitializers is not null)
                assignedInSetUpMethods.UnionWith(assignedWithInitializers);
            assignedInSetUpMethods.ExceptWith(disposedInTearDownMethods);

            foreach (var assignedButNotDisposed in assignedInSetUpMethods)
            {
                SyntaxNode syntaxNode = symbols[assignedButNotDisposed];

                context.ReportDiagnostic(Diagnostic.Create(
                    fieldIsNotDisposedInTearDown,
                    syntaxNode.GetLocation(),
                    syntaxNode is PropertyDeclarationSyntax ? "property" : "field",
                    assignedButNotDisposed,
                    where));
            }
        }

        #region AssignedIn

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, IEnumerable<IMethodSymbol> methods)
        {
            var assignedSymbols = new HashSet<string>();

            foreach (var method in methods)
            {
                HashSet<string> assignedSymbolsInMethod = AssignedIn(model, type, names, method);
                assignedSymbols.UnionWith(assignedSymbolsInMethod);
            }

            return assignedSymbols;
        }

        /// <summary>
        /// Returns a hash set of the symbols assigned in <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The method to look for.</param>
        /// <param name="names">The symbols to check for assignment.</param>
        /// <returns>HashSet of <paramref name="names"/> that are assigned in <paramref name="symbol"/>.</returns>
        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, IMethodSymbol symbol)
        {
            BaseMethodDeclarationSyntax? method =
                symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as BaseMethodDeclarationSyntax;

            return method is null ? new HashSet<string>() : AssignedIn(model, type, names, method);
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, BaseMethodDeclarationSyntax method)
        {
            if (method.ExpressionBody is not null)
            {
                return AssignedIn(model, type, names, method.ExpressionBody.Expression);
            }

            if (method.Body is not null)
            {
                return AssignedIn(model, type, names, method.Body);
            }

            return new HashSet<string>();
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, ExpressionSyntax expression)
        {
            var assignedSymbols = new HashSet<string>();
            if (expression is AssignmentExpressionSyntax assignmentExpression)
            {
                // We only deal with simple assignments, not tuple or deconstruct
                string? name = GetIdentifier(assignmentExpression.Left);
                if (name is not null && names.Contains(name))
                {
                    if (NeedsDisposal(model, assignmentExpression.Right))
                    {
                        assignedSymbols.Add(name);
                    }
                }
            }
            else if (expression is InvocationExpressionSyntax invocationExpression)
            {
                string? method = GetIdentifier(invocationExpression.Expression);
                if (method is not null)
                {
                    IMethodSymbol? calledMethod = model.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
                    if (calledMethod is not null && SymbolEqualityComparer.Default.Equals(calledMethod.ContainingType, type))
                    {
                        // We are calling a local method on our class, keep looking for assignments.
                        return AssignedIn(model, type, names, calledMethod);
                    }
                }
            }

            return assignedSymbols;
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, StatementSyntax statement)
        {
            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    return AssignedIn(model, type, names, expressionStatement.Expression);

                case IfStatementSyntax ifStatement:
                {
                    // We don't care about the condition
                    HashSet<string> assignedSymbolsInStatement = AssignedIn(model, type, names, ifStatement.Statement);
                    if (ifStatement.Else is not null)
                        assignedSymbolsInStatement.UnionWith(AssignedIn(model, type, names, ifStatement.Else.Statement));

                    return assignedSymbolsInStatement;
                }

                case BlockSyntax block:
                    return AssignedIn(model, type, names, block.Statements);

                case SwitchStatementSyntax switchStatement:
                {
                    var assignedSymbols = new HashSet<string>();

                    foreach (var caseStatements in switchStatement.Sections.Select(x => x.Statements))
                    {
                        HashSet<string> assignedSymbolsInStatement = AssignedIn(model, type, names, caseStatements);
                        assignedSymbols.UnionWith(assignedSymbolsInStatement);
                    }

                    return assignedSymbols;
                }

                default:
                    // Anything assigned in a loop is bad as it overrides previous assignments.
                    return new HashSet<string>();
            }
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, SyntaxList<StatementSyntax> statements)
        {
            var assignedSymbols = new HashSet<string>();

            foreach (var statement in statements)
            {
                HashSet<string> assignedSymbolsInStatement = AssignedIn(model, type, names, statement);
                assignedSymbols.UnionWith(assignedSymbolsInStatement);
            }

            return assignedSymbols;
        }

        private static bool NeedsDisposal(SemanticModel model, ExpressionSyntax expression)
        {
            if (IsPossibleDisposableCreation(expression))
            {
                ITypeSymbol? instanceType = model.GetTypeInfo(expression).Type;
                return instanceType is not null &&
                       instanceType.IsDisposable() &&
                       !IsDisposableTypeNotRequiringToBeDisposed(instanceType);
            }

            return false;
        }

        private static bool IsPossibleDisposableCreation(ExpressionSyntax expression)
        {
            if (expression is ObjectCreationExpressionSyntax)
                return true;

            if (expression is InvocationExpressionSyntax invocationExpression)
            {
                // Make one exemption, if the value is returned from a 'xxx.Add()' call.
                // It is then assumed that owner ship is transferred to that 'collection'.
                // This matches the (undocumented) CA2000 behaviour.
                // Although we don't actually check if the class implements ICollection.
                // https://github.com/dotnet/roslyn-analyzers/blob/main/src/Utilities/Compiler/Extensions/IMethodSymbolExtensions.cs#L465-L499
                return invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccessExpression ||
                       !memberAccessExpression.Name.Identifier.Text.StartsWith("Add", StringComparison.Ordinal);
            }

            return false;
        }

        private static bool IsDisposableTypeNotRequiringToBeDisposed(ITypeSymbol typeSymbol)
        {
            return DisposableTypeNotRequiringToBeDisposed.Contains(typeSymbol.GetFullMetadataName());
        }

        #endregion

        #region DisposedIn

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, IEnumerable<IMethodSymbol> methods)
        {
            var disposedSymbols = new HashSet<string>();

            foreach (var method in methods)
            {
                HashSet<string> disposedSymbolsInMethod = DisposedIn(model, type, names, method);
                disposedSymbols.UnionWith(disposedSymbolsInMethod);
            }

            return disposedSymbols;
        }

        /// <summary>
        /// Returns a hash set of the symbols disposed in <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The method to look for.</param>
        /// <param name="names">The symbols to check for disposal.</param>
        /// <returns>HashSet of <paramref name="names"/> that are disposed in <paramref name="symbol"/>.</returns>
        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, IMethodSymbol symbol)
        {
            MethodDeclarationSyntax? method =
                symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            return method is null ? new HashSet<string>() : DisposedIn(model, type, names, method);
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, MethodDeclarationSyntax method)
        {
            if (method.ExpressionBody is not null)
            {
                return DisposedIn(model, type, names, method.ExpressionBody.Expression);
            }

            if (method.Body is not null)
            {
                return DisposedIn(model, type, names, method.Body);
            }

            return new HashSet<string>();
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, ExpressionSyntax expression)
        {
            var disposedSymbols = new HashSet<string>();
            if (expression is AwaitExpressionSyntax awaitExpression)
            {
                expression = awaitExpression.Expression;
                if (expression is InvocationExpressionSyntax awaitInvocationExpression &&
                    awaitInvocationExpression.Expression is MemberAccessExpressionSyntax awaitMemberAccessExpression &&
                    awaitMemberAccessExpression.Name.Identifier.Text == "ConfigureAwait")
                {
                    expression = awaitMemberAccessExpression.Expression;
                }
            }

            if (expression is InvocationExpressionSyntax invocationExpression)
            {
                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    if (IsDisposeMethod(memberAccessExpression.Name))
                    {
                        string? target = GetTargetName(memberAccessExpression.Expression);
                        if (target is not null && names.Contains(target))
                        {
                            disposedSymbols.Add(target);
                        }

                        return disposedSymbols;
                    }
                }

                string? method = GetIdentifier(invocationExpression.Expression);
                if (method is not null)
                {
                    IMethodSymbol? calledMethod = model.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
                    if (calledMethod is not null)
                    {
                        if (SymbolEqualityComparer.Default.Equals(calledMethod.ContainingType, type))
                        {
                            // We are calling a local method on our class, keep looking for disposals.
                            return DisposedIn(model, type, names, calledMethod);
                        }
                    }
                }
            }
            else if (expression is ConditionalAccessExpressionSyntax conditionalAccessExpression &&
                conditionalAccessExpression.WhenNotNull is InvocationExpressionSyntax conditionalInvocationExpression &&
                conditionalInvocationExpression.Expression is MemberBindingExpressionSyntax memberBindingExpression &&
                IsDisposeMethod(memberBindingExpression.Name))
            {
                string? target = GetTargetName(conditionalAccessExpression.Expression);
                if (target is not null && names.Contains(target))
                {
                    disposedSymbols.Add(target);
                }
            }

            return disposedSymbols;
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, StatementSyntax statement)
        {
            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    return DisposedIn(model, type, names, expressionStatement.Expression);

                case IfStatementSyntax ifStatement:
                {
                    // Check for:
                    // if (field is IDisposable disposable)
                    //     disposable.Dispose();
                    if (ifStatement.Condition is IsPatternExpressionSyntax isPatternExpression &&
                        isPatternExpression.Pattern is DeclarationPatternSyntax declarationPattern &&
                        declarationPattern.Type is IdentifierNameSyntax identifierName &&
                        identifierName.Identifier.Text.EndsWith("Disposable", StringComparison.Ordinal) &&
                        declarationPattern.Designation is SingleVariableDesignationSyntax singleVariableDesignation)
                    {
                        string? member = GetIdentifier(isPatternExpression.Expression);
                        if (member is not null && names.Contains(member))
                        {
                            string variable = singleVariableDesignation.Identifier.Text;
                            HashSet<string> disposedSymbols = DisposedIn(model, type, new HashSet<string>() { variable }, ifStatement.Statement);
                            if (disposedSymbols.Contains(variable))
                            {
                                return new HashSet<string>() { member };
                            }
                        }
                    }

                    // In other cases we don't care about the condition
                    HashSet<string> disposedSymbolsInStatement = DisposedIn(model, type, names, ifStatement.Statement);
                    if (ifStatement.Else is not null)
                        disposedSymbolsInStatement.UnionWith(DisposedIn(model, type, names, ifStatement.Else.Statement));

                    return disposedSymbolsInStatement;
                }

                case BlockSyntax block:
                    return DisposedIn(model, type, names, block.Statements);

                case SwitchStatementSyntax switchStatement:
                {
                    var disposedSymbols = new HashSet<string>();

                    foreach (var caseStatements in switchStatement.Sections.Select(x => x.Statements))
                    {
                        HashSet<string> disposedSymbolsInStatement = DisposedIn(model, type, names, caseStatements);
                        disposedSymbols.UnionWith(disposedSymbolsInStatement);
                    }

                    return disposedSymbols;
                }

                case TryStatementSyntax tryStatement:
                {
                    var disposedSymbols = DisposedIn(model, type, names, tryStatement.Block);
                    if (tryStatement.Finally is not null)
                        disposedSymbols.UnionWith(DisposedIn(model, type, names, tryStatement.Finally.Block));
                    return disposedSymbols;
                }

                default:
                    return new HashSet<string>();
            }
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> names, SyntaxList<StatementSyntax> statements)
        {
            var disposedSymbols = new HashSet<string>();

            foreach (var statement in statements)
            {
                HashSet<string> disposedSymbolsInStatement = DisposedIn(model, type, names, statement);
                disposedSymbols.UnionWith(disposedSymbolsInStatement);
            }

            return disposedSymbols;
        }

        private static bool IsDisposeMethod(SimpleNameSyntax name) => DisposeMethods.Contains(name.Identifier.Text);

        private static string? GetTargetName(ExpressionSyntax expression)
        {
            if (expression is MemberAccessExpressionSyntax memberAccessExpression &&
               memberAccessExpression.Expression is ThisExpressionSyntax)
            {
                expression = memberAccessExpression.Name;
            }

            if (expression is SimpleNameSyntax simpleName)
            {
                return simpleName.Identifier.Text;
            }

            return null;
        }

        #endregion

        private static string? GetIdentifier(ExpressionSyntax expression)
        {
            if (expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }
            else if (expression is MemberAccessExpressionSyntax memberAccessExpression &&
                memberAccessExpression.Expression is ThisExpressionSyntax)
            {
                return memberAccessExpression.Name.Identifier.Text;
            }

            return null;
        }
    }
}
