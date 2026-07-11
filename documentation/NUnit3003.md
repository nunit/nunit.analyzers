# NUnit3003

## Class is an NUnit TestFixture or TestData source and is instantiated using reflection

| Topic    | Value
| :--      | :--
| Id       | NUnit3003
| Severity | Info
| Enabled  | True
| Category | Suppressor
| Code     | [AvoidUninstantiatedInternalClassSuppressor](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/DiagnosticSuppressors/AvoidUninstantiatedInternalClassSuppressor.cs)

## Description

Class is an NUnit TestFixture or TestData source and is instantiated using reflection

## Motivation

The default roslyn analyzer has rule
[CA1812](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1812)
which warns about internal classes not being used.
That analyzer doesn't know about NUnit test classes.
This suppressor catches the error,
verifies the class is an NUnit TestFixture or TestData source and if so suppresses the error.

NUnit test fixtures can be marked internal but this will cause the CA1812 warning to be raised.
This suppressor will suppress the warning for internal test fixtures.

```csharp
[TestFixture]
internal class MyTestFixture
{
    [Test]
    public void TestMethod()
    {
        Assert.Pass();
    }
}
```

Test data source classes can be marked internal but this will cause the CA1812 warning to be raised.
This suppressor will suppress the warning for internal test data sources that are actually used by test fixtures.

Given the following test data source class:

```csharp
using System.Collections;

internal sealed class TestData : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return "Hello";
        yield return "World";
    }
}
```

Where this class is used to supply arguments to a test method like this:

```csharp
[TestFixture]
internal sealed class TestFixture
{
    [TestCaseSource(typeof(TestData))]
    public void ParameterizedTestMethod(string value)
    {
        Assert.That(value, Is.Not.Null);
        Assert.That(value, Has.Length.EqualTo(5));
    }
}
```

or where it is used to supply arguments to a parameterized test fixture like this:

```csharp
[TestFixtureSource(typeof(TestData))]
public sealed class ParameterizedTestFixture(string value)
{
    [Test]
    public void MustBeNotNull()
    {
        Assert.That(value, Is.Not.Null);
    }

    [Test]
    public void MustBeFiveCharactersLong()
    {
        Assert.That(value, Has.Length.EqualTo(5));
    }
}
```

<!-- start generated config severity -->
## Configure severity

The rule has no severity, but can be disabled.

### Via ruleset file

To disable the rule for a project, you need to add a
[ruleset file](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/DiagnosticSuppressors/NUnit.Analyzers.Suppressions.ruleset)

```xml
<?xml version="1.0" encoding="utf-8"?>
<RuleSet Name="NUnit.Analyzer Suppressions" Description="DiagnosticSuppression Rules" ToolsVersion="12.0">
  <Rules AnalyzerId="DiagnosticSuppressors" RuleNamespace="NUnit.NUnitAnalyzers">
    <Rule Id="NUnit3001" Action="Info" /> <!-- Possible Null Reference -->
    <Rule Id="NUnit3002" Action="Info" /> <!-- NonNullableField/Property is Uninitialized -->
    <Rule Id="NUnit3003" Action="Info" /> <!-- Avoid Uninstantiated Internal Classes -->
    <Rule Id="NUnit3004" Action="Info" /> <!-- Types that own disposable fields should be disposable -->
  </Rules>
</RuleSet>
```

and add it to the project like:

```xml
<PropertyGroup>
  <CodeAnalysisRuleSet>NUnit.Analyzers.Suppressions.ruleset</CodeAnalysisRuleSet>
</PropertyGroup>
```

For more info about rulesets see [MSDN](https://learn.microsoft.com/en-us/visualstudio/code-quality/using-rule-sets-to-group-code-analysis-rules?view=vs-2022).

### Via .editorconfig file

This is currently not working. Waiting for [Roslyn](https://github.com/dotnet/roslyn/issues/49727)

```ini
# NUnit3003: Class is an NUnit TestFixture or TestData source and is instantiated using reflection
dotnet_diagnostic.NUnit3003.severity = none
```
<!-- end generated config severity -->
