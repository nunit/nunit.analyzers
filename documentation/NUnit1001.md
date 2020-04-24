# NUnit1001
## The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.

| Topic    | Value
| :--      | :--
| Id       | NUnit1001
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestCaseUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestCaseUsage/TestCaseUsageAnalyzer.cs)


## Description

The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.

## Motivation

ADD MOTIVATION HERE

## How to fix violations

### Example Violation

```csharp
[TestCase(true)]
public void SampleTest(int numberValue)
{
    Assert.That(numberValue, Is.EqualTo(1));
}
```

### Problem

In the test case above, `1` in the test case indicates that `numberValue` should be an int. However, it's sent in as an `bool`.

### Fix

Ensure that the type of the test case and the input matches.

So, this fix would be acceptable:

```csharp
// TestCase input and parameter are both of type bool
[TestCase(true)]
public void SampleTest(bool numberValue)
{
    Assert.That(numberValue, Is.True);
}
```

And this would also work:

```csharp
// TestCase input and parameter are both of type int
[TestCase(1)]
public void SampleTest(int numberValue)
{
    Assert.That(numberValue, Is.EqualTo(1));
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable NUnit1001 // The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.
Code violating the rule here
#pragma warning restore NUnit1001 // The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1001 // The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1001:The individual arguments provided by a TestCaseAttribute must match the type of the matching parameter of the method.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
