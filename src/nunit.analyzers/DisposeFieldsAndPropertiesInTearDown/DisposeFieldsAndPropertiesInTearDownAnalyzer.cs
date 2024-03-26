using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
    public sealed class DisposeFieldsAndPropertiesInTearDownAnalyzer : DiagnosticAnalyzer
    {
        private static readonly char[] AdditionalDisposalMethodsSeparators = { ',', ';', ' ' };

        // Methods that are considered to be Dispoing an instance.
        private static readonly ImmutableHashSet<string> StandardDisposeMethods = ImmutableHashSet.Create(
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

        internal static int EarlyOutCount { get; private set; }

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

            if (typeSymbol.IsInstancePerTestCaseFixture(context.Compilation) ||
                !typeSymbol.IsTestFixture(context.Compilation))
            {
                // CA1001 should pick this up. Assuming it is enabled.
                return;
            }

            var fieldDeclarations = classDeclaration.Members
                                                    .OfType<FieldDeclarationSyntax>();

            var propertyDeclarations = classDeclaration.Members
                                                       .OfType<PropertyDeclarationSyntax>()
                                                       .Where(x => x.AccessorList is not null);

            bool hasConstructors = classDeclaration.Members
                                                   .OfType<ConstructorDeclarationSyntax>()
                                                   .Any();

            HashSet<string> symbolsWithDisposableInitializers = new();

            Dictionary<string, SyntaxNode> symbols = new();
            foreach (var fieldDeclaration in fieldDeclarations)
            {
                foreach (var field in fieldDeclaration.Declaration.Variables)
                {
                    if (field.Initializer is not null)
                    {
                        if (NeedsDisposal(model, field.Initializer.Value))
                        {
                            symbolsWithDisposableInitializers.Add(field.Identifier.Text);
                            symbols.Add(field.Identifier.Text, field);
                            continue;
                        }
                    }

                    if (CanBeAssignedTo(fieldDeclaration, hasConstructors))
                    {
                        symbols.Add(field.Identifier.Text, field);
                    }
                }
            }

            foreach (var property in propertyDeclarations)
            {
                if (property.ExplicitInterfaceSpecifier is not null)
                    continue;

                if (property.Initializer is not null)
                {
                    if (NeedsDisposal(model, property.Initializer.Value))
                    {
                        symbolsWithDisposableInitializers.Add(property.Identifier.Text);
                        symbols.Add(property.Identifier.Text, property);
                        continue;
                    }
                }

                if (CanBeAssignedTo(property, hasConstructors))
                {
                    symbols.Add(property.Identifier.Text, property);
                }
            }

            if (symbols.Count == 0)
            {
                // No fields or properties to consider.
                EarlyOutCount++;
                return;
            }

            ImmutableHashSet<string> disposeMethods = StandardDisposeMethods;

            // Are there any additional methods configured that are considered Dispose Methods
            // e.g. DisposeIfDisposeable or Release
            AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(classDeclaration.SyntaxTree);
            if (options.TryGetValue("dotnet_diagnostic.NUnit1032.additional_dispose_methods", out string? value))
            {
                disposeMethods = disposeMethods.Union(value.Split(AdditionalDisposalMethodsSeparators, StringSplitOptions.RemoveEmptyEntries));
            }

            HashSet<string> symbolNames = new(symbols.Keys);

            Parameters parameters = new(model, typeSymbol, disposeMethods, symbolNames);

            ImmutableArray<ISymbol> members = typeSymbol.GetMembers();
            var methods = members.OfType<IMethodSymbol>().ToArray();
            var oneTimeTearDownMethods = methods.Where(m => HasAttribute(m, NUnitFrameworkConstants.NameOfOneTimeTearDownAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var oneTimeSetUpMethods = methods.Where(m => m.MethodKind == MethodKind.Constructor || HasAttribute(m, NUnitFrameworkConstants.NameOfOneTimeSetUpAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var setUpMethods = methods.Where(m => HasAttribute(m, NUnitFrameworkConstants.NameOfSetUpAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var tearDownMethods = methods.Where(m => HasAttribute(m, NUnitFrameworkConstants.NameOfTearDownAttribute)).ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

            var setUpAndTearDownMethods = oneTimeSetUpMethods.Union(oneTimeTearDownMethods).Union(setUpMethods).Union(tearDownMethods);
            var otherMethods = methods.Where(m => m.DeclaredAccessibility != Accessibility.Private && !setUpAndTearDownMethods.Contains(m));

            // Fields assigned in a OneTimeSetUp method must be disposed in a OneTimeTearDown method
            AnalyzeAssignedButNotDisposed(context, symbols, parameters,
                NUnitFrameworkConstants.NameOfOneTimeTearDownAttribute, oneTimeSetUpMethods, oneTimeTearDownMethods, symbolsWithDisposableInitializers);

            // Fields assigned in a SetUp method must be disposed in a TearDown method
            AnalyzeAssignedButNotDisposed(context, symbols, parameters,
                NUnitFrameworkConstants.NameOfTearDownAttribute, setUpMethods, tearDownMethods);

            // Fields assignd in any method, must be (conditionally) disposed in TearDown method.
            // If the field is disposed in the method itself, why is it a field?
            AnalyzeAssignedButNotDisposed(context, symbols, parameters,
                NUnitFrameworkConstants.NameOfTearDownAttribute, otherMethods, tearDownMethods);
        }

        private static bool CanBeAssignedTo(FieldDeclarationSyntax declaration, bool hasConstructors)
        {
            return !declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ConstKeyword)) &&
                (!declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ReadOnlyKeyword)) || hasConstructors);
        }

        private static bool CanBeAssignedTo(PropertyDeclarationSyntax declaration, bool hasConstructors)
        {
            AccessorListSyntax? accessorList = declaration.AccessorList;
            if (accessorList is null)
                return false;           // Expression body property

            return hasConstructors || accessorList.Accessors.Any(accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));
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
            Dictionary<string, SyntaxNode> symbols,
            Parameters parameters,
            string where,
            IEnumerable<IMethodSymbol> setUpMethods,
            IEnumerable<IMethodSymbol> tearDownMethods,
            HashSet<string>? assignedWithInitializers = null)
        {
            HashSet<string> assignedInSetUpMethods = assignedWithInitializers ?? new();
            HashSet<string> disposedInTearDownMethods = new();

            AssignedIn(parameters, assignedInSetUpMethods, setUpMethods);
            DisposedIn(parameters, disposedInTearDownMethods, tearDownMethods);

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

        private static void AssignedIn(Parameters parameters, HashSet<string> assignments, IEnumerable<IMethodSymbol> methods)
        {
            parameters.ResetMethodCallVisits();
            foreach (var method in methods)
            {
                AssignedIn(parameters, assignments, method);
            }
        }

        private static void AssignedIn(Parameters parameters, HashSet<string> assignments, IMethodSymbol symbol)
        {
            BaseMethodDeclarationSyntax? method =
                symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as BaseMethodDeclarationSyntax;

            if (method is not null)
                AssignedIn(parameters, assignments, method);
        }

        private static void AssignedIn(Parameters parameters, HashSet<string> assignments, BaseMethodDeclarationSyntax method)
        {
            if (method.ExpressionBody is not null)
            {
                AssignedIn(parameters, assignments, method.ExpressionBody.Expression);
            }
            else if (method.Body is not null)
            {
                AssignedIn(parameters, assignments, method.Body);
            }
        }

        private static void AssignedIn(Parameters parameters, HashSet<string> assignments, ExpressionSyntax expression)
        {
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

            if (expression is AssignmentExpressionSyntax assignmentExpression)
            {
                // We only deal with simple assignments, not tuple or deconstruct
                string? name = GetIdentifier(assignmentExpression.Left);
                if (name is not null && parameters.HasSymbolFor(name))
                {
                    if (NeedsDisposal(parameters.Model, assignmentExpression.Right))
                    {
                        assignments.Add(name);
                    }
                }
            }
            else if (expression is InvocationExpressionSyntax invocationExpression)
            {
                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression &&
                    memberAccessExpression.Expression is InvocationExpressionSyntax awaitedInvocationExpression &&
                    memberAccessExpression.Name.Identifier.Text == "Wait")
                {
                    invocationExpression = awaitedInvocationExpression;
                }

                string? method = GetIdentifier(invocationExpression.Expression);
                if (method is not null)
                {
                    if (parameters.IsLocalNotYetVisitedMethodCall(invocationExpression, out IMethodSymbol? calledMethod))
                    {
                        // We are calling a local method on our class, keep looking for assignments.
                        AssignedIn(parameters, assignments, calledMethod);
                    }
                }
            }
        }

        private static void AssignedIn(Parameters parameters, HashSet<string> assignments, StatementSyntax statement)
        {
            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    AssignedIn(parameters, assignments, expressionStatement.Expression);
                    break;

                case IfStatementSyntax ifStatement:
                    // We don't care about the condition
                    AssignedIn(parameters, assignments, ifStatement.Statement);
                    if (ifStatement.Else is not null)
                        AssignedIn(parameters, assignments, ifStatement.Else.Statement);
                    break;

                case BlockSyntax block:
                    AssignedIn(parameters, assignments, block.Statements);
                    break;

                case SwitchStatementSyntax switchStatement:
                    foreach (var caseStatements in switchStatement.Sections.Select(x => x.Statements))
                    {
                        AssignedIn(parameters, assignments, caseStatements);
                    }

                    break;

                case TryStatementSyntax tryStatement:
                    AssignedIn(parameters, assignments, tryStatement.Block);
                    if (tryStatement.Finally is not null)
                        AssignedIn(parameters, assignments, tryStatement.Finally.Block);
                    break;

                default:
                    // Anything assigned in a loop is bad as it overrides previous assignments.
                    break;
            }
        }

        private static void AssignedIn(Parameters parameters, HashSet<string> assignments, SyntaxList<StatementSyntax> statements)
        {
            foreach (var statement in statements)
            {
                AssignedIn(parameters, assignments, statement);
            }
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

        private static void DisposedIn(Parameters parameters, HashSet<string> disposals, IEnumerable<IMethodSymbol> methods)
        {
            parameters.ResetMethodCallVisits();
            foreach (var method in methods)
            {
                DisposedIn(parameters, disposals, method);
            }
        }

        private static void DisposedIn(Parameters parameters, HashSet<string> disposals, IMethodSymbol symbol)
        {
            MethodDeclarationSyntax? method =
                symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            if (method is not null)
                DisposedIn(parameters, disposals, method);
        }

        private static void DisposedIn(Parameters parameters, HashSet<string> disposals, MethodDeclarationSyntax method)
        {
            if (method.ExpressionBody is not null)
            {
                DisposedIn(parameters, disposals, method.ExpressionBody.Expression);
            }
            else if (method.Body is not null)
            {
                DisposedIn(parameters, disposals, method.Body);
            }
        }

        private static void DisposedIn(Parameters parameters, HashSet<string> disposals, ExpressionSyntax expression)
        {
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
                if (parameters.IsDisposalOf(invocationExpression, null, out string? disposedSymbol))
                {
                    disposals.Add(disposedSymbol);
                }
                else if (parameters.IsLocalNotYetVisitedMethodCall(invocationExpression, out IMethodSymbol? calledMethod))
                {
                    // We are calling a local method on our class, keep looking for disposals.
                    DisposedIn(parameters, disposals, calledMethod);
                }
            }
            else if (expression is ConditionalAccessExpressionSyntax conditionalAccessExpression &&
                conditionalAccessExpression.WhenNotNull is InvocationExpressionSyntax conditionalInvocationExpression &&
                parameters.IsDisposalOf(conditionalInvocationExpression, conditionalAccessExpression.Expression, out string? disposedSymbol))
            {
                disposals.Add(disposedSymbol);
            }
        }

        private static void DisposedIn(Parameters parameters, HashSet<string> disposals, StatementSyntax statement)
        {
            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    DisposedIn(parameters, disposals, expressionStatement.Expression);
                    break;

                case IfStatementSyntax ifStatement:
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
                        if (member is not null && parameters.HasSymbolFor(member))
                        {
                            string variable = singleVariableDesignation.Identifier.Text;
                            HashSet<string> disposedSymbols = new();
                            DisposedIn(parameters.With(variable), disposedSymbols, ifStatement.Statement);
                            if (disposedSymbols.Contains(variable))
                            {
                                disposals.Add(member);
                            }
                        }
                    }

                    // In other cases we don't care about the condition
                    DisposedIn(parameters, disposals, ifStatement.Statement);
                    if (ifStatement.Else is not null)
                        DisposedIn(parameters, disposals, ifStatement.Else.Statement);
                    break;

                case BlockSyntax block:
                    DisposedIn(parameters, disposals, block.Statements);
                    break;

                case SwitchStatementSyntax switchStatement:
                    foreach (var caseStatements in switchStatement.Sections.Select(x => x.Statements))
                    {
                        DisposedIn(parameters, disposals, caseStatements);
                    }

                    break;

                case TryStatementSyntax tryStatement:
                    DisposedIn(parameters, disposals, tryStatement.Block);
                    if (tryStatement.Finally is not null)
                        DisposedIn(parameters, disposals, tryStatement.Finally.Block);
                    break;

                default:
                    break;
            }
        }

        private static void DisposedIn(Parameters parameters, HashSet<string> disposals, SyntaxList<StatementSyntax> statements)
        {
            foreach (var statement in statements)
            {
                DisposedIn(parameters, disposals, statement);
            }
        }

        #endregion

        private static string? GetIdentifier(ExpressionSyntax expression)
        {
            // Account for 'Release(field!)'
            if (expression is PostfixUnaryExpressionSyntax postfixUnaryExpression &&
                postfixUnaryExpression.IsKind(SyntaxKind.SuppressNullableWarningExpression))
            {
                expression = postfixUnaryExpression.Operand;
            }

            if (expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }
            else if (expression is MemberAccessExpressionSyntax memberAccessExpression &&
                memberAccessExpression.Expression is ThisExpressionSyntax)
            {
                return memberAccessExpression.Name.Identifier.Text;
            }
            else if (expression is ParenthesizedExpressionSyntax { Expression: CastExpressionSyntax { Expression: IdentifierNameSyntax castIdentifierNameSyntax, Type: IdentifierNameSyntax
                     {
                         Identifier.Text: "IDisposable"
                     } } })
            {
                return castIdentifierNameSyntax.Identifier.Text;
            }

            return null;
        }

        private sealed class Parameters
        {
            private readonly INamedTypeSymbol type;
            private readonly ImmutableHashSet<string> disposeMethods;
            private readonly HashSet<string> names;
            private readonly HashSet<IMethodSymbol> visitedMethods = new(SymbolEqualityComparer.Default);

            public Parameters(SemanticModel model, INamedTypeSymbol type, ImmutableHashSet<string> disposeMethods, HashSet<string> names)
            {
                this.Model = model;
                this.type = type;
                this.disposeMethods = disposeMethods;
                this.names = names;
            }

            public SemanticModel Model { get; }

            public bool IsLocalMethodCall(
                InvocationExpressionSyntax invocationExpression,
                [NotNullWhen(true)] out IMethodSymbol? calledMethod)
            {
                if (this.Model.SyntaxTree != invocationExpression.SyntaxTree)
                {
                    calledMethod = null;
                    return false;
                }

                calledMethod = this.Model.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
                return calledMethod is not null &&
                    SymbolEqualityComparer.Default.Equals(calledMethod.ContainingType, this.type);
            }

            public void ResetMethodCallVisits() => this.visitedMethods.Clear();

            public bool IsLocalNotYetVisitedMethodCall(
                InvocationExpressionSyntax invocationExpression,
                [NotNullWhen(true)] out IMethodSymbol? calledMethod)
            {
                return this.IsLocalMethodCall(invocationExpression, out calledMethod) &&
                       this.visitedMethods.Add(calledMethod);
            }

            public bool IsDisposalOf(
                InvocationExpressionSyntax invocationExpression,
                ExpressionSyntax? conditionalTarget,
                [NotNullWhen(true)] out string? symbol)
            {
                SeparatedSyntaxList<ArgumentSyntax> arguments = invocationExpression.ArgumentList.Arguments;
                if (arguments.Count > 1)
                {
                    symbol = null;
                    return false;
                }

                SimpleNameSyntax? calledMethod = GetCalledMethod(invocationExpression.Expression, out ExpressionSyntax? target);
                if (calledMethod is null || !this.IsDisposeMethod(calledMethod))
                {
                    symbol = null;
                    return false;
                }

                if (arguments.Count == 0 && (target is not null || conditionalTarget is not null))
                {
                    // This must be `diposable(?).DisposeMethod()`
                    symbol = GetIdentifier(target ?? conditionalTarget!);
                }
                else if (arguments.Count == 1)
                {
                    // This must be a `(factory.)DiposeMethod(disposable)`
                    symbol = GetIdentifier(arguments[0].Expression);
                }
                else
                {
                    symbol = null;
                }

                return symbol is not null && this.HasSymbolFor(symbol);
            }

            public bool HasSymbolFor(string name) => this.names.Contains(name);

            public bool IsDisposeMethod(SimpleNameSyntax name) => this.disposeMethods.Contains(name.Identifier.Text);

            public Parameters With(string name) => new(this.Model, this.type, this.disposeMethods, new HashSet<string>() { name });

            private static SimpleNameSyntax? GetCalledMethod(ExpressionSyntax expression, out ExpressionSyntax? target)
            {
                // Get the called method name.
                if (expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    target = memberAccessExpression.Expression is ThisExpressionSyntax ? null : memberAccessExpression.Expression;
                    return memberAccessExpression.Name;
                }
                else if (expression is SimpleNameSyntax simpleName)
                {
                    target = null;
                    return simpleName;
                }
                else if (expression is MemberBindingExpressionSyntax memberBindingExpression)
                {
                    target = null;
                    return memberBindingExpression.Name;
                }
                else
                {
                    target = null;
                    return null;
                }
            }
        }
    }
}
