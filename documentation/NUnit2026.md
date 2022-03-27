# NUnit2026

## Wrong actual type used with the SomeItemsConstraint with EqualConstraint

| Topic    | Value
| :--      | :--
| Id       | NUnit2026
| Severity | Error
| Enabled  | True
| Category | Assertion
| Code     | [SomeItemsIncompatibleTypesAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers.common/SomeItemsIncompatibleTypes/SomeItemsIncompatibleTypesAnalyzer.cs)

## Description

The SomeItemsConstraint with EqualConstraint requires the actual argument to be a collection where the element type can match the type of the expected argument.

## Motivation

Using Does.Contain or Contains.Item constraints with actual argument, which is either not a collection, or has wrong element type, leads to assertion error.

## How to fix violations

Fix the actual value or use appropriate constraint.

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via .editorconfig file

```ini
# NUnit2026: Wrong actual type used with the SomeItemsConstraint with EqualConstraint
dotnet_diagnostic.NUnit2026.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable NUnit2026 // Wrong actual type used with the SomeItemsConstraint with EqualConstraint
Code violating the rule here
#pragma warning restore NUnit2026 // Wrong actual type used with the SomeItemsConstraint with EqualConstraint
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable NUnit2026 // Wrong actual type used with the SomeItemsConstraint with EqualConstraint
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion",
    "NUnit2026:Wrong actual type used with the SomeItemsConstraint with EqualConstraint",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
