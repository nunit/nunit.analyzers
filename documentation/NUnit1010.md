# NUnit1010
## No ParallelScope.Fixtures on a test method.

| Topic    | Value
| :--      | :--
| Id       | NUnit1010
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [ParallelizableUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ParallelizableUsage/ParallelizableUsageAnalyzer.cs)


## Description

One may not specify ParallelScope.Fixtures on a test method.

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
#pragma warning disable NUnit1010 // No ParallelScope.Fixtures on a test method.
Code violating the rule here
#pragma warning restore NUnit1010 // No ParallelScope.Fixtures on a test method.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1010 // No ParallelScope.Fixtures on a test method.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1010:No ParallelScope.Fixtures on a test method.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
