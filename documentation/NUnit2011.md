# NUnit2011
## Use Is.Not.EqualTo constraint

| Topic    | Value
| :--      | :--
| Id       | NUnit2011
| Severity | Info
| Enabled  | True
| Category | Assertion
| Code     | [EqualToConstraintUsageAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/ConstraintUsage/EqualToConstraintUsageAnalyzer.cs)


## Description

Using Is.Not.EqualTo constraint will lead to better assertion messages in case of failure

## Motivation

Using `Is.Not.EqualTo` constraint will lead to better assertion messages in case of failure, 
so this analyzer marks all usages of `!=` operator and negated `Equals` method where it is possible to replace 
with `Is.Not.EqualTo` constraint

```csharp
[Test]
public void Test()
{
    Assert.True(actual != expected);
}
```
## How to fix violations

The analyzer comes with a code fix that will replace `Assert.True(actual != expected)` with
`Assert.That(actual, Is.Not.EqualTo(expected))`. So the code block above will be changed into

```csharp
[Test]
public void Test()
{
    Assert.That(actual, Is.Not.EqualTo(expected));
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable NUnit2011 // Use Is.Not.EqualTo constraint
Code violating the rule here
#pragma warning restore NUnit2011 // Use Is.Not.EqualTo constraint
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable NUnit2011 // Use Is.Not.EqualTo constraint
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", 
    "NUnit2011:Use Is.Not.EqualTo constraint",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
