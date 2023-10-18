# NUnit1033

## NUnit 4 no longer supports string.Format specification

| Topic    | Value
| :--      | :--
| Id       | NUnit1033
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [UpdateStringFormatToInterpolatableStringAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/UpdateStringFormatToInterpolatableString/UpdateStringFormatToInterpolatableStringAnalyzer.cs)

## Description

Replace format specification with interpolated string.

## Motivation

In order to get better failure messages, NUnit4 uses [`CallerArgumentExpression`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute?view=net-7.0)
to include the expression passed in for the _actual_ and _constraint_ parameters.
These are parameters automatically supplied by the compiler.

To facilitate this, we needed to drop support for [composite formatting](https://learn.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting)
All NUnit4 asserts only allow a single *message* parameter which can be either a simple string literal
or a [interpolatable string](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/string-interpolation)

This analyzer needs to be run when still building against NUnit3 as otherwise your code won't compile.
When usages of the new methods with `params` are detected, the associated CodeFix will convert the format specification
into an interpolated string.

## How to fix violations

The following code, valid in NUNit3:

```csharp
[TestCase(4)]
public void MustBeMultipleOf3(int value)
{
    Assert.That(value % 3, Is.Zero, "Expected value ({0}) to be multiple of 3", value);
}
```

Will fail with the following message:

```
Expected value (4) to be multiple of 3
Expected: 0
But was:  1
```

The associated CodeFix for this Analyzer rule will convert the test into:

```csharp
[TestCase(4)]
public void MustBeMultipleOf3(int value)
{
    Assert.That(value % 3, Is.Zero, $"Expected value ({value}) to be multiple of 3");
}
```

The failure message for NUnit4 becomes:

```
Expected value (4) to be multiple of 3
Assert.That(value % 3, Is.Zero)
Expected: 0
But was:  1
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see [MSDN](https://learn.microsoft.com/en-us/visualstudio/code-quality/using-rule-sets-to-group-code-analysis-rules?view=vs-2022).

### Via .editorconfig file

```ini
# NUnit1033: NUnit 4 no longer supports string.Format specification
dotnet_diagnostic.NUnit1033.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable NUnit1033 // NUnit 4 no longer supports string.Format specification
Code violating the rule here
#pragma warning restore NUnit1033 // NUnit 4 no longer supports string.Format specification
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable NUnit1033 // NUnit 4 no longer supports string.Format specification
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure",
    "NUnit1033:NUnit 4 no longer supports string.Format specification",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
