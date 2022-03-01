using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;
using NUnit.Analyzers.Helpers;
using NUnit.Analyzers.Operations;

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
                    !char.IsLetterOrDigit(argument[previousArgument.Length]))
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
                Add(previousArguments, assertOperation.Arguments[0].Syntax.ToString());

                int firstAssert = -1;
                int lastAssert = -1;

                // Find this expression in the list.
                int i = -1;
                foreach (var statement in blockOperation.Operations)
                {
                    i++;
                    if (statement == assertExpression)
                    {
                        firstAssert = lastAssert = i;
                    }
                    else if (firstAssert >= 0)
                    {
                        IInvocationOperation? currentAssertOperation = TryGetAssertThatOperation(statement);
                        if (currentAssertOperation is not null)
                        {
                            // No need to check argument count as Assert.That needs at least one argument.
                            string currentArgument = currentAssertOperation.Arguments[0].Syntax.ToString();

                            // Check if test is independent
                            if (IsIndependent(previousArguments, currentArgument))
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
                            break;
                        }
                    }
                }

                if (lastAssert > firstAssert)
                {
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, assertOperation.Syntax.GetLocation()));
                }
            }
        }

        private static IInvocationOperation? TryGetAssertThatOperation(IOperation operation)
        {
            return (operation is IExpressionStatementOperation expressionOperation &&
                expressionOperation.Operation is IInvocationOperation invocationOperation &&
                IsAssert(invocationOperation) &&
                invocationOperation.TargetMethod.Name == NUnitFrameworkConstants.NameOfAssertThat)
                ? invocationOperation : null;
        }
    }
}
