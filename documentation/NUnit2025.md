# NUnit2025
## Wrong actual type used with ContainsConstraint.

| Topic    | Value
| :--      | :--
| Id       | NUnit2025
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [ContainsConstraintWrongActualTypeAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ContainsConstraintWrongActualType/ContainsConstraintWrongActualTypeAnalyzer.cs)


## Description

The ContainsConstraint requires actual value to be either a string or a collection of strings.

## Motivation

Using a ContainsConstraint with an actual argument, which is neither a string nor a collection of strings, leads to an assertion error.

## How to fix violations

Fix the actual value or use appropriate constraint.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable NUnit2025 // Wrong actual type used with ContainsConstraint.
Code violating the rule here
#pragma warning restore NUnit2025 // Wrong actual type used with ContainsConstraint.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2025 // Wrong actual type used with ContainsConstraint.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2025:Wrong actual type used with ContainsConstraint.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
