# NUnit1004
## Too many arguments provided by TestCaseAttribute.

| Topic    | Value
| :--      | :--
| Id       | NUnit1004
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestCaseUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestCaseUsage/TestCaseUsageAnalyzer.cs)


## Description

The number of arguments provided by a TestCaseAttribute must match the number of parameters of the method.

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
#pragma warning disable NUnit1004 // Too many arguments provided by TestCaseAttribute.
Code violating the rule here
#pragma warning restore NUnit1004 // Too many arguments provided by TestCaseAttribute.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1004 // Too many arguments provided by TestCaseAttribute.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1004:Too many arguments provided by TestCaseAttribute.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
