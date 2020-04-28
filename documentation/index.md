# Analyzers #

| Id       | Title       | Enabled by Default |
| :--      | :--         | :--:               |
| [NUnit1001](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1001.md)| The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method. | :white_check_mark: |
| [NUnit1002](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1002.md)| TestCaseSource should use nameof operator to specify target. | :white_check_mark: |
| [NUnit1003](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1003.md)| Too few arguments provided by TestCaseAttribute. | :white_check_mark: |
| [NUnit1004](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1004.md)| Too many arguments provided by TestCaseAttribute. | :white_check_mark: |
| [NUnit1005](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1005.md)| The type of ExpectedResult must match the return type. | :white_check_mark: |
| [NUnit1006](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1006.md)| ExpectedResult must not be specified when the method returns void. | :white_check_mark: |
| [NUnit1007](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1007.md)| Method has non-void return type, but no result is expected in ExpectedResult. | :white_check_mark: |
| [NUnit1008](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1008.md)| Specifying ParallelScope.Self on assembly level has no effect. | :white_check_mark: |
| [NUnit1009](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1009.md)| No ParallelScope.Children on a non-parameterized test method. | :white_check_mark: |
| [NUnit1010](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1010.md)| No ParallelScope.Fixtures on a test method. | :white_check_mark: |
| [NUnit1011](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1011.md)| TestCaseSource argument does not specify an existing member. | :white_check_mark: |
| [NUnit1012](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1012.md)| Async test method must have non-void return type. | :white_check_mark: |
| [NUnit1013](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1013.md)| Async test method must have non-generic Task return type when no result is expected. | :white_check_mark: |
| [NUnit1014](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1014.md)| Async test method must have Task<T> return type when a result is expected | :white_check_mark: |
| [NUnit1015](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1015.md)| Source type does not implement IEnumerable. | :white_check_mark: |
| [NUnit1016](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1016.md)| Source type does not have a default constructor. | :white_check_mark: |
| [NUnit1017](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1017.md)| Specified source is not static. | :white_check_mark: |
| [NUnit1018](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1018.md)| The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method. | :white_check_mark: |
| [NUnit1019](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1019.md)| The source specified by the TestCaseSource does not return an IEnumerable or a type that implements IEnumerable. | :white_check_mark: |
| [NUnit2001](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2001.md)| Consider using Assert.That(expr, Is.False) instead of Assert.False(expr). | :white_check_mark: |
| [NUnit2002](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2002.md)| Consider using Assert.That(expr, Is.False) instead of Assert.IsFalse(expr). | :white_check_mark: |
| [NUnit2003](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2003.md)| Consider using Assert.That(expr, Is.True) instead of Assert.IsTrue(expr). | :white_check_mark: |
| [NUnit2004](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2004.md)| Consider using Assert.That(expr, Is.True) instead of Assert.True(expr). | :white_check_mark: |
| [NUnit2005](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2005.md)| Consider using Assert.That(expr2, Is.EqualTo(expr1)) instead of Assert.AreEqual(expr1, expr2). | :white_check_mark: |
| [NUnit2006](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2006.md)| Consider using Assert.That(expr2, Is.Not.EqualTo(expr1)) instead of Assert.AreNotEqual(expr1, expr2). | :white_check_mark: |
| [NUnit2007](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2007.md)| Actual value should not be constant. | :white_check_mark: |
| [NUnit2008](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2008.md)| Incorrect IgnoreCase usage. | :white_check_mark: |
| [NUnit2009](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2009.md)| Same value provided as actual and expected argument. | :white_check_mark: |
| [NUnit2010](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2010.md)| Use EqualConstraint. | :white_check_mark: |
| [NUnit2011](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2011.md)| Use ContainsConstraint. | :white_check_mark: |
| [NUnit2012](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2012.md)| Use StartsWithConstraint. | :white_check_mark: |
| [NUnit2013](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2013.md)| Use EndsWithConstraint. | :white_check_mark: |
| [NUnit2014](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2014.md)| Use SomeItemsConstraint. | :white_check_mark: |
| [NUnit2015](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2015.md)| Consider using Assert.That(expr2, Is.SameAs(expr1)) instead of Assert.AreSame(expr1, expr2). | :white_check_mark: |
| [NUnit2016](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2016.md)| Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr). | :white_check_mark: |
| [NUnit2017](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2017.md)| Consider using Assert.That(expr, Is.Null) instead of Assert.IsNull(expr). | :white_check_mark: |
| [NUnit2018](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2018.md)| Consider using Assert.That(expr, Is.Not.Null) instead of Assert.NotNull(expr). | :white_check_mark: |
| [NUnit2019](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2019.md)| Consider using Assert.That(expr, Is.Not.Null) instead of Assert.IsNotNull(expr). | :white_check_mark: |
| [NUnit2020](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2020.md)| Incompatible types for SameAs constraint. | :white_check_mark: |
| [NUnit2021](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2021.md)| Incompatible types for EqualTo constraint. | :white_check_mark: |
| [NUnit2022](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2022.md)| Missing property required for constraint. | :white_check_mark: |
| [NUnit2023](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2023.md)| Invalid NullConstraint usage. | :white_check_mark: |
