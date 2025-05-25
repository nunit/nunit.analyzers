namespace NUnit.Analyzers.RangeUsage
{
    internal static class RangeUsageConstants
    {
        internal const string StepMustNotBeZeroTitle = "The 'step' parameter to Range cannot be zero";
        internal const string StepMustNotBeZeroMessage = "The 'step' parameter to Range cannot be zero";
        internal const string StepMustNotBeZeroDescription = "The 'step' parameter to Range cannot be zero.";

        internal const string InvalidIncrementingRangeTitle = "The value for 'from' must be less than 'to' when 'step' is positive";
        internal const string InvalidIncrementingRangeMessage = "The value for 'from' must be less than 'to' when 'step' is positive";
        internal const string InvalidIncrementingRangeDescription = "Ensure that 'to' is greater than 'from' when 'step' is positive.";

        internal const string InvalidDecrementingRangeTitle = "The value for 'from' must be greater than 'to' when 'step' is negative";
        internal const string InvalidDecrementingRangeMessage = "The value for 'from' must be greater than 'to' when 'step' is negative";
        internal const string InvalidDecrementingRangeDescription = "Ensure that 'from' is greater than 'to' when 'step' is negative.";

        internal const string MismatchParameterTypeTitle = "The type of the attribute values doesn't match the parameter type";
        internal const string MismatchParameterTypeMessage = "The type of the attribute values '{0}' doesn't match the parameter type '{1}'";
        internal const string MismatchParameterTypeDescription = "Ensure that the attribute and parameter types match.";
    }
}
