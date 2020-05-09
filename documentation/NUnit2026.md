# NUnit2026
## Wrong actual type used with SomeItemsConstraint.

| Topic    | Value
| :--      | :--
| Id       | NUnit2026
| Severity | Warning
| Enabled  | True
| Category | Assertion
| Code     | [SomeItemsIncompatibleTypesAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/SomeItemsIncompatibleTypes/SomeItemsIncompatibleTypesAnalyzer.cs)


## Description

The SomeItemsConstraint requires actual argument to be a collection with matching element type to the expected argument.

## Motivation

Using SomeItemsConstraint with actual argument, which is either not a collection, or has wrong element type, leads to assertion error.

## How to fix violations

Fix the actual value or use appropriate constraint.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable NUnit2026 // Wrong actual type used with SomeItemsConstraint.
Code violating the rule here
#pragma warning restore NUnit2026 // Wrong actual type used with SomeItemsConstraint.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2026 // Wrong actual type used with SomeItemsConstraint.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2026:Wrong actual type used with SomeItemsConstraint.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
