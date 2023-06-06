using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;

namespace NUnit.Analyzers.UseAssertMultiple
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseAssertMultipleAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.UseAssertMultiple,
            title: UseAssertMultipleConstants.Title,
            messageFormat: UseAssertMultipleConstants.Message,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: UseAssertMultipleConstants.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(descriptor);

        /// <summary>
        /// Assert.That statements are considered independent if an argument is not suffixed later
        /// e.g.:
        ///     Assert.That(instance.Property1, Is.EqualTo(0));
        ///     Assert.That(instance.Property2, Is.EqualTo(0));
        ///     Assert.That(instance.Property12, Is.EqualTo(0));
        /// But not:
        ///     Assert.That(instance, Is.Not.Null);
        ///     Assert.That(instance, Has.Length.EqualTo(1));
        ///     Assert.That(instance.Length, Is.EqualTo(1));
        ///     Assert.That(instance[0], Is.EqualTo(' '));
        /// Or if they are more complex than that.
        /// </summary>
        internal static bool IsIndependent(HashSet<string> previousArguments, string argument)
        {
            if (previousArguments.Contains(argument))
            {
                return false;
            }

            foreach (var previousArgument in previousArguments)
            {
                if (argument.StartsWith(previousArgument, StringComparison.Ordinal) &&
                   (argument.Length > previousArgument.Length && !char.IsLetterOrDigit(argument[previousArgument.Length])))
                {
                    return false;
                }
            }

            Add(previousArguments, argument);
            return true;
        }

        internal static void Add(HashSet<string> previousArguments, string argument)
        {
            previousArguments.Add(argument);
            if (argument.EndsWith(".Length", StringComparison.Ordinal) ||
                argument.EndsWith(".Count", StringComparison.Ordinal))
            {
                previousArguments.Add(argument.Substring(0, argument.LastIndexOf('.')));
            }
        }

        protected override void AnalyzeAssertInvocation(OperationAnalysisContext context, IInvocationOperation assertOperation)
        {
            if (assertOperation.TargetMethod.Name != NUnitFrameworkConstants.NameOfAssertThat ||
                AssertHelper.IsInsideAssertMultiple(assertOperation.Syntax))
            {
                return;
            }

            var assertExpression = assertOperation.Parent as IExpressionStatementOperation;
            if (assertExpression?.Parent is IBlockOperation blockOperation)
            {
                // Check if the next operation is also an Assert invocation.
                var previousArguments = new HashSet<string>(StringComparer.Ordinal);

                // No need to check argument count as Assert.That needs at least one argument.
                var assertArgument = assertOperation.Arguments[0].Syntax.ToString();

                IOperation? statementBefore = null;
                int firstAssert = -1;
                int lastAssert = -1;

                // Find this expression in the list.
                int i = -1;
                foreach (var statement in blockOperation.Operations)
                {
                    i++;
                    if (statement == assertExpression)
                    {
                        if (statementBefore is not null)
                        {
                            var beforeArguments = new HashSet<string>(StringComparer.Ordinal);
                            if (IsIndependentAssert(beforeArguments, statementBefore) &&
                                IsIndependent(beforeArguments, assertArgument))
                            {
                                // This statement can be merged with the previous, hence was reported already.
                                return;
                            }
                        }

                        Add(previousArguments, assertArgument);
                        firstAssert = lastAssert = i;
                    }
                    else if (firstAssert >= 0)
                    {
                        if (IsIndependentAssert(previousArguments, statement))
                        {
                            lastAssert = i;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        statementBefore = statement;
                    }
                }

                if (lastAssert > firstAssert)
                {
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, assertOperation.Syntax.GetLocation()));
                }
            }
        }

        private static bool IsIndependentAssert(HashSet<string> previousArguments, IOperation statement)
        {
            IInvocationOperation? currentAssertOperation = TryGetAssertThatOperation(statement);
            if (currentAssertOperation is not null)
            {
                // No need to check argument count as Assert.That needs at least one argument.
                string currentArgument = currentAssertOperation.Arguments[0].Syntax.ToString();

                // Check if test is independent
                return IsIndependent(previousArguments, currentArgument);
            }

            // Not even an Assert operation.
            return false;
        }

        private static IInvocationOperation? TryGetAssertThatOperation(IOperation operation)
        {
            return (operation is IExpressionStatementOperation expressionOperation &&
                expressionOperation.Operation is IInvocationOperation invocationOperation &&
                invocationOperation.TargetMethod.ContainingType.IsAssert() &&
                invocationOperation.TargetMethod.Name == NUnitFrameworkConstants.NameOfAssertThat)
                ? invocationOperation : null;
        }
    }
}
