# NUnit2055

## Use Assert.ThatAsync

| Topic    | Value
| :--      | :--
| Id       | NUnit2055
| Severity | Info
| Enabled  | True
| Category | Assertion
| Code     | [UseAssertThatAsyncAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/UseAssertThatAsync/UseAssertThatAsyncAnalyzer.cs)

## Description

You can use `Assert.ThatAsync` to assert asynchronously.

## Motivation

`Assert.That` runs synchronously, even if pass an asynchronous delegate. This "sync-over-async" pattern blocks
the calling thread, preventing it from doing anything else in the meantime.

`Assert.ThatAsync` allows for a proper async/await. This allows for a better utilization of threads while waiting for the
asynchronous operation to finish.

## How to fix violations

Convert the asynchronous method call with a lambda expression and `await` the `Assert.ThatAsync` instead of the
asynchronous method call.

```csharp
Assert.That(await DoAsync(), Is.EqualTo(expected));             // bad (sync-over-async)
await Assert.ThatAsync(() => DoAsync(), Is.EqualTo(expected));  // good (proper async/await)
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see
[MSDN](https://learn.microsoft.com/en-us/visualstudio/code-quality/using-rule-sets-to-group-code-analysis-rules?view=vs-2022).

### Via .editorconfig file

```ini
# NUnit2055: Use Assert.ThatAsync
dotnet_diagnostic.NUnit2055.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable NUnit2055 // Use Assert.ThatAsync
Code violating the rule here
#pragma warning restore NUnit2055 // Use Assert.ThatAsync
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable NUnit2055 // Use Assert.ThatAsync
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion",
    "NUnit2055:Use Assert.ThatAsync",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
