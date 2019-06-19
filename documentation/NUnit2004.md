# NUnit2004
## Consider using Assert.That(expr, Is.True) instead of Assert.True(expr).

| Topic    | Value
| :--      | :--
| Id       | NUnit2004
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [ClassicModelAssertUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ClassicModelAssertUsage/ClassicModelAssertUsageAnalyzer.cs)


## Description

Consider using the constraint model, Assert.That(expr, Is.True), instead of the classic model, Assert.True(expr).

## Motivation

ADD MOTIVATION HERE

## How to fix violations

ADD HOW TO FIX VIOLATIONS HERE

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable NUnit2004 // Consider using Assert.That(expr, Is.True) instead of Assert.True(expr).
Code violating the rule here
#pragma warning restore NUnit2004 // Consider using Assert.That(expr, Is.True) instead of Assert.True(expr).
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2004 // Consider using Assert.That(expr, Is.True) instead of Assert.True(expr).
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2004:Consider using Assert.That(expr, Is.True) instead of Assert.True(expr).",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
