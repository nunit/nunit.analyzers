# Overview of implemented NUnit diagnostics and code fixes

In the tables below we use the following symbols:

* :mag: - whether the diagnostic is enabled by default or not
* :memo: - the severity of the diagnostic
* :bulb: - whether the diagnostic has an associated code fix or not

The severity of a diagnostic can the one of the following (in increasing severity):

* :thought_balloon: - indicates a severity of **Hidden**
* :information_source: - indicates a severity of **Info**
* :warning: - indicates a severity of **Warning**
* :exclamation: - indicates a severity of **Error**

## Structure Rules (NUnit1001 - )

Rules which enforce structural requirements on the test code.

| Id       | Title       | :mag: | :memo: | :bulb: |
| :--      | :--         | :--:  | :--:   | :--:   |
| [NUnit1001](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1001.md) | The individual arguments provided by a TestCaseAttribute must match the type of the corresponding parameter of the method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1002](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1002.md) | The TestCaseSource should use nameof operator to specify target | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit1003](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1003.md) | The TestCaseAttribute provided too few arguments | :white_check_mark: | :exclamation: | :x: |
| [NUnit1004](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1004.md) | The TestCaseAttribute provided too many arguments | :white_check_mark: | :exclamation: | :x: |
| [NUnit1005](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1005.md) | The type of the value specified via ExpectedResult must match the return type of the method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1006](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1006.md) | ExpectedResult must not be specified when the method returns void | :white_check_mark: | :exclamation: | :x: |
| [NUnit1007](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1007.md) | The method has non-void return type, but no result is expected in ExpectedResult | :white_check_mark: | :exclamation: | :x: |
| [NUnit1008](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1008.md) | Specifying ParallelScope.Self on assembly level has no effect | :white_check_mark: | :warning: | :x: |
| [NUnit1009](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1009.md) | One may not specify ParallelScope.Children on a non-parameterized test method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1010](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1010.md) | One may not specify ParallelScope.Fixtures on a test method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1011](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1011.md) | The TestCaseSource argument does not specify an existing member | :white_check_mark: | :exclamation: | :x: |
| [NUnit1012](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1012.md) | The async test method must have a non-void return type | :white_check_mark: | :exclamation: | :x: |
| [NUnit1013](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1013.md) | The async test method must have a non-generic Task return type when no result is expected | :white_check_mark: | :exclamation: | :x: |
| [NUnit1014](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1014.md) | The async test method must have a Task\<T> return type when a result is expected | :white_check_mark: | :exclamation: | :x: |
| [NUnit1015](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1015.md) | The source type does not implement I(Async)Enumerable | :white_check_mark: | :exclamation: | :x: |
| [NUnit1016](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1016.md) | The source type does not have a default constructor | :white_check_mark: | :exclamation: | :x: |
| [NUnit1017](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1017.md) | The specified source is not static | :white_check_mark: | :exclamation: | :x: |
| [NUnit1018](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1018.md) | The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1019](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1019.md) | The source specified by the TestCaseSource does not return an I(Async)Enumerable or a type that implements I(Async)Enumerable | :white_check_mark: | :exclamation: | :x: |
| [NUnit1020](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1020.md) | The TestCaseSource provides parameters to a source - field or property - that expects no parameters | :white_check_mark: | :exclamation: | :x: |
| [NUnit1021](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1021.md) | The ValueSource should use nameof operator to specify target | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit1022](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1022.md) | The specified source is not static | :white_check_mark: | :exclamation: | :x: |
| [NUnit1023](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1023.md) | The target method expects parameters which cannot be supplied by the ValueSource | :white_check_mark: | :exclamation: | :x: |
| [NUnit1024](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1024.md) | The source specified by the ValueSource does not return an I(Async)Enumerable or a type that implements I(Async)Enumerable | :white_check_mark: | :exclamation: | :x: |
| [NUnit1025](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1025.md) | The ValueSource argument does not specify an existing member | :white_check_mark: | :exclamation: | :x: |
| [NUnit1026](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1026.md) | The test or setup/teardown method is not public | :white_check_mark: | :exclamation: | :white_check_mark: |
| [NUnit1027](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1027.md) | The test method has parameters, but no arguments are supplied by attributes | :white_check_mark: | :exclamation: | :x: |
| [NUnit1028](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1028.md) | The non-test method is public | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit1029](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1029.md) | The number of parameters provided by the TestCaseSource does not match the number of parameters in the Test method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1030](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1030.md) | The type of parameter provided by the TestCaseSource does not match the type of the parameter in the Test method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1031](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1031.md) | The individual arguments provided by a ValuesAttribute must match the type of the corresponding parameter of the method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1032](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1032.md) | An IDisposable field/property should be Disposed in a TearDown method | :white_check_mark: | :exclamation: | :x: |
| [NUnit1033](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1033.md) | The Write methods on TestContext will be marked as Obsolete and eventually removed | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit1034](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1034.md) | Base TestFixtures should be abstract | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit1035](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1035.md) | The 'step' parameter to Range cannot be zero | :white_check_mark: | :exclamation: | :x: |
| [NUnit1036](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1036.md) | The value for 'from' must be less than 'to' when 'step' is positive | :white_check_mark: | :exclamation: | :x: |
| [NUnit1037](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1037.md) | The value for 'from' must be greater than 'to' when 'step' is negative | :white_check_mark: | :exclamation: | :x: |
| [NUnit1038](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit1038.md) | The type of the attribute values doesn't match the parameter type | :white_check_mark: | :exclamation: | :x: |

