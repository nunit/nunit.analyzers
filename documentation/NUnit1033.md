# NUnit1033

## The Write methods on TestContext are Obsolete

| Topic    | Value
| :--      | :--
| Id       | NUnit1033
| Severity | Error
| Enabled  | True
| Category | Structure
| Code     | [TestContextWriteIsObsoleteAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TestContextWriteIsObsolete/TestContextWriteIsObsoleteAnalyzer.cs)

## Description

Direct Write calls should be replaced with Out.Write.

## Motivation

The `Write` methods are simple wrappers calling `Out.Write`.
There is no wrapper for `Error` which always required to use `TestContext.Error.Write`.
Besides this being inconsistent, later versions of .NET added new overloads,
 e.g. for `ReadOnlySpan<char>` and `async` methods like `WriteAsync`.
Instead of adding more and more dummy wrappers, it was decided that user code should use
 the `Out` property and then can use any `Write` overload available on `TextWriter`.

## How to fix violations

Simple insert `.Out` between `TestContext` and `.Write`.

`TestContext.WriteLine("This isn't right");`

becomes

`TestContext.Out.WriteLine("This isn't right");`

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see
[MSDN](https://learn.microsoft.com/en-us/visualstudio/code-quality/using-rule-sets-to-group-code-analysis-rules?view=vs-2022).

### Via .editorconfig file

```ini
# NUnit1033: The Write methods on TestContext are Obsolete
dotnet_diagnostic.NUnit1033.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable NUnit1033 // The Write methods on TestContext are Obsolete
Code violating the rule here
#pragma warning restore NUnit1033 // The Write methods on TestContext are Obsolete
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable NUnit1033 // The Write methods on TestContext are Obsolete
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure",
    "NUnit1033:The Write methods on TestContext are Obsolete",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
