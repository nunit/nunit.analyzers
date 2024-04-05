using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            // Replace the original ClassicAssert.<Method> invocation name into Assert.That
            var newInvocationNode = invocationNode.UpdateClassicAssertToAssertThat(out TypeArgumentListSyntax? typeArguments);

            if (newInvocationNode is null)
                return;

            var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocationNode).Symbol!;

            var (argumentNamesToArguments, args) = SplitUpNamedParametersAndArgs();
            var arguments = invocationNode.ArgumentList.Arguments.ToList();

            // Remove null message to avoid ambiguous calls.
            if (argumentNamesToArguments.TryGetValue(NUnitFrameworkConstants.NameOfMessageParameter, out ArgumentSyntax? messageArgument)
                && messageArgument.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                argumentNamesToArguments.Remove(NUnitFrameworkConstants.NameOfMessageParameter);
            }

            // Now, replace the arguments.
            List<ArgumentSyntax> newArguments = new();

            // Do the rule specific conversion
            if (typeArguments is null)
            {
                var (actualArgument, constraintArgument) = this.UpdateArguments(diagnostic, argumentNamesToArguments);
                newArguments.Add(actualArgument);
                newArguments.Add(constraintArgument);
                if (CodeFixHelper.GetInterpolatedMessageArgumentOrDefault(messageArgument, args) is ArgumentSyntax interpolatedMessageArgument)
                {
                    newArguments.Add(interpolatedMessageArgument);
                }
            }
            else
            {
                this.UpdateArguments(diagnostic, arguments, typeArguments);

                // Do the format spec, params to formattable string conversion
                CodeFixHelper.UpdateStringFormatToFormattableString(arguments, this.MinimumNumberOfParameters);
                newArguments = arguments;
            }

            var newArgumentsList = invocationNode.ArgumentList.WithArguments(newArguments);
            newInvocationNode = newInvocationNode.WithArgumentList(newArgumentsList);

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(invocationNode, newInvocationNode);

            context.RegisterCodeFix(
                CodeAction.Create(
                    this.Title,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    this.Title), diagnostic);

            (Dictionary<string, ArgumentSyntax> argumentNamesToArguments, List<ArgumentSyntax> args) SplitUpNamedParametersAndArgs()
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
                        argumentNamesToArguments[argumentName] =
                            argumentName is NUnitFrameworkConstants.NameOfExpectedParameter or NUnitFrameworkConstants.NameOfActualParameter
                                ? CastIfNecessary(argument)
                                : argument;
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
                string? implicitTypeConversion = GetUserDefinedImplicitTypeConversion(argument.Expression);
                if (implicitTypeConversion is null)
                    return argument;

                // Assert.That only expects objects whilst the classic methods have overloads
                // Add an explicit cast operation for the first argument.
                return SyntaxFactory.Argument(SyntaxFactory.CastExpression(
                    SyntaxFactory.ParseTypeName(implicitTypeConversion),
                    argument.Expression));
            }

            string? GetUserDefinedImplicitTypeConversion(ExpressionSyntax expression)
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

        protected virtual (ArgumentSyntax ActualArgument, ArgumentSyntax ConstraintArgument) UpdateArguments(
            Diagnostic diagnostic,
            IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments)
        {
            throw new NotImplementedException($"Class must override {nameof(UpdateArguments)}");
        }

        protected virtual void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments, TypeArgumentListSyntax typeArguments)
        {
            throw new InvalidOperationException($"Class must override {nameof(UpdateArguments)} accepting {nameof(TypeArgumentListSyntax)}");
        }

        protected virtual void UpdateArguments(Diagnostic diagnostic, List<ArgumentSyntax> arguments)
        {
            throw new InvalidOperationException($"Class must override {nameof(UpdateArguments)}");
        }
    }
}
