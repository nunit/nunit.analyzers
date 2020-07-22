namespace NUnit.Analyzers.Constants
{
    internal static class ClassicModelUsageAnalyzerConstants
    {
        internal const string IsTrueTitle = "Consider using Assert.That(expr, Is.True) instead of Assert.IsTrue(expr).";
        internal const string IsTrueMessage = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, Assert.IsTrue(expr).";
        internal const string IsTrueDescription = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, Assert.IsTrue(expr).";

        internal const string TrueTitle = "Consider using Assert.That(expr, Is.True) instead of Assert.True(expr).";
        internal const string TrueMessage = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, Assert.True(expr).";
        internal const string TrueDescription = "Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, Assert.True(expr).";

        internal const string IsFalseTitle = "Consider using Assert.That(expr, Is.False) instead of Assert.IsFalse(expr).";
        internal const string IsFalseMessage = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, Assert.IsFalse(expr).";
        internal const string IsFalseDescription = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, Assert.IsFalse(expr).";

        internal const string FalseTitle = "Consider using Assert.That(expr, Is.False) instead of Assert.False(expr).";
        internal const string FalseMessage = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, Assert.False(expr).";
        internal const string FalseDescription = "Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, Assert.False(expr).";

        internal const string AreEqualTitle = "Consider using Assert.That(actual, Is.EqualTo(expected)) instead of Assert.AreEqual(expected, actual).";
        internal const string AreEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.EqualTo(expected)), instead of the classic model, Assert.AreEqual(expected, actual).";
        internal const string AreEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.EqualTo(expected)), instead of the classic model, Assert.AreEqual(expected, actual).";

        internal const string AreNotEqualTitle = "Consider using Assert.That(actual, Is.Not.EqualTo(expected)) instead of Assert.AreNotEqual(expected, actual).";
        internal const string AreNotEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.Not.EqualTo(expected)), instead of the classic model, Assert.AreNotEqual(expected, actual).";
        internal const string AreNotEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.Not.EqualTo(expected)), instead of the classic model, Assert.AreNotEqual(expected, actual).";

        internal const string AreSameTitle = "Consider using Assert.That(actual, Is.SameAs(expected)) instead of Assert.AreSame(expected, actual).";
        internal const string AreSameMessage = "Consider using the constraint model, Assert.That(actual, Is.SameAs(expected)), instead of the classic model, Assert.AreSame(expected, actual).";
        internal const string AreSameDescription = "Consider using the constraint model, Assert.That(actual, Is.SameAs(expected)), instead of the classic model, Assert.AreSame(expected, actual).";

        internal const string IsNullTitle = "Consider using Assert.That(expr, Is.Null) instead of Assert.IsNull(expr).";
        internal const string IsNullMessage = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, Assert.IsNull(expr).";
        internal const string IsNullDescription = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, Assert.IsNull(expr).";

        internal const string NullTitle = "Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr).";
        internal const string NullMessage = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, Assert.Null(expr).";
        internal const string NullDescription = "Consider using the constraint model, Assert.That(expr, Is.Null), instead of the classic model, Assert.Null(expr).";

        internal const string IsNotNullTitle = "Consider using Assert.That(expr, Is.Not.Null) instead of Assert.IsNotNull(expr).";
        internal const string IsNotNullMessage = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, Assert.IsNotNull(expr).";
        internal const string IsNotNullDescription = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, Assert.IsNotNull(expr).";

        internal const string NotNullTitle = "Consider using Assert.That(expr, Is.Not.Null) instead of Assert.NotNull(expr).";
        internal const string NotNullMessage = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, Assert.NotNull(expr).";
        internal const string NotNullDescription = "Consider using the constraint model, Assert.That(expr, Is.Not.Null), instead of the classic model, Assert.NotNull(expr).";

        internal const string GreaterTitle = "Consider using Assert.That(actual, Is.GreaterThan(expected)) instead of Assert.Greater(actual, expected).";
        internal const string GreaterMessage = "Consider using the constraint model, Assert.That(actual, Is.GreaterThan(expected)), instead of the classic model, Assert.Greater(actual, expected).";
        internal const string GreaterDescription = "Consider using the constraint model, Assert.That(actual, Is.GreaterThan(expected)), instead of the classic model, Assert.Greater(actual, expected).";

        internal const string GreaterOrEqualTitle = "Consider using Assert.That(actual, Is.GreaterThanOrEqualTo(expected)) instead of Assert.GreaterOrEqual(actual, expected).";
        internal const string GreaterOrEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.GreaterThanOrEqualTo(expected)), instead of the classic model, Assert.GreaterOrEqual(actual, expected).";
        internal const string GreaterOrEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.GreaterThanOrEqualTo(expected)), instead of the classic model, Assert.GreaterOrEqual(actual, expected).";

        internal const string LessTitle = "Consider using Assert.That(actual, Is.LessThan(expected)) instead of Assert.Less(actual, expected).";
        internal const string LessMessage = "Consider using the constraint model, Assert.That(actual, Is.LessThan(expected)), instead of the classic model, Assert.Less(actual, expected).";
        internal const string LessDescription = "Consider using the constraint model, Assert.That(actual, Is.LessThan(expected)), instead of the classic model, Assert.Less(actual, expected).";

        internal const string LessOrEqualTitle = "Consider using Assert.That(actual, Is.LessThanOrEqualTo(expected)) instead of Assert.LessOrEqual(actual, expected).";
        internal const string LessOrEqualMessage = "Consider using the constraint model, Assert.That(actual, Is.LessThanOrEqualTo(expected)), instead of the classic model, Assert.LessOrEqual(actual, expected).";
        internal const string LessOrEqualDescription = "Consider using the constraint model, Assert.That(actual, Is.LessThanOrEqualTo(expected)), instead of the classic model, Assert.LessOrEqual(actual, expected).";

        internal const string AreNotSameTitle = "Consider using Assert.That(actual, Is.Not.SameAs(expected)) instead of Assert.AreNotSame(expected, actual).";
        internal const string AreNotSameMessage = "Consider using the constraint model, Assert.That(actual, Is.Not.SameAs(expected)), instead of the classic model, Assert.AreNotSame(expected, actual).";
        internal const string AreNotSameDescription = "Consider using the constraint model, Assert.That(actual, Is.Not.SameAs(expected)), instead of the classic model, Assert.AreNotSame(expected, actual).";

        internal const string ZeroTitle = "Consider using Assert.That(expr, Is.Zero) instead of Assert.Zero(expr).";
        internal const string ZeroMessage = "Consider using the constraint model, Assert.That(expr, Is.Zero), instead of the classic model, Assert.Zero(expr).";
        internal const string ZeroDescription = "Consider using the constraint model, Assert.That(expr, Is.Zero), instead of the classic model, Assert.Zero(expr).";

        internal const string NotZeroTitle = "Consider using Assert.That(expr, Is.Not.Zero) instead of Assert.NotZero(expr).";
        internal const string NotZeroMessage = "Consider using the constraint model, Assert.That(expr, Is.Not.Zero), instead of the classic model, Assert.NotZero(expr).";
        internal const string NotZeroDescription = "Consider using the constraint model, Assert.That(expr, Is.Not.Zero), instead of the classic model, Assert.NotZero(expr).";

        internal const string IsNaNTitle = "Consider using Assert.That(expr, Is.NaN) instead of Assert.IsNaN(expr).";
        internal const string IsNaNMessage = "Consider using the constraint model, Assert.That(expr, Is.NaN), instead of the classic model, Assert.IsNaN(expr).";
        internal const string IsNaNDescription = "Consider using the constraint model, Assert.That(expr, Is.NaN), instead of the classic model, Assert.IsNaN(expr).";

        internal const string IsEmptyTitle = "Consider using Assert.That(collection, Is.Empty) instead of Assert.IsEmpty(collection).";
        internal const string IsEmptyMessage = "Consider using the constraint model, Assert.That(collection, Is.Empty), instead of the classic model, Assert.IsEmpty(collection).";
        internal const string IsEmptyDescription = "Consider using the constraint model, Assert.That(collection, Is.Empty), instead of the classic model, Assert.IsEmpty(collection).";

        internal const string IsNotEmptyTitle = "Consider using Assert.That(collection, Is.Not.Empty) instead of Assert.IsNotEmpty(collection).";
        internal const string IsNotEmptyMessage = "Consider using the constraint model, Assert.That(collection, Is.Not.Empty), instead of the classic model, Assert.IsNotEmpty(collection).";
        internal const string IsNotEmptyDescription = "Consider using the constraint model, Assert.That(collection, Is.Not.Empty), instead of the classic model, Assert.IsNotEmpty(collection).";

        internal const string ContainsTitle = "Consider using Assert.That(collection, Does.Contain(instance)) instead of Assert.Contains(instance, collection).";
        internal const string ContainsMessage = "Consider using the constraint model, Assert.That(collection, Does.Contain(instance)), instead of the classic model, Assert.Contains(instance, collection).";
        internal const string ContainsDescription = "Consider using the constraint model, Assert.That(collection, Does.Contain(instance)), instead of the classic model, Assert.Contains(instance, collection).";

        internal const string IsInstanceOfTitle = "Consider using Assert.That(actual, Is.InstanceOf(expected)) instead of Assert.IsInstanceOf(expected, actual).";
        internal const string IsInstanceOfMessage = "Consider using the constraint model, Assert.That(actual, Is.InstanceOf(expected)), instead of the classic model, Assert.IsInstanceOf(expected, actual).";
        internal const string IsInstanceOfDescription = "Consider using the constraint model, Assert.That(actual, Is.InstanceOf(expected)), instead of the classic model, Assert.IsInstanceOf(expected, actual).";

        internal const string IsNotInstanceOfTitle = "Consider using Assert.That(actual, Is.Not.InstanceOf(expected)) instead of Assert.IsNotInstanceOf(expected, actual).";
        internal const string IsNotInstanceOfMessage = "Consider using the constraint model, Assert.That(actual, Is.Not.InstanceOf(expected)), instead of the classic model, Assert.IsNotInstanceOf(expected, actual).";
        internal const string IsNotInstanceOfDescription = "Consider using the constraint model, Assert.That(actual, Is.Not.InstanceOf(expected)), instead of the classic model, Assert.IsNotInstanceOf(expected, actual).";
    }
}
