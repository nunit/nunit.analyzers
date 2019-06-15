# NUnit2006
## Consider using Assert.That(expr1, Is.Not.EqualTo(expr2)) instead of Assert.AreNotEqual(expr1, expr2).

| Topic    | Value
| :--      | :--
| Id       | NUnit2006
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [ClassicModelAssertUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ClassicModelAssertUsage/ClassicModelAssertUsageAnalyzer.cs)


## Description

Consider using the constraint model, Assert.That(expr1, Is.Not.EqualTo(expr2)), instead of the classic model, Assert.AreNotEqual(expr1, expr2).

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
#pragma warning disable NUnit2006 // Consider using Assert.That(expr1, Is.Not.EqualTo(expr2)) instead of Assert.AreNotEqual(expr1, expr2).
Code violating the rule here
#pragma warning restore NUnit2006 // Consider using Assert.That(expr1, Is.Not.EqualTo(expr2)) instead of Assert.AreNotEqual(expr1, expr2).
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2006 // Consider using Assert.That(expr1, Is.Not.EqualTo(expr2)) instead of Assert.AreNotEqual(expr1, expr2).
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2006:Consider using Assert.That(expr1, Is.Not.EqualTo(expr2)) instead of Assert.AreNotEqual(expr1, expr2).",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