## Assertion Rules (NUnit2001 - )

Rules which improve assertions in the test code.

| Id       | Title       | :mag: | :memo: | :bulb: |
| :--      | :--         | :--:  | :--:   | :--:   |
| [NUnit2001](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2001.md) | Consider using Assert.That(expr, Is.False) instead of ClassicAssert.False(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2002](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2002.md) | Consider using Assert.That(expr, Is.False) instead of ClassicAssert.IsFalse(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2003](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2003.md) | Consider using Assert.That(expr, Is.True) instead of ClassicAssert.IsTrue(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2004](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2004.md) | Consider using Assert.That(expr, Is.True) instead of ClassicAssert.True(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2005](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2005.md) | Consider using Assert.That(actual, Is.EqualTo(expected)) instead of ClassicAssert.AreEqual(expected, actual) | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2006](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2006.md) | Consider using Assert.That(actual, Is.Not.EqualTo(expected)) instead of ClassicAssert.AreNotEqual(expected, actual) | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2007](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2007.md) | The actual value should not be a constant | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2008](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2008.md) | Incorrect IgnoreCase usage | :white_check_mark: | :warning: | :x: |
| [NUnit2009](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2009.md) | The same value has been provided as both the actual and the expected argument | :white_check_mark: | :warning: | :x: |
| [NUnit2010](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2010.md) | Use EqualConstraint for better assertion messages in case of failure | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2011](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2011.md) | Use ContainsConstraint for better assertion messages in case of failure | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2012](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2012.md) | Use StartsWithConstraint for better assertion messages in case of failure | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2013](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2013.md) | Use EndsWithConstraint for better assertion messages in case of failure | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2014](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2014.md) | Use SomeItemsConstraint for better assertion messages in case of failure | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2015](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2015.md) | Consider using Assert.That(actual, Is.SameAs(expected)) instead of ClassicAssert.AreSame(expected, actual) | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2016](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2016.md) | Consider using Assert.That(expr, Is.Null) instead of ClassicAssert.Null(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2017](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2017.md) | Consider using Assert.That(expr, Is.Null) instead of ClassicAssert.IsNull(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2018](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2018.md) | Consider using Assert.That(expr, Is.Not.Null) instead of ClassicAssert.NotNull(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2019](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2019.md) | Consider using Assert.That(expr, Is.Not.Null) instead of ClassicAssert.IsNotNull(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2020](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2020.md) | Incompatible types for SameAs constraint | :white_check_mark: | :exclamation: | :x: |
| [NUnit2021](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2021.md) | Incompatible types for EqualTo constraint | :white_check_mark: | :exclamation: | :x: |
| [NUnit2022](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2022.md) | Missing property required for constraint | :white_check_mark: | :exclamation: | :white_check_mark: |
| [NUnit2023](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2023.md) | Invalid NullConstraint usage | :white_check_mark: | :exclamation: | :x: |
| [NUnit2024](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2024.md) | Wrong actual type used with String Constraint | :white_check_mark: | :exclamation: | :x: |
| [NUnit2025](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2025.md) | Wrong actual type used with ContainsConstraint | :x: | :thought_balloon: | :x: |
| [NUnit2026](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2026.md) | Wrong actual type used with the SomeItemsConstraint with EqualConstraint | :white_check_mark: | :exclamation: | :x: |
| [NUnit2027](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2027.md) | Consider using Assert.That(actual, Is.GreaterThan(expected)) instead of ClassicAssert.Greater(actual, expected) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2028](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2028.md) | Consider using Assert.That(actual, Is.GreaterThanOrEqualTo(expected)) instead of ClassicAssert.GreaterOrEqual(actual, expected) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2029](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2029.md) | Consider using Assert.That(actual, Is.LessThan(expected)) instead of ClassicAssert.Less(actual, expected) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2030](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2030.md) | Consider using Assert.That(actual, Is.LessThanOrEqualTo(expected)) instead of ClassicAssert.LessOrEqual(actual, expected) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2031](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2031.md) | Consider using Assert.That(actual, Is.Not.SameAs(expected)) instead of ClassicAssert.AreNotSame(expected, actual) | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2032](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2032.md) | Consider using Assert.That(expr, Is.Zero) instead of ClassicAssert.Zero(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2033](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2033.md) | Consider using Assert.That(expr, Is.Not.Zero) instead of ClassicAssert.NotZero(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2034](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2034.md) | Consider using Assert.That(expr, Is.NaN) instead of ClassicAssert.IsNaN(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2035](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2035.md) | Consider using Assert.That(collection, Is.Empty) instead of ClassicAssert.IsEmpty(collection) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2036](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2036.md) | Consider using Assert.That(collection, Is.Not.Empty) instead of ClassicAssert.IsNotEmpty(collection) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2037](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2037.md) | Consider using Assert.That(collection, Does.Contain(instance)) instead of ClassicAssert.Contains(instance, collection) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2038](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2038.md) | Consider using Assert.That(actual, Is.InstanceOf(expected)) instead of ClassicAssert.IsInstanceOf(expected, actual) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2039](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2039.md) | Consider using Assert.That(actual, Is.Not.InstanceOf(expected)) instead of ClassicAssert.IsNotInstanceOf(expected, actual) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2040](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2040.md) | Non-reference types for SameAs constraint | :white_check_mark: | :exclamation: | :white_check_mark: |
| [NUnit2041](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2041.md) | Incompatible types for comparison constraint | :white_check_mark: | :exclamation: | :x: |
| [NUnit2042](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2042.md) | Comparison constraint on object | :white_check_mark: | :information_source: | :x: |
| [NUnit2043](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2043.md) | Use ComparisonConstraint for better assertion messages in case of failure | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2044](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2044.md) | Non-delegate actual parameter | :white_check_mark: | :exclamation: | :white_check_mark: |
| [NUnit2045](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2045.md) | Use Assert.EnterMultipleScope or Assert.Multiple | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2046](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2046.md) | Use CollectionConstraint for better assertion messages in case of failure | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2047](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2047.md) | Incompatible types for Within constraint | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2048](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2048.md) | Consider using Assert.That(...) instead of StringAssert(...) | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2049](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2049.md) | Consider using Assert.That(...) instead of CollectionAssert(...) | :white_check_mark: | :warning: | :white_check_mark: |
| [NUnit2050](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2050.md) | NUnit 4 no longer supports string.Format specification | :white_check_mark: | :exclamation: | :white_check_mark: |
| [NUnit2051](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2051.md) | Consider using Assert.That(expr, Is.Positive) instead of ClassicAssert.Positive(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2052](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2052.md) | Consider using Assert.That(expr, Is.Negative) instead of ClassicAssert.Negative(expr) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2053](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2053.md) | Consider using Assert.That(actual, Is.AssignableFrom(expected)) instead of ClassicAssert.IsAssignableFrom(expected, actual) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2054](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2054.md) | Consider using Assert.That(actual, Is.Not.AssignableFrom(expected)) instead of ClassicAssert.IsNotAssignableFrom(expected, actual) | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2055](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2055.md) | Consider using Is.InstanceOf\<T> constraint instead of an 'is T' expression | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit2056](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit2056.md) | Consider using Assert.EnterMultipleScope statement instead of Assert.Multiple/Assert.MultipleAsync | :white_check_mark: | :information_source: | :white_check_mark: |

