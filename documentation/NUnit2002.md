# NUnit2002
## Find Classic Assertion Usage

| Topic    | Value
| :--      | :--
| Id       | NUnit2002
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [ClassicModelAssertUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ClassicModelAssertUsage/ClassicModelAssertUsageAnalyzer.cs)


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
#pragma warning disable NUnit2002 // Find Classic Assertion Usage
Code violating the rule here
#pragma warning restore NUnit2002 // Find Classic Assertion Usage
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2002 // Find Classic Assertion Usage
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2002:Find Classic Assertion Usage", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
