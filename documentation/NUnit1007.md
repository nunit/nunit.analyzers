# NUnit1007
## Method has non-void return type, but no result is expected in ExpectedResult.

| Topic    | Value
| :--      | :--
| Id       | NUnit1007
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestMethodUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestMethodUsage/TestMethodUsageAnalyzer.cs)


## Description

Method has non-void return type, but no result is expected in ExpectedResult.

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
#pragma warning disable NUnit1007 // Method has non-void return type, but no result is expected in ExpectedResult.
Code violating the rule here
#pragma warning restore NUnit1007 // Method has non-void return type, but no result is expected in ExpectedResult.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1007 // Method has non-void return type, but no result is expected in ExpectedResult.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1007:Method has non-void return type, but no result is expected in ExpectedResult.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
