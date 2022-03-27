# NUnit2016

## Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr)

| Topic    | Value
| :--      | :--
| Id       | NUnit2016
| Severity | Info
| Enabled  | True
| Category | Assertion
| Code     | [ClassicModelAssertUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers.common/ClassicModelAssertUsage/ClassicModelAssertUsageAnalyzer.cs)

## Description

Consider using the constraint model, `Assert.That(expr, Is.Null)`, instead of the classic model, `Assert.Null(expr)`.

## Motivation

The classic Assert model contains less flexibility than the constraint model,
so this analyzer marks usages of `Assert.Null` from the classic Assert model.

```csharp
[Test]
public void Test()
{
    object obj = null;
    Assert.Null(obj);
}
```

## How to fix violations

The analyzer comes with a code fix that will replace `Assert.Null(expression)` with
`Assert.That(expression, Is.Null)`. So the code block above will be changed into.

```csharp
[Test]
public void Test()
{
    object obj = null;
    Assert.That(obj, Is.Null);
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via .editorconfig file

```ini
# NUnit2016: Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr)
dotnet_diagnostic.NUnit2016.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable NUnit2016 // Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr)
Code violating the rule here
#pragma warning restore NUnit2016 // Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr)
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable NUnit2016 // Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr)
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion",
    "NUnit2016:Consider using Assert.That(expr, Is.Null) instead of Assert.Null(expr)",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
