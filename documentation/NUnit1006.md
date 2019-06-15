# NUnit1006
## ExpectedResult must not be specified when the method returns void.

| Topic    | Value
| :--      | :--
| Id       | NUnit1006
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestMethodUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestMethodUsage/TestMethodUsageAnalyzer.cs)


## Description

ExpectedResult must not be specified when the method returns void. This will lead to an error at run-time.

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
#pragma warning disable NUnit1006 // ExpectedResult must not be specified when the method returns void.
Code violating the rule here
#pragma warning restore NUnit1006 // ExpectedResult must not be specified when the method returns void.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1006 // ExpectedResult must not be specified when the method returns void.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1006:ExpectedResult must not be specified when the method returns void.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
