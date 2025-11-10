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
            bool not = constraintExpressionPart.GetPrefix(NUnitFrameworkConstants.NameOfIsNot) is not null;

            return constraintExpressionPart.GetConstraintName() switch
            {
                NUnitFrameworkConstants.NameOfIsTrue => !not,
                NUnitFrameworkConstants.NameOfIsFalse => not,
                _ => null,
            };
        }
    }
}
