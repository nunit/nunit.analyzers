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

        public static bool? IsTrueOrFalse(this ConstraintExpressionPart constraintExpressionPart)
        {
            int notCount = constraintExpressionPart.GetPrefixesNames().Count(name => name == NUnitFrameworkConstants.NameOfIsNot);
            bool not = notCount % 2 == 1;

            return constraintExpressionPart.GetConstraintName() switch
            {
                NUnitFrameworkConstants.NameOfIsTrue => !not,
                NUnitFrameworkConstants.NameOfIsFalse => not,
                _ => null,
            };
        }
    }
}
