using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.DisposeFieldsInTearDown
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposeFieldsInTearDownAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor fieldIsNotDisposedInTearDown = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.FieldIsNotDisposedInTearDown,
            title: DisposeFieldsInTearDownConstants.FieldIsNotDisposedInTearDownTitle,
            messageFormat: DisposeFieldsInTearDownConstants.FieldIsNotDisposedInTearDownMessageFormat,
            category: Categories.Structure,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: DisposeFieldsInTearDownConstants.FieldIsNotDisposedInTearDownDescription);

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
                // If the type is Disposable, the MS Analyzer will conflict, so bail out.
                return;
            }

            var fieldDeclarations = classDeclaration.Members
                                                    .OfType<FieldDeclarationSyntax>()
                                                    .Select(x => x.Declaration)
                                                    .SelectMany(x => x.Variables);

            Dictionary<string, VariableDeclaratorSyntax> fields = fieldDeclarations.ToDictionary(x => x.Identifier.Text);
            HashSet<string> fieldNames = new HashSet<string>(fields.Keys);

            ImmutableArray<ISymbol> members = typeSymbol.GetMembers();
            var methods = members.OfType<IMethodSymbol>().Where(m => !m.IsStatic).ToArray();
            var oneTimeTearDownMethods = methods.Where(m => HasAttribute(m, "OneTimeTearDownAttribute")).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var oneTimeSetUpMethods = methods.Where(m => HasAttribute(m, "OneTimeSetUpAttribute")).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var setUpMethods = methods.Where(m => HasAttribute(m, "SetUpAttribute")).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var tearDownMethods = methods.Where(m => HasAttribute(m, "TearDownAttribute")).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

            var setUpAndTearDownMethods = oneTimeSetUpMethods.Union(oneTimeTearDownMethods).Union(setUpMethods).Union(tearDownMethods);
            var otherMethods = methods.Where(m => m.DeclaredAccessibility != Accessibility.Private && !setUpAndTearDownMethods.Contains(m));

            // Fields assigned in a OneTimeSetUp method must be disposed in a OneTimeTearDown method
            AnalyzeAssignedButNotDisposed(context, model, typeSymbol, fields, fieldNames,
                "OneTimeTearDown", oneTimeSetUpMethods, oneTimeTearDownMethods);

            // Fields assigned in a SetUp method must be disposed in a TearDown method
            AnalyzeAssignedButNotDisposed(context, model, typeSymbol, fields, fieldNames,
                "TearDown", setUpMethods, tearDownMethods);

            // Fields assignd in any method, must be (conditionally) disposed in TearDown method.
            // If the field is disposed in the method itself, why is it a field?
            AnalyzeAssignedButNotDisposed(context, model, typeSymbol, fields, fieldNames,
                "TearDown", otherMethods, tearDownMethods);
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
            Dictionary<string, VariableDeclaratorSyntax> fields,
            HashSet<string> names,
            string where,
            IEnumerable<IMethodSymbol> setUpMethods,
            IEnumerable<IMethodSymbol> tearDownMethods)
        {
            var assignedInSetUpMethods = AssignedIn(model, type, names, setUpMethods);
            var disposedInTearDownMethods = DisposedIn(model, type, names, tearDownMethods);
            assignedInSetUpMethods.ExceptWith(disposedInTearDownMethods);

            foreach (var assignedButNotDisposed in assignedInSetUpMethods)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    fieldIsNotDisposedInTearDown,
                    fields[assignedButNotDisposed].GetLocation(),
                    assignedButNotDisposed,
                    where));
            }
        }

        #region AssignedIn

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, IEnumerable<IMethodSymbol> methods)
        {
            var assignedSymbols = new HashSet<string>();

            foreach (var method in methods)
            {
                HashSet<string> assignedSymbolsInMethod = AssignedIn(model, type, symbols, method);
                assignedSymbols.UnionWith(assignedSymbolsInMethod);
            }

            return assignedSymbols;
        }

        /// <summary>
        /// Returns a hash set of the fields assigned in <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The method to look for.</param>
        /// <param name="symbols">The symbols to check for assignment.</param>
        /// <returns>HashSet of <paramref name="symbols"/> that are assigned in <paramref name="symbol"/>.</returns>
        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, IMethodSymbol symbol)
        {
            MethodDeclarationSyntax? method =
                symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            return method is null ? new HashSet<string>() : AssignedIn(model, type, symbols, method);
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, MethodDeclarationSyntax method)
        {
            if (method.ExpressionBody is not null)
            {
                return AssignedIn(model, type, symbols, method.ExpressionBody.Expression);
            }

            if (method.Body is not null)
            {
                return AssignedIn(model, type, symbols, method.Body);
            }

            return new HashSet<string>();
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, ExpressionSyntax expression)
        {
            var assignedSymbols = new HashSet<string>();
            if (expression is AssignmentExpressionSyntax assignmentExpression)
            {
                // We only deal with simple assignments, not tuple or deconstruct
                string? name = GetIdentifier(assignmentExpression.Left);
                if (name is not null && symbols.Contains(name))
                {
                    TypeInfo typeInfo = model.GetTypeInfo(assignmentExpression.Right);
                    if (typeInfo.Type?.IsDisposable() == true)
                    {
                        // Make one exemption, if the value is returned from a 'xxx.Add()' call.
                        // It is then assumed that owner ship is transferred to that 'collection'.
                        // This matches the (undocumented) CA2000 behaviour.
                        if (assignmentExpression.Right is not InvocationExpressionSyntax invocationExpression ||
                            invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccessExpression ||
                            memberAccessExpression.Name.Identifier.Text != "Add")
                        {
                            assignedSymbols.Add(name);
                        }
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
                        return AssignedIn(model, type, symbols, calledMethod);
                    }
                }
            }

            return assignedSymbols;
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, StatementSyntax statement)
        {
            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    return AssignedIn(model, type, symbols, expressionStatement.Expression);

                case IfStatementSyntax ifStatement:
                {
                    // We don't care about the condition
                    HashSet<string> assignedSymbolsInStatement = AssignedIn(model, type, symbols, ifStatement.Statement);
                    if (ifStatement.Else is not null)
                        assignedSymbolsInStatement.UnionWith(AssignedIn(model, type, symbols, ifStatement.Else.Statement));

                    return assignedSymbolsInStatement;
                }

                case BlockSyntax block:
                    return AssignedIn(model, type, symbols, block.Statements);

                case SwitchStatementSyntax switchStatement:
                {
                    var assignedSymbols = new HashSet<string>();

                    foreach (var caseStatements in switchStatement.Sections.Select(x => x.Statements))
                    {
                        HashSet<string> assignedSymbolsInStatement = AssignedIn(model, type, symbols, caseStatements);
                        assignedSymbols.UnionWith(assignedSymbolsInStatement);
                    }

                    return assignedSymbols;
                }

                default:
                    // Anything assigned in a loop is bad as it overrides previous assignments.
                    return new HashSet<string>();
            }
        }

        private static HashSet<string> AssignedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, SyntaxList<StatementSyntax> statements)
        {
            var assignedSymbols = new HashSet<string>();

            foreach (var statement in statements)
            {
                HashSet<string> assignedSymbolsInStatement = AssignedIn(model, type, symbols, statement);
                assignedSymbols.UnionWith(assignedSymbolsInStatement);
            }

            return assignedSymbols;
        }

        #endregion

        #region DisposedIn

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, IEnumerable<IMethodSymbol> methods)
        {
            var disposedSymbols = new HashSet<string>();

            foreach (var method in methods)
            {
                HashSet<string> disposedSymbolsInMethod = DisposedIn(model, type, symbols, method);
                disposedSymbols.UnionWith(disposedSymbolsInMethod);
            }

            return disposedSymbols;
        }

        /// <summary>
        /// Returns a hash set of the fields disposed in <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The method to look for.</param>
        /// <param name="symbols">The symbols to check for assignment.</param>
        /// <returns>HashSet of <paramref name="symbols"/> that are disposed in <paramref name="symbol"/>.</returns>
        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, IMethodSymbol symbol)
        {
            MethodDeclarationSyntax? method =
                symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            return method is null ? new HashSet<string>() : DisposedIn(model, type, symbols, method);
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, MethodDeclarationSyntax method)
        {
            if (method.ExpressionBody is not null)
            {
                return DisposedIn(model, type, symbols, method.ExpressionBody.Expression);
            }

            if (method.Body is not null)
            {
                return DisposedIn(model, type, symbols, method.Body);
            }

            return new HashSet<string>();
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, ExpressionSyntax expression)
        {
            var disposedSymbols = new HashSet<string>();
            if (expression is InvocationExpressionSyntax invocationExpression)
            {
                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is not ThisExpressionSyntax)
                {
                    if (IsDispose(memberAccessExpression.Name))
                    {
                        string? target = GetTargetName(memberAccessExpression.Expression);
                        if (target is not null && symbols.Contains(target))
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
                            // We are calling a local method on our class, keep looking for assignments.
                            return DisposedIn(model, type, symbols, calledMethod);
                        }
                    }
                }
            }
            else if (expression is ConditionalAccessExpressionSyntax conditionalAccessExpression &&
                conditionalAccessExpression.WhenNotNull is InvocationExpressionSyntax conditionalInvocationExpression &&
                conditionalInvocationExpression.Expression is MemberBindingExpressionSyntax memberBindingExpression &&
                IsDispose(memberBindingExpression.Name))
            {
                string? target = GetTargetName(conditionalAccessExpression.Expression);
                if (target is not null && symbols.Contains(target))
                {
                    disposedSymbols.Add(target);
                }
            }

            return disposedSymbols;
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, StatementSyntax statement)
        {
            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    return DisposedIn(model, type, symbols, expressionStatement.Expression);

                case IfStatementSyntax ifStatement:
                    {
                        // We don't care about the condition
                        HashSet<string> disposedSymbolsInStatement = DisposedIn(model, type, symbols, ifStatement.Statement);
                        if (ifStatement.Else is not null)
                            disposedSymbolsInStatement.UnionWith(DisposedIn(model, type, symbols, ifStatement.Else.Statement));

                        return disposedSymbolsInStatement;
                    }

                case BlockSyntax block:
                    return DisposedIn(model, type, symbols, block.Statements);

                case SwitchStatementSyntax switchStatement:
                    {
                        var disposedSymbols = new HashSet<string>();

                        foreach (var caseStatements in switchStatement.Sections.Select(x => x.Statements))
                        {
                            HashSet<string> disposedSymbolsInStatement = DisposedIn(model, type, symbols, caseStatements);
                            disposedSymbols.UnionWith(disposedSymbolsInStatement);
                        }

                        return disposedSymbols;
                    }

                default:
                    return new HashSet<string>();
            }
        }

        private static HashSet<string> DisposedIn(SemanticModel model, INamedTypeSymbol type, HashSet<string> symbols, SyntaxList<StatementSyntax> statements)
        {
            var disposedSymbols = new HashSet<string>();

            foreach (var statement in statements)
            {
                HashSet<string> disposedSymbolsInStatement = DisposedIn(model, type, symbols, statement);
                disposedSymbols.UnionWith(disposedSymbolsInStatement);
            }

            return disposedSymbols;
        }

        private static bool IsDispose(SimpleNameSyntax name) => name.Identifier.Text is "Dispose" or "DisposeAsync";

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
