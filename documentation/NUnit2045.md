# NUnit2045

## Incompatible types for Within constraint

| Topic    | Value
| :--      | :--
| Id       | NUnit2045
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [WithinUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/WithinUsage/WithinUsageAnalyzer.cs)

## Description

The Within modifier should only be used for numeric or Date/Time arguments or tuples containing only these element types. Using it on another type will not have any effect.

## Motivation

To bring developers' attention to a scenario in which their code is actually having no effect and may reveal that their test is not doing what they expect.

## How to fix violations

### Example Violation

```csharp
[Test]
public void RecordsEqualsMismatch()
{
    var a = new Data(1, 1.0);
    var b = new Data(1, 1.1);

    Assert.That(a, Is.EqualTo(b).Within(0.2), $"{a} != {b}");
}

private sealed record Data(int number, double Value);
```

### Explanation

Using Within here doesn't make any sense, because nunit cannot apply comparison with tolerance to the types we're comparing.

### Fix

Remove the errant call to `Within`:

```csharp
[Test]
public void RecordsEqualsMismatch()
{
    var a = new Data(1, 1.0);
    var b = new Data(1, 1.1);

    Assert.That(a, Is.EqualTo(b), $"{a} != {b}");
}

private sealed record Data(int number, double Value);
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via .editorconfig file

```ini
# NUnit2045: Incompatible types for Within constraint
dotnet_diagnostic.NUnit2045.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable NUnit2045 // Incompatible types for Within constraint
Code violating the rule here
#pragma warning restore NUnit2045 // Incompatible types for Within constraint
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable NUnit2045 // Incompatible types for Within constraint
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion",
    "NUnit2045:Incompatible types for Within constraint",
    Justification = "Reason...")]
```
<!-- end generated config severity -->