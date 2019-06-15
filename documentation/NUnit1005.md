# NUnit1005
## The type of ExpectedResult must match the return type.

| Topic    | Value
| :--      | :--
| Id       | NUnit1005
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestMethodUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestMethodUsage/TestMethodUsageAnalyzer.cs)


## Description

The type of ExpectedResult must match the return type. This will lead to an error at run-time.

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
#pragma warning disable NUnit1005 // The type of ExpectedResult must match the return type.
Code violating the rule here
#pragma warning restore NUnit1005 // The type of ExpectedResult must match the return type.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1005 // The type of ExpectedResult must match the return type.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1005:The type of ExpectedResult must match the return type.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
