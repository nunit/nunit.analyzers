# NUnit2008
## Incorrect IgnoreCase usage.

| Topic    | Value
| :--      | :--
| Id       | NUnit2008
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [IgnoreCaseUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/IgnoreCaseUsage/IgnoreCaseUsageAnalyzer.cs)


## Description

IgnoreCase modifier should only be used for string or char arguments. Using it on another type will not have any effect.

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
#pragma warning disable NUnit2008 // Incorrect IgnoreCase usage.
Code violating the rule here
#pragma warning restore NUnit2008 // Incorrect IgnoreCase usage.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2008 // Incorrect IgnoreCase usage.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2008:Incorrect IgnoreCase usage.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
