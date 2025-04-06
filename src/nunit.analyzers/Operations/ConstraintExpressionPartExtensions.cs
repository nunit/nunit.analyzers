using System.Linq;
using NUnit.Analyzers.Constants;

namespace NUnit.Analyzers.Operations
{
    internal static class ConstraintExpressionPartExtensions
    {
        public static bool HasCustomComparer(this ConstraintExpressionPart constraintPartExpression)
        {
            return constraintPartExpression.GetSuffixesNames().Any(s => s == NUnitFrameworkConstants.NameOfEqualConstraintUsing);
        }
    }
}
