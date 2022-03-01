using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace NUnit.Analyzers.Extensions
{
    public static class IOperationExtensions
    {
        public static string? GetName(this IOperation @this)
        {
            return @this switch
            {
                IMemberReferenceOperation memberReference => memberReference.Member.Name,
                IInvocationOperation invocation => invocation.TargetMethod?.Name,
                _ => null
            };
        }

        public static IOperation? GetInstance(this IOperation @this)
        {
            return @this switch
            {
                IMemberReferenceOperation memberReference => memberReference.Instance,
                IInvocationOperation invocation => invocation.Instance,
                _ => null
            };
        }

        public static IEnumerable<IOperation> SplitCallChain(this IOperation @this)
        {
            var stack = new Stack<IOperation>();

            var current = @this;
            while (current is not null)
            {
                stack.Push(current);
                current = current.GetInstance();
            }

            return stack;
        }

        public static IOperation? GetArgumentOperation(this IInvocationOperation @this, string parameterName)
        {
            var argument = @this.Arguments.FirstOrDefault(a => a.Parameter?.Name == parameterName)?.Value;

            if (argument is IConversionOperation { IsImplicit: true, Conversion: { IsUserDefined: false } } conversionOperation)
                argument = conversionOperation.Operand;

            return argument;
        }
    }
}
