# NUnit1009
## No ParallelScope.Children on a non-parameterized test method.

| Topic    | Value
| :--      | :--
| Id       | NUnit1009
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [ParallelizableUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ParallelizableUsage/ParallelizableUsageAnalyzer.cs)


## Description

One may not specify ParallelScope.Children on a non-parameterized test method.

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
#pragma warning disable NUnit1009 // No ParallelScope.Children on a non-parameterized test method.
Code violating the rule here
#pragma warning restore NUnit1009 // No ParallelScope.Children on a non-parameterized test method.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1009 // No ParallelScope.Children on a non-parameterized test method.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1009:No ParallelScope.Children on a non-parameterized test method.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
