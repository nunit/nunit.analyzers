# NUnit1013
## Find Incorrect TestAttribute or TestCaseAttribute Usage

| Topic    | Value
| :--      | :--
| Id       | NUnit1013
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestMethodUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestMethodUsage/TestMethodUsageAnalyzer.cs)


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
#pragma warning disable NUnit1013 // Find Incorrect TestAttribute or TestCaseAttribute Usage
Code violating the rule here
#pragma warning restore NUnit1013 // Find Incorrect TestAttribute or TestCaseAttribute Usage
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1013 // Find Incorrect TestAttribute or TestCaseAttribute Usage
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1013:Find Incorrect TestAttribute or TestCaseAttribute Usage", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->
