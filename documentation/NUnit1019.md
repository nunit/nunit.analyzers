# NUnit1019
## The source specified by the TestCaseSource does not return an IEnumerable or a type that implements IEnumerable.

| Topic    | Value
| :--      | :--
| Id       | NUnit1019
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestCaseSourceUsesStringAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestCaseSourceUsage/TestCaseSourceUsesStringAnalyzer.cs)


## Description

The source specified by the TestCaseSource must return an IEnumerable or a type that implements IEnumerable.

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
#pragma warning disable NUnit1019 // The source specified by the TestCaseSource does not return an IEnumerable or a type that implements IEnumerable.
Code violating the rule here
#pragma warning restore NUnit1019 // The source specified by the TestCaseSource does not return an IEnumerable or a type that implements IEnumerable.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1019 // The source specified by the TestCaseSource does not return an IEnumerable or a type that implements IEnumerable.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1019:The source specified by the TestCaseSource does not return an IEnumerable or a type that implements IEnumerable.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