## Suppressor Rules (NUnit3001 - )

Rules which suppress compiler errors based on context. Note that these rules are only available in the .NET Standard 2.0
builds (version 3.0.0 and above) which require Visual Studio 2019 (version 16.3) or newer.

| Id       | Title       | :mag: | :memo: | :bulb: |
| :--      | :--         | :--:  | :--:   | :--:   |
| [NUnit3001](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit3001.md) | Expression was checked in an ClassicAssert.NotNull, ClassicAssert.IsNotNull or Assert.That call | :white_check_mark: | :information_source: | :x: |
| [NUnit3002](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit3002.md) | Field/Property is initialized in SetUp or OneTimeSetUp method | :white_check_mark: | :information_source: | :x: |
| [NUnit3003](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit3003.md) | Class is an NUnit TestFixture and is instantiated using reflection | :white_check_mark: | :information_source: | :x: |
| [NUnit3004](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit3004.md) | Field should be Disposed in TearDown or OneTimeTearDown method | :white_check_mark: | :information_source: | :x: |

## Style Rules (NUnit4001 - )

Rules which help you write concise and readable NUnit test code.

| Id       | Title       | :mag: | :memo: | :bulb: |
| :--      | :--         | :--:  | :--:   | :--:   |
| [NUnit4001](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit4001.md) | Simplify the Values attribute | :white_check_mark: | :information_source: | :white_check_mark: |
| [NUnit4002](https://github.com/nunit/nunit.analyzers/tree/master/documentation/NUnit4002.md) | Use Specific constraint | :white_check_mark: | :information_source: | :white_check_mark: |
