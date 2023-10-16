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

            var invocationIdentifier = diagnostic.Properties[AnalyzerPropertyKeys.ModelName];
            var isGenericMethod = diagnostic.Properties[AnalyzerPropertyKeys.IsGenericMethod] == true.ToString();

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

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

            // Replace the original ClassicAssert.<Method> invocation name into Assert.That
            var newInvocationNode = invocationNode.UpdateClassicAssertToAssertThat(out TypeArgumentListSyntax? typeArguments);

            if (newInvocationNode is null)
                return;

            // Now, replace the arguments.
            List<ArgumentSyntax> arguments = invocationNode.ArgumentList.Arguments.ToList();

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

            // See if we need to cast the arguments when they were using a specific classic overload.
            arguments[0] = CastIfNecessary(arguments[0]);
            if (arguments.Count > 1)
                arguments[1] = CastIfNecessary(arguments[1]);

            // Do the rule specific conversion
            if (typeArguments is null)
                this.UpdateArguments(diagnostic, arguments);
            else
                this.UpdateArguments(diagnostic, arguments, typeArguments);

            // Do the format spec, params to formattable string conversion
            CodeFixHelper.UpdateStringFormatToFormattableString(arguments, MinimumNumberOfParameters);

            var newArgumentsList = invocationNode.ArgumentList.WithArguments(arguments);
            newInvocationNode = newInvocationNode.WithArgumentList(newArgumentsList);

            context.CancellationToken.ThrowIfCancellationRequested();

            var newRoot = root.ReplaceNode(invocationNode, newInvocationNode);

            context.RegisterCodeFix(
                CodeAction.Create(
                    this.Title,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                    this.Title), diagnostic);
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
