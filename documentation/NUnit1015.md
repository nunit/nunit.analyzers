# NUnit1015
## Source type does not implement IEnumerable.

| Topic    | Value
| :--      | :--
| Id       | NUnit1015
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestCaseSourceUsesStringAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestCaseSourceUsage/TestCaseSourceUsesStringAnalyzer.cs)


## Description

The source type must implement IEnumerable in order to provide test cases.

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
#pragma warning disable NUnit1015 // Source type does not implement IEnumerable.
Code violating the rule here
#pragma warning restore NUnit1015 // Source type does not implement IEnumerable.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit1015 // Source type does not implement IEnumerable.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", 
    "NUnit1015:Source type does not implement IEnumerable.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
