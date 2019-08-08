# NUnit2010
## Use Is.EqualTo constraint.

| Topic    | Value
| :--      | :--
| Id       | NUnit2010
| Severity | Info
| Enabled  | True
| Category | Assertion
| Code     | [EqualToConstraintUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ConstraintUsage/EqualToConstraintUsageAnalyzer.cs)


## Description

Using `Is.EqualTo` constraint will lead to better assertion messages in case of failure.

## Motivation

Using `Is.EqualTo` constraint will lead to better assertion messages in case of failure, 
so this analyzer marks all usages of `==` operator and `Equals` method where it is possible to replace 
with `Is.EqualTo` constraint.

```csharp
[Test]
public void Test()
{
    Assert.True(actual == expected);
}
```
## How to fix violations

The analyzer comes with a code fix that will replace `Assert.True(actual == expected)` with
`Assert.That(actual, Is.EqualTo(expected))`. So the code block above will be changed into

```csharp
[Test]
public void Test()
{
    Assert.That(actual, Is.EqualTo(expected));
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable NUnit2010 // Use Is.EqualTo constraint.
Code violating the rule here
#pragma warning restore NUnit2010 // Use Is.EqualTo constraint.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2010 // Use Is.EqualTo constraint.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2010:Use Is.EqualTo constraint.",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
