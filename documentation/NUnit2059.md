# NUnit2059

## Task result of method should be used

| Topic    | Value
| :--      | :--
| Id       | NUnit2059
| Severity | Error
| Enabled  | True
| Category | Assertion
| Code     | [TaskReturnShouldBeUsedAnalyzer](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/TaskReturnShouldBeUsed/TaskReturnShouldBeUsedAnalyzer.cs)

## Description

Methods that return a Task should have their results observed to ensure proper handling of completion and errors.

## Motivation

In NUnit V5, the return type of `Assert.CatchAsync`, `Assert.ThrowsAsync` and `Assert.DoesNotThrowAsync`
will change from `Exception?` to `Task<Exception?>`.
This allows for better handling of asynchronous assertions,
including proper propagation of exceptions and improved test readability.
By returning a Task, these methods can be awaited,
ensuring that any exceptions thrown during the assertion
are correctly captured and reported by the testing framework.
This change enhances the robustness and reliability of asynchronous tests in NUnit.

Existing code that calls these methods without awaiting the result will not observe the returned Task,
which can lead to compile time errors or unhandled exceptions and incorrect test outcomes.

## How to fix violations

Any existing code needs to add an `await` to observe the returned Task and
change the return type of the test method to `async Task` if necessary.

Before:

```csharp
[Test]
public void TestMethod()
{
    Exception? ex = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
    Assert.That(ex.Message, Is.EqualTo("Expected exception message"));
}
```

After:

```csharp
[Test]
public async Task TestMethod()
{
    Exception? ex = await Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
    Assert.That(ex.Message, Is.EqualTo("Expected exception message"));
}
```

The rule has an associated codefix that will make any possible changes,
including changing the return type from `void` to `async Task` if possible,
and adding the `await` keyword.

If the method is an override, the codefix can not change the return type
to `async Task` but it will add `.Result`.
Even if the method returns `Task` but is not `async`,
the codefix will add `.Result` instead of `await`
as the method likely uses `return Task.FromXXX` or
returns a Task return from another method.

Before:

```csharp
[Test]
public override void TestMethod()
{
    Exception? ex = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
}
```

After:

```csharp
[Test]
public override void TestMethod()
{
    Exception? ex = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0)).Result;
}
```

If the variable is declared as `var`, the codefix can be configured
to add `await` or ignore the violation and leave it up to the user.

```csharp
[Test]
public void TestMethod()
{
    var result = Assert.ThrowsAsync<Exception>(() => Task.FromResult(0));
    Assert.That(result.Message, Is.EqualTo("Expected exception message"));
}
```

By default if the variable contains `"ex"`, but not `"task"`,
the codefix will add `await` otherwise it will ignore the violation.
This can be configured in the .editorconfig file with the following two options.
Note that this only does a simple case-insensitive string match.

```ini
dotnet_diagnostic.NUnit2059.raise_for_var_declaration_containing: ex
dotnet_diagnostic.NUnit2059.do_not_raise_for_var_declaration_containing: task
```

If you want it to be more strict and always add `await` for `var` declarations,
one can set the first option to an empty string.

```ini
dotnet_diagnostic.NUnit2059.raise_for_var_declaration_containing:
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see
[MSDN](https://learn.microsoft.com/en-us/visualstudio/code-quality/using-rule-sets-to-group-code-analysis-rules?view=vs-2022).

### Via .editorconfig file

```ini
# NUnit2059: Task result of method should be used
dotnet_diagnostic.NUnit2059.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable NUnit2059 // Task result of method should be used
Code violating the rule here
#pragma warning restore NUnit2059 // Task result of method should be used
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable NUnit2059 // Task result of method should be used
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion",
    "NUnit2059:Task result of method should be used",
    Justification = "Reason...")]
```
<!-- end generated config severity -->
