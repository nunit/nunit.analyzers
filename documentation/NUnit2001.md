# NUnit2001
## Consider using Assert.That(expr, Is.False) instead of Assert.False(expr).

| Topic    | Value
| :--      | :--
| Id       | NUnit2001
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [ClassicModelAssertUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ClassicModelAssertUsage/ClassicModelAssertUsageAnalyzer.cs)


## Description

Consider using the constraint model, Assert.That(expr, Is.False), instead of the classic model, Assert.False(expr).

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
#pragma warning disable NUnit2001 // Consider using Assert.That(expr, Is.False) instead of Assert.False(expr).
Code violating the rule here
#pragma warning restore NUnit2001 // Consider using Assert.That(expr, Is.False) instead of Assert.False(expr).
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2001 // Consider using Assert.That(expr, Is.False) instead of Assert.False(expr).
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2001:Consider using Assert.That(expr, Is.False) instead of Assert.False(expr).",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
