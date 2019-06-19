# NUnit2007
## Actual value should not be constant.

| Topic    | Value
| :--      | :--
| Id       | NUnit2007
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [ConstActualValueUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ConstActualValueUsage/ConstActualValueUsageAnalyzer.cs)


## Description

Actual value should not be constant. This indicates that the actual and expected values have switched places.

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
#pragma warning disable NUnit2007 // Actual value should not be constant.
Code violating the rule here
#pragma warning restore NUnit2007 // Actual value should not be constant.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2007 // Actual value should not be constant.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2007:Actual value should not be constant.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
