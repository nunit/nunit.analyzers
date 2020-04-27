# NUnit1018
## The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method.

| Topic    | Value
| :--      | :--
| Id       | NUnit1018
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestCaseSourceUsesStringAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestCaseSourceUsage/TestCaseSourceUsesStringAnalyzer.cs)


## Description

The number of parameters provided by the TestCaseSource must match the number of parameters in the target method.

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
#pragma warning disable NUnit1018 // The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method.
Code violating the rule here
#pragma warning restore NUnit1018 // The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1018 // The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1018:The number of parameters provided by the TestCaseSource does not match the number of parameters in the target method.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
