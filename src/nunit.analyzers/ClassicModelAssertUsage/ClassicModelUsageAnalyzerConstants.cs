namespace NUnit.Analyzers.ClassicModelAssertUsage
{
    internal static class ClassicModelUsageAnalyzerConstants
    {
        internal const string IsTrueTitle = "Consider using Assert.That(expr, Is.True) instead of ClassicAssert.IsTrue(expr)";
        internal const string IsTrueMessage = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, ClassicAssert.IsTrue(expr)";
        internal const string IsTrueDescription = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, ClassicAssert.IsTrue(expr).";

        internal const string TrueTitle = "Consider using Assert.That(expr, Is.True) instead of ClassicAssert.True(expr)";
        internal const string TrueMessage = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, ClassicAssert.True(expr)";
        internal const string TrueDescription = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, ClassicAssert.True(expr).";

        internal const string IsFalseTitle = "Consider using Assert.That(expr, Is.False) instead of ClassicAssert.IsFalse(expr)";
        internal const string IsFalseMessage = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, ClassicAssert.IsFalse(expr)";
        internal const string IsFalseDescription = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, ClassicAssert.IsFalse(expr).";

        internal const string FalseTitle = "Consider using Assert.That(expr, Is.False) instead of ClassicAssert.False(expr)";
        internal const string FalseMessage = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, ClassicAssert.False(expr)";
        internal const string FalseDescription = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, ClassicAssert.False(expr).";

        internal const string AreEqualTitle = "Consider using Assert.That(actual, Is.EqualTo(expected)) instead of ClassicAssert.AreEqual(expected, actual)";
        internal const string AreEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.EqualTo(expected)), instead of the classic model, ClassicAssert.AreEqual(expected, actual)";
        internal const string AreEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.EqualTo(expected)), instead of the classic model, ClassicAssert.AreEqual(expected, actual).";

        internal const string AreNotEqualTitle = "Consider using Assert.That(actual, Is.Not.EqualTo(expected)) instead of ClassicAssert.AreNotEqual(expected, actual)";
        internal const string AreNotEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.Not.EqualTo(expected)), instead of the classic model, ClassicAssert.AreNotEqual(expected, actual)";
        internal const string AreNotEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.Not.EqualTo(expected)), instead of the classic model, ClassicAssert.AreNotEqual(expected, actual).";

        internal const string AreSameTitle = "Consider using Assert.That(actual, Is.SameAs(expected)) instead of ClassicAssert.AreSame(expected, actual)";
        internal const string AreSameMessage = "Consider using the constraint model, Assert.That(actual, Is.SameAs(expected)), instead of the classic model, ClassicAssert.AreSame(expected, actual)";
        internal const string AreSameDescription = "Consider using the constraint model, Assert.That(actual, Is.SameAs(expected)), instead of the classic model, ClassicAssert.AreSame(expected, actual).";

        internal const string IsNullTitle = "Consider using Assert.That(expr, Is.Null) instead of ClassicAssert.IsNull(expr)";
        internal const string IsNullMessage = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, ClassicAssert.IsNull(expr)";
        internal const string IsNullDescription = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, ClassicAssert.IsNull(expr).";

        internal const string NullTitle = "Consider using Assert.That(expr, Is.Null) instead of ClassicAssert.Null(expr)";
        internal const string NullMessage = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, ClassicAssert.Null(expr)";
        internal const string NullDescription = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, ClassicAssert.Null(expr).";

        internal const string IsNotNullTitle = "Consider using Assert.That(expr, Is.Not.Null) instead of ClassicAssert.IsNotNull(expr)";
        internal const string IsNotNullMessage = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, ClassicAssert.IsNotNull(expr)";
        internal const string IsNotNullDescription = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, ClassicAssert.IsNotNull(expr).";

        internal const string NotNullTitle = "Consider using Assert.That(expr, Is.Not.Null) instead of ClassicAssert.NotNull(expr)";
        internal const string NotNullMessage = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, ClassicAssert.NotNull(expr)";
        internal const string NotNullDescription = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, ClassicAssert.NotNull(expr).";

        internal const string GreaterTitle = "Consider using Assert.That(actual, Is.GreaterThan(expected)) instead of ClassicAssert.Greater(actual, expected)";
        internal const string GreaterMessage = "Consider using the constraint model, Assert.That(actual, Is.GreaterThan(expected)), instead of the classic model, ClassicAssert.Greater(actual, expected)";
        internal const string GreaterDescription = "Consider using the constraint model, Assert.That(actual, Is.GreaterThan(expected)), instead of the classic model, ClassicAssert.Greater(actual, expected).";

        internal const string GreaterOrEqualTitle = "Consider using Assert.That(actual, Is.GreaterThanOrEqualTo(expected)) instead of ClassicAssert.GreaterOrEqual(actual, expected)";
        internal const string GreaterOrEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.GreaterThanOrEqualTo(expected)), instead of the classic model, ClassicAssert.GreaterOrEqual(actual, expected)";
        internal const string GreaterOrEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.GreaterThanOrEqualTo(expected)), instead of the classic model, ClassicAssert.GreaterOrEqual(actual, expected).";

        internal const string LessTitle = "Consider using Assert.That(actual, Is.LessThan(expected)) instead of ClassicAssert.Less(actual, expected)";
        internal const string LessMessage = "Consider using the constraint model, Assert.That(actual, Is.LessThan(expected)), instead of the classic model, ClassicAssert.Less(actual, expected)";
        internal const string LessDescription = "Consider using the constraint model, Assert.That(actual, Is.LessThan(expected)), instead of the classic model, ClassicAssert.Less(actual, expected).";

        internal const string LessOrEqualTitle = "Consider using Assert.That(actual, Is.LessThanOrEqualTo(expected)) instead of ClassicAssert.LessOrEqual(actual, expected)";
        internal const string LessOrEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.LessThanOrEqualTo(expected)), instead of the classic model, ClassicAssert.LessOrEqual(actual, expected)";
        internal const string LessOrEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.LessThanOrEqualTo(expected)), instead of the classic model, ClassicAssert.LessOrEqual(actual, expected).";

        internal const string AreNotSameTitle = "Consider using Assert.That(actual, Is.Not.SameAs(expected)) instead of ClassicAssert.AreNotSame(expected, actual)";
        internal const string AreNotSameMessage = "Consider using the constraint model, Assert.That(actual, Is.Not.SameAs(expected)), instead of the classic model, ClassicAssert.AreNotSame(expected, actual)";
        internal const string AreNotSameDescription = "Consider using the constraint model, Assert.That(actual, Is.Not.SameAs(expected)), instead of the classic model, ClassicAssert.AreNotSame(expected, actual).";

        internal const string ZeroTitle = "Consider using Assert.That(expr, Is.Zero) instead of ClassicAssert.Zero(expr)";
        internal const string ZeroMessage = "Consider using the constraint model, Assert.That(expr, Is.Zero), instead of the classic model, ClassicAssert.Zero(expr)";
        internal const string ZeroDescription = "Consider using the constraint model, Assert.That(expr, Is.Zero), instead of the classic model, ClassicAssert.Zero(expr).";

        internal const string NotZeroTitle = "Consider using Assert.That(expr, Is.Not.Zero) instead of ClassicAssert.NotZero(expr)";
        internal const string NotZeroMessage = "Consider using the constraint model, Assert.That(expr, Is.Not.Zero), instead of the classic model, ClassicAssert.NotZero(expr)";
        internal const string NotZeroDescription = "Consider using the constraint model, Assert.That(expr, Is.Not.Zero), instead of the classic model, ClassicAssert.NotZero(expr).";

        internal const string IsNaNTitle = "Consider using Assert.That(expr, Is.NaN) instead of ClassicAssert.IsNaN(expr)";
        internal const string IsNaNMessage = "Consider using the constraint model, Assert.That(expr, Is.NaN), instead of the classic model, ClassicAssert.IsNaN(expr)";
        internal const string IsNaNDescription = "Consider using the constraint model, Assert.That(expr, Is.NaN), instead of the classic model, ClassicAssert.IsNaN(expr).";

        internal const string IsEmptyTitle = "Consider using Assert.That(collection, Is.Empty) instead of ClassicAssert.IsEmpty(collection)";
        internal const string IsEmptyMessage = "Consider using the constraint model, Assert.That(collection, Is.Empty), instead of the classic model, ClassicAssert.IsEmpty(collection)";
        internal const string IsEmptyDescription = "Consider using the constraint model, Assert.That(collection, Is.Empty), instead of the classic model, ClassicAssert.IsEmpty(collection).";

        internal const string IsNotEmptyTitle = "Consider using Assert.That(collection, Is.Not.Empty) instead of ClassicAssert.IsNotEmpty(collection)";
        internal const string IsNotEmptyMessage = "Consider using the constraint model, Assert.That(collection, Is.Not.Empty), instead of the classic model, ClassicAssert.IsNotEmpty(collection)";
        internal const string IsNotEmptyDescription = "Consider using the constraint model, Assert.That(collection, Is.Not.Empty), instead of the classic model, ClassicAssert.IsNotEmpty(collection).";

        internal const string ContainsTitle = "Consider using Assert.That(collection, Does.Contain(instance)) instead of ClassicAssert.Contains(instance, collection)";
        internal const string ContainsMessage = "Consider using the constraint model, Assert.That(collection, Does.Contain(instance)), instead of the classic model, ClassicAssert.Contains(instance, collection)";
        internal const string ContainsDescription = "Consider using the constraint model, Assert.That(collection, Does.Contain(instance)), instead of the classic model, ClassicAssert.Contains(instance, collection).";

        internal const string IsInstanceOfTitle = "Consider using Assert.That(actual, Is.InstanceOf(expected)) instead of ClassicAssert.IsInstanceOf(expected, actual)";
        internal const string IsInstanceOfMessage = "Consider using the constraint model, Assert.That(actual, Is.InstanceOf(expected)), instead of the classic model, ClassicAssert.IsInstanceOf(expected, actual)";
        internal const string IsInstanceOfDescription = "Consider using the constraint model, Assert.That(actual, Is.InstanceOf(expected)), instead of the classic model, ClassicAssert.IsInstanceOf(expected, actual).";

        internal const string IsNotInstanceOfTitle = "Consider using Assert.That(actual, Is.Not.InstanceOf(expected)) instead of ClassicAssert.IsNotInstanceOf(expected, actual)";
        internal const string IsNotInstanceOfMessage = "Consider using the constraint model, Assert.That(actual, Is.Not.InstanceOf(expected)), instead of the classic model, ClassicAssert.IsNotInstanceOf(expected, actual)";
        internal const string IsNotInstanceOfDescription = "Consider using the constraint model, Assert.That(actual, Is.Not.InstanceOf(expected)), instead of the classic model, ClassicAssert.IsNotInstanceOf(expected, actual).";

        internal const string PositiveTitle = "Consider using Assert.That(expr, Is.Positive) instead of ClassicAssert.Positive(expr)";
        internal const string PositiveMessage = "Consider using the constraint model, Assert.That(expr, Is.Positive), instead of the classic model, ClassicAssert.Positive(expr)";
        internal const string PositiveDescription = "Consider using the constraint model, Assert.That(expr, Is.Positive), instead of the classic model, ClassicAssert.Positive(expr).";

        internal const string NegativeTitle = "Consider using Assert.That(expr, Is.Negative) instead of ClassicAssert.Negative(expr)";
        internal const string NegativeMessage = "Consider using the constraint model, Assert.That(expr, Is.Negative), instead of the classic model, ClassicAssert.Negative(expr)";
        internal const string NegativeDescription = "Consider using the constraint model, Assert.That(expr, Is.Negative), instead of the classic model, ClassicAssert.Negative(expr).";

        internal const string IsAssignableFromTitle = "Consider using Assert.That(actual, Is.AssignableFrom(expected)) instead of ClassicAssert.IsAssignableFrom(expected, actual)";
        internal const string IsAssignableFromMessage = "Consider using the constraint model, Assert.That(actual, Is.AssignableFrom(expected)), instead of the classic model, ClassicAssert.IsAssignableFrom(expected, actual)";
        internal const string IsAssignableFromDescription = "Consider using the constraint model, Assert.That(actual, Is.AssignableFrom(expected)), instead of the classic model, ClassicAssert.IsAssignableFrom(expected, actual).";

        internal const string IsNotAssignableFromTitle = "Consider using Assert.That(actual, Is.Not.AssignableFrom(expected)) instead of ClassicAssert.IsNotAssignableFrom(expected, actual)";
        internal const string IsNotAssignableFromMessage = "Consider using the constraint model, Assert.That(actual, Is.Not.AssignableFrom(expected)), instead of the classic model, ClassicAssert.IsNotAssignableFrom(expected, actual)";
        internal const string IsNotAssignableFromDescription = "Consider using the constraint model, Assert.That(actual, Is.Not.AssignableFrom(expected)), instead of the classic model, ClassicAssert.IsNotAssignableFrom(expected, actual).";
    }
}
