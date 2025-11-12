namespace NUnit.Analyzers.Operations
{
    internal static class ConstraintExpressionExtensions
    {
        public static bool? IsTrueOrFalse(this ConstraintExpression constraintExpression)
        {
            return constraintExpression.ConstraintParts.Length switch
            {
                0 => true, // No constraint parts means "Is.True"
                1 => constraintExpression.ConstraintParts[0].IsTrueOrFalse(),
                _ => null
            };
        }
    }
}
