# NUnit2024
## Wrong actual type used with String Constraint.

| Topic    | Value
| :--      | :--
| Id       | NUnit2024
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [StringConstraintWrongActualTypeAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/StringConstraintWrongActualType/StringConstraintWrongActualTypeAnalyzer.cs)


## Description

Provided actual argument value is not string type and cannot be used with String Constraint.

## Motivation

Assertions with string constraints and non-string actual value will fail with error.

## How to fix violations

Fix actual value or use appropriate constraint.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable NUnit2024 // Wrong actual type used with String Constraint.
Code violating the rule here
#pragma warning restore NUnit2024 // Wrong actual type used with String Constraint.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2024 // Wrong actual type used with String Constraint.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2024:Wrong actual type used with String Constraint.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
