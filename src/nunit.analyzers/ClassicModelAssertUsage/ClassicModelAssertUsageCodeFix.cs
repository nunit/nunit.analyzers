using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    public abstract class ClassicModelAssertUsageCodeFix
        : CodeFixProvider
    {
        internal const string TransformToConstraintModelDescription = "Transform to constraint model";

        protected virtual int MinimumNumberOfParameters { get; } = 2;

        protected virtual string Title => TransformToConstraintModelDescription;

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var invocationNode = node as InvocationExpressionSyntax;
            if (invocationNode is null)
                return;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel is null)
                return;

            // Replace the original ClassicAssert.<Method> invocation name into Assert.That
            var newInvocationNode = invocationNode.UpdateClassicAssertToAssertThat(out TypeArgumentListSyntax? typeArguments);
            if (newInvocationNode is null)
                return;

            var methodSymbol = semanticModel.GetSymbolInfo(invocationNode).Symbol as IMethodSymbol;
            if (methodSymbol is null)
                return;

            var (argumentNamesToArguments, args) = SplitUpOtherParametersAndParamParameter(methodSymbol, invocationNode);

            // Now, replace the arguments.
            List<ArgumentSyntax> newArguments = new();

            // Do the rule specific conversion
            var (actualArgument, constraintArgument) = typeArguments is null
                ? this.ConstructActualAndConstraintArguments(diagnostic, argumentNamesToArguments)
                : this.ConstructActualAndConstraintArguments(diagnostic, argumentNamesToArguments, typeArguments);
            newArguments.Add(actualArgument);
            if (constraintArgument is not null)
                newArguments.Add(constraintArgument);

            // Do the format spec, params to formattable string conversion
            bool argsIsArray = !string.IsNullOrEmpty(diagnostic.Properties[AnalyzerPropertyKeys.ArgsIsArray]);

            argumentNamesToArguments.TryGetValue(NUnitFrameworkConstants.NameOfMessageParameter, out ArgumentSyntax? messageArgument);
            if (CodeFixHelper.GetInterpolatedMessageArgumentOrDefault(messageArgument, args, unconditional: false, argsIsArray) is ArgumentSyntax interpolatedMessageArgument)
                newArguments.Add(interpolatedMessageArgument);

            // Fix trailing trivia for the first and the last argument
            if (newArguments.Count > 1)
                newArguments[0] = newArguments[0].WithoutTrailingTrivia();
            var lastIndex = newArguments.Count - 1;
            var previousLastArgument = invocationNode.ArgumentList.Arguments.Last();
            newArguments[lastIndex] = newArguments[lastIndex].WithTrailingTrivia(previousLastArgument.GetTrailingTrivia());

            var newArgumentsList = invocationNode.ArgumentList.WithArguments(newArguments);
            newInvocationNode = newInvocationNode.WithArgumentList(newArgumentsList);

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(invocationNode, newInvocationNode);

            context.RegisterCodeFix(
                CodeAction.Create(
                    this.Title,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    this.Title),
                diagnostic);

            (Dictionary<string, ArgumentSyntax> argumentNamesToArguments, List<ArgumentSyntax> args) SplitUpOtherParametersAndParamParameter(
                IMethodSymbol methodSymbol,
                InvocationExpressionSyntax invocationNode)
            {
                Dictionary<string, ArgumentSyntax> argumentNamesToArguments = new();

                // There can be 0 to any number of arguments mapped to args.
                List<ArgumentSyntax> args = new();

                var arguments = invocationNode.ArgumentList.Arguments.ToList();
                for (var i = 0; i < arguments.Count; ++i)
                {
                    var argument = arguments[i];
                    if (i < methodSymbol.Parameters.Length
                        && (argument.NameColon?.Name.Identifier.Text ?? methodSymbol.Parameters[i].Name) is string argumentName
                        && argumentName != NUnitFrameworkConstants.NameOfArgsParameter)
                    {
                        // See if we need to cast the arguments when they were using a specific classic overload.
                        argumentNamesToArguments[argumentName] = argumentName is NUnitFrameworkConstants.NameOfMessageParameter
                            ? argument
                            : CastIfNecessary(argument);
                    }
                    else
                    {
                        args.Add(argument);
                    }
                }

                return (argumentNamesToArguments, args);
            }

            ArgumentSyntax CastIfNecessary(ArgumentSyntax argument)
            {
                string? implicitTypeConversion = GetUserDefinedImplicitTypeConversionOrDefault(argument.Expression);
                if (implicitTypeConversion is null)
                    return argument;

                // Assert.That only expects objects whilst the classic methods have overloads
                // Add an explicit cast operation for the first argument.
                return SyntaxFactory.Argument(SyntaxFactory.CastExpression(
                    SyntaxFactory.ParseTypeName(implicitTypeConversion),
                    argument.Expression));
            }

            string? GetUserDefinedImplicitTypeConversionOrDefault(ExpressionSyntax expression)
            {
                var typeInfo = semanticModel.GetTypeInfo(expression, context.CancellationToken);
                var convertedType = typeInfo.ConvertedType;
                if (convertedType is null)
                {
                    return null;
                }

                var conversion = semanticModel.ClassifyConversion(expression, convertedType);

                if (!conversion.IsUserDefined)
                {
                    return null;
                }

                return convertedType.ToString();
            }
        }

        // ConstraintArgument is nullable because Assert.True and Assert.IsTrue are transformed into Assert.That without a constraint.
        protected virtual (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments) =>
            throw new NotImplementedException($"Class must override {nameof(ConstructActualAndConstraintArguments)}");

        protected virtual (ArgumentSyntax ActualArgument, ArgumentSyntax ConstraintArgument) ConstructActualAndConstraintArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments,
            TypeArgumentListSyntax typeArguments) =>
            throw new InvalidOperationException($"Class must override {nameof(ConstructActualAndConstraintArguments)} accepting {nameof(TypeArgumentListSyntax)}");
    }
}
