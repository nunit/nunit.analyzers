using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.Operations
{
    /// <summary>
    /// Represents part of <see cref="ConstraintExpression"/>, which is combined with others with
    /// binary operators ('&', '|')  or methods ('And', 'Or', 'With').
    /// </summary>
    internal class ConstraintExpressionPart
    {
        private readonly IReadOnlyList<IOperation> callChainOperations;

        public ConstraintExpressionPart(IReadOnlyList<IOperation> callChainOperations)
        {
            this.callChainOperations = callChainOperations;
            (this.HelperClass, this.Prefixes, this.Root, this.Suffixes) = SplitExpressions(callChainOperations);
        }

        /// <summary>
        /// Gets helper class used to access constraints,
        /// e.g. 'Has', 'Is', 'Throws', 'Does', etc.
        /// </summary>
        public ITypeSymbol? HelperClass { get; }

        /// <summary>
        /// Gets constraint modifiers that go before actual constraint,
        /// e.g. 'Not', 'Some', 'Property(propertyName)', etc.
        /// </summary>
        public IReadOnlyCollection<IOperation> Prefixes { get; }

        /// <summary>
        /// Gets actual constraint, e.g. 'EqualTo(expected)', 'Null', 'Empty', etc.
        /// </summary>
        public IOperation? Root { get; }

        /// <summary>
        /// Gets constraint modifiers that go after actual constraint,
        /// e.g. 'IgnoreCase', 'After(timeout)', 'Within(range)', etc.
        /// </summary>
        public IReadOnlyCollection<IOperation> Suffixes { get; }

        public TextSpan Span
        {
            get
            {
                // If constraint is built using And/ Or methods, and current part is second operand,
                // we need to cut after operator.
                var operation = this.callChainOperations[0];
                var syntax = operation.Syntax;

                int spanStart = syntax.SpanStart;

                // If instance is null - that means we're accessing static helper class, no need to cut anything.
                if (operation.GetInstance() != null)
                {
                    if (syntax is MemberAccessExpressionSyntax memberAccess)
                        spanStart = memberAccess.Name.Span.Start;
                    else if (syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax invokedMemberAccess })
                        spanStart = invokedMemberAccess.Name.Span.Start;
                }

                var spanEnd = this.callChainOperations.Last().Syntax.Span.End;

                return TextSpan.FromBounds(spanStart, spanEnd);
            }
        }

        /// <summary>
        /// Returns prefixes names (i.e. method or property names).
        /// </summary>
        public string[] GetPrefixesNames()
        {
            return this.Prefixes
                .Select(e => e.GetName())
                .Where(e => e != null)
                .ToArray()!;
        }

        /// <summary>
        /// Returns suffixes names (i.e. method or property names).
        /// </summary>
        public string[] GetSuffixesNames()
        {
            return this.Suffixes
                .Select(e => e.GetName())
                .Where(e => e != null)
                .ToArray()!;
        }

        /// <summary>
        /// Returns prefix operation with provided name, or null, if none found.
        /// </summary>
        public IOperation? GetPrefix(string name)
        {
            return this.Prefixes.FirstOrDefault(p => p.GetName() == name);
        }

        /// <summary>
        /// Returns suffix operation with provided name, or null, if none found.
        /// </summary>
        public IOperation? GetSuffix(string name)
        {
            return this.Suffixes.FirstOrDefault(s => s.GetName() == name);
        }

        /// <summary>
        /// Returns constraint root name (i.e. method or property name).
        /// </summary>
        public string? GetConstraintName()
        {
            return this.Root?.GetName();
        }

        /// <summary>
        /// If constraint root is method, returns expected argument operation.
        /// Otherwise - null.
        /// </summary>
        public IOperation? GetExpectedArgument()
        {
            var argument = (this.Root as IInvocationOperation)?.Arguments.FirstOrDefault()?.Value;

            if (argument is IConversionOperation conversion)
                argument = conversion.Operand;

            return argument;
        }

        /// <summary>
        /// If constraint root is method, return corresponding <see cref="IMethodSymbol"/>.
        /// Otherwise (e.g. if constraint root is property) - null.
        /// </summary>
        public IMethodSymbol? GetConstraintMethod()
        {
            return (this.Root as IInvocationOperation)?.TargetMethod;
        }

        /// <summary>
        /// Returns true if part has any conditional or variable expressions.
        /// </summary>
        public bool HasUnknownExpressions()
        {
            foreach (var part in this.callChainOperations)
            {
                switch (part)
                {
                    // e.g. '.EqualTo(1)'
                    case IInvocationOperation _:
                    // e.g. '.IgnoreCase'
                    case IMemberReferenceOperation _:
                        break;
                    default:
                        return true;
                }
            }

            return false;
        }

        public Location GetLocation()
        {
            var syntaxTree = this.callChainOperations[0].Syntax.SyntaxTree;
            return Location.Create(syntaxTree, this.Span);
        }

        public override string ToString()
        {
            var operationSyntax = this.callChainOperations.Last().Syntax;
            var startDelta = this.Span.Start - operationSyntax.Span.Start;

            return operationSyntax.ToString().Substring(startDelta);
        }

        private static (ITypeSymbol? helperClass, IReadOnlyCollection<IOperation> prefixes, IOperation? root, IReadOnlyCollection<IOperation> suffixes)
            SplitExpressions(IReadOnlyList<IOperation> callChainOperations)
        {
            ITypeSymbol? helperClass;
            var prefixes = new List<IOperation>();
            IOperation? root = null;
            var suffixes = new List<IOperation>();

            var rootFound = false;

            helperClass = callChainOperations[0] switch
            {
                IMemberReferenceOperation memberReference when memberReference.Instance is null => memberReference.Member.ContainingType,
                IInvocationOperation invocation when invocation.Instance is null => invocation.TargetMethod?.ContainingType,
                _ => null
            };

            foreach (var operation in callChainOperations)
            {
                if (!rootFound)
                {
                    if (!ReturnsConstraint(operation))
                    {
                        // Prefixes - take expressions until found the expression that returns a constraint.
                        prefixes.Add(operation);
                    }
                    else
                    {
                        // Root - first expression that returns a constraint.
                        root = operation;
                        rootFound = true;
                    }
                }
                else
                {
                    // Suffixes - everything after root.
                    suffixes.Add(operation);
                }
            }

            return (helperClass, prefixes, root, suffixes);
        }

        private static bool ReturnsConstraint(IOperation operation)
        {
            var returnType = operation.Type;
            return returnType != null && returnType.IsConstraint();
        }
    }
}
