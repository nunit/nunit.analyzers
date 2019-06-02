# NUnit2009
## Find same value provided as actual and expected argument

| Topic    | Value
| :--      | :--
| Id       | NUnit2009
| Severity | Warning
| Enabled  | True
| Category | Structure
| Code     | [SameActualExpectedValueAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/SameActualExpectedValue/SameActualExpectedValueAnalyzer.cs)


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
#pragma warning disable NUnit2009 // Find same value provided as actual and expected argument
Code violating the rule here
#pragma warning restore NUnit2009 // Find same value provided as actual and expected argument
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2009 // Find same value provided as actual and expected argument
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit2009:Find same value provided as actual and expected argument", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
