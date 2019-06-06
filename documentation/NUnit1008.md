# NUnit1008
## Find Incorrect ParallelizableAttribute Usage

| Topic    | Value
| :--      | :--
| Id       | NUnit1008
| Severity | Warning
| Enabled  | True
| Category | Structure
| Code     | [ParallelizableUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ParallelizableUsage/ParallelizableUsageAnalyzer.cs)


## Description



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
#pragma warning disable NUnit1008 // Find Incorrect ParallelizableAttribute Usage
Code violating the rule here
#pragma warning restore NUnit1008 // Find Incorrect ParallelizableAttribute Usage
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1008 // Find Incorrect ParallelizableAttribute Usage
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1008:Find Incorrect ParallelizableAttribute Usage", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
