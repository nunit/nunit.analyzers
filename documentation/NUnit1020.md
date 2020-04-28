# NUnit1020
## The TestCaseSource provides parameters to source - field or property - that expects no parameters.

| Topic    | Value
| :--      | :--
| Id       | NUnit1020
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestCaseSourceUsesStringAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestCaseSourceUsage/TestCaseSourceUsesStringAnalyzer.cs)


## Description

The TestCaseSource must not provide any parameters when the source is a field or property.

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
#pragma warning disable NUnit1020 // The TestCaseSource provides parameters to source - field or property - that expects no parameters.
Code violating the rule here
#pragma warning restore NUnit1020 // The TestCaseSource provides parameters to source - field or property - that expects no parameters.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1020 // The TestCaseSource provides parameters to source - field or property - that expects no parameters.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1020:The TestCaseSource provides parameters to source - field or property - that expects no parameters.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
