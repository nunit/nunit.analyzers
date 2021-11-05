using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.ConstraintUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests
{
    public class DocumentationTests
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> analyzers =
            typeof(BaseAssertionAnalyzer)
                .Assembly
                .GetTypes()
                .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
                .OrderBy(x => x.Name)
                .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                .ToArray();

        private static readonly IReadOnlyList<string> diagnosticsWithCodeFixes =
            typeof(BaseConditionConstraintCodeFix)
                .Assembly
                .GetTypes()
                .Where(t => typeof(CodeFixProvider).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (CodeFixProvider)Activator.CreateInstance(t))
                .SelectMany(c => c.FixableDiagnosticIds)
                .ToArray();

        private static readonly IReadOnlyList<DescriptorInfo> descriptorInfos =
            analyzers
            .SelectMany(DescriptorInfo.Create)
            .ToArray();

        private static readonly IReadOnlyList<DiagnosticSuppressor> suppressors =
            typeof(BaseAssertionAnalyzer)
                .Assembly
                .GetTypes()
                .Where(t => typeof(DiagnosticSuppressor).IsAssignableFrom(t) && !t.IsAbstract)
                .OrderBy(x => x.Name)
                .Select(t => (DiagnosticSuppressor)Activator.CreateInstance(t))
                .ToArray();

        private static readonly IReadOnlyList<SuppressorInfo> suppressorInfos =
            suppressors
            .SelectMany(SuppressorInfo.Create)
            .ToArray();

        private static IReadOnlyList<DescriptorInfo> DescriptorsWithDocs =>
            descriptorInfos.Where(d => d.DocumentationFile.Exists).ToArray();

        private static IReadOnlyList<SuppressorInfo> SuppressorsWithDocs =>
            suppressorInfos.Where(d => d.DocumentationFile.Exists).ToArray();

        private static IReadOnlyList<BaseInfo> RulesWithDocs =>
            ((IEnumerable<BaseInfo>)DescriptorsWithDocs).Concat(SuppressorsWithDocs).ToArray();

        private static DirectoryInfo RepositoryDirectory =>
            SolutionFile.Find("nunit.analyzers.sln").Directory.Parent;

        private static DirectoryInfo DocumentsDirectory =>
            RepositoryDirectory.EnumerateDirectories("documentation", SearchOption.TopDirectoryOnly).Single();

        private static Dictionary<DiagnosticSeverity, string> SeverityEmoji => new()
        {
            { DiagnosticSeverity.Hidden, ":thought_balloon:" },
            { DiagnosticSeverity.Info, ":information_source:" },
            { DiagnosticSeverity.Warning, ":warning:" },
            { DiagnosticSeverity.Error, ":exclamation:" }
        };

        [TestCaseSource(nameof(descriptorInfos))]
        public void EnsureAllDescriptorsHaveDocumentation(DescriptorInfo descriptorInfo)
        {
            if (!descriptorInfo.DocumentationFile.Exists)
            {
                var descriptor = descriptorInfo.Descriptor;
                var id = descriptor.Id;
                DumpIfDebug(descriptorInfo.Stub);
                File.WriteAllText(descriptorInfo.DocumentationFile.Name + ".generated", descriptorInfo.Stub);
                Assert.Fail($"Documentation is missing for {id}");
            }
        }

        [TestCaseSource(nameof(suppressorInfos))]
        public void EnsureAllSuppressorsHaveDocumentation(SuppressorInfo suppressorInfo)
        {
            if (!suppressorInfo.DocumentationFile.Exists)
            {
                var descriptor = suppressorInfo.Descriptor;
                var id = descriptor.Id;
                DumpIfDebug(suppressorInfo.Stub);
                File.WriteAllText(suppressorInfo.DocumentationFile.Name + ".generated", suppressorInfo.Stub);
                Assert.Fail($"Documentation is missing for {id}");
            }
        }

        [TestCaseSource(nameof(descriptorInfos))]
        public void EnsureThatAllIdsAreUnique(DescriptorInfo descriptorInfo)
        {
            Assert.AreEqual(1, descriptorInfos.Select(x => x.Descriptor)
                                              .Distinct()
                                              .Count(d => d.Id == descriptorInfo.Descriptor.Id));
        }

        [TestCaseSource(nameof(descriptorInfos))]
        public void EnsureThatHelpLinkUriIsCorrect(DescriptorInfo descriptorInfo)
        {
            Assert.That(descriptorInfo.Descriptor.HelpLinkUri, Is.Not.Null.And.Not.Empty);
            var expectedUri = $"https://github.com/nunit/nunit.analyzers/tree/master/documentation/{descriptorInfo.Descriptor.Id}.md";
            Assert.That(descriptorInfo.Descriptor.HelpLinkUri, Is.EqualTo(expectedUri));
        }

        [TestCaseSource(nameof(RulesWithDocs))]
        public void EnsureThatFirstLineMatchesId(BaseInfo info)
        {
            var firstLine = info.DocumentationFile.AllLines[0];
            Assert.That(firstLine, Is.EqualTo($"# {info.Id}"));
        }

        [TestCaseSource(nameof(RulesWithDocs))]
        public void EnsureThatTitleIsAsExpected(BaseInfo info)
        {
            var expected = new[] { "", $"## {info.Title}" };
            var actual = info
                .DocumentationFile.AllLines
                .Skip(1)
                .Select(l => Replace(l, @"\<", "<"))
                .Take(2);

            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void Description(DescriptorInfo descriptorInfo)
        {
            var expected =
                descriptorInfo.Descriptor
                              .Description
                              .ToString(CultureInfo.InvariantCulture)
                              .Split('\n')
                              .First();
            var actualRaw =
                descriptorInfo.DocumentationFile.AllLines
                              .SkipWhile(l => !l.StartsWith("## Description", StringComparison.OrdinalIgnoreCase))
                              .Skip(1)
                              .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
            var actual = Replace(actualRaw, "`", string.Empty);

            DumpIfDebug(expected);
            DumpIfDebug(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(RulesWithDocs))]
        public void EnsureThatTableIsAsExpected(BaseInfo info)
        {
            const string headerRow = "| Topic    | Value";
            var expected = GetTable(info.Stub, headerRow);
            DumpIfDebug(expected);
            var actual = GetTable(info.DocumentationFile.AllText, headerRow);
            CodeAssert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(RulesWithDocs))]
        public void EnsureThatConfigSeverityIsAsExpected(BaseInfo info)
        {
            var expected = GetConfigSeverity(info.Stub);
            DumpIfDebug(expected);
            var actual = GetConfigSeverity(info.DocumentationFile.AllText);
            CodeAssert.AreEqual(expected, actual);

            string GetConfigSeverity(string doc)
            {
                return GetSection(doc, "<!-- start generated config severity -->", "<!-- end generated config severity -->");
            }

            string GetSection(string doc, string startToken, string endToken)
            {
                var start = doc.IndexOf(startToken, StringComparison.Ordinal);
                Assert.That(start, Is.GreaterThan(0), "Missing: " + startToken);
                var end = doc.IndexOf(endToken, start, StringComparison.Ordinal);
                Assert.That(end, Is.GreaterThan(start), "Missing: " + endToken);
                return doc.Substring(start, end + endToken.Length - start);
            }
        }

        [TestCase(Categories.Structure, 0)]
        [TestCase(Categories.Assertion, 1)]
        public void EnsureThatAnalyzerIndexIsAsExpected(string category, int tableNumber)
        {
            var builder = new StringBuilder();
            const string headerRow = "| Id       | Title       | :mag: | :memo: | :bulb: |";
            builder.AppendLine(headerRow)
                   .AppendLine("| :--      | :--         | :--:  | :--:   | :--:   |");

            var descriptors = DescriptorsWithDocs
                .Select(x => x.Descriptor)
                .Where(x => x.Category == category)
                .Distinct()
                .OrderBy(x => x.Id);

            foreach (var descriptor in descriptors)
            {
                var enabledEmoji = descriptor.IsEnabledByDefault ? ":white_check_mark:" : ":x:";
                var severityEmoji = SeverityEmoji[descriptor.DefaultSeverity];

                var codefixEmoji = diagnosticsWithCodeFixes.Contains(descriptor.Id)
                    ? ":white_check_mark:"
                    : ":x:";

                builder.Append($"| [{descriptor.Id}]({descriptor.HelpLinkUri}) ")
                       .Append($"| {EscapeTags(descriptor.Title)} | {enabledEmoji} ")
                       .AppendLine($"| {severityEmoji} | {codefixEmoji} |");
            }

            var expected = builder.ToString();
            DumpIfDebug(expected);
            var actual = GetTable(File.ReadAllText(Path.Combine(DocumentsDirectory.FullName, "index.md")), headerRow, tableNumber);
            CodeAssert.AreEqual(expected, actual);
        }

        [TestCase(2)]
        public void EnsureThatSuppressionIndexIsAsExpected(int tableNumber)
        {
            var builder = new StringBuilder();
            const string headerRow = "| Id       | Title       | :mag: | :memo: | :bulb: |";
            builder.AppendLine(headerRow)
                   .AppendLine("| :--      | :--         | :--:  | :--:   | :--:   |");

            var suppressors = SuppressorsWithDocs
                .Distinct()
                .OrderBy(x => x.Descriptor.Id);

            foreach (var suppressor in suppressors)
            {
                var enabledEmoji = ":white_check_mark:";
                var severityEmoji = SeverityEmoji[DiagnosticSeverity.Info];

                var codefixEmoji = ":x:";

                builder.Append($"| [{suppressor.Id}]({suppressor.HelpLinkUri}) ")
                       .Append($"| {EscapeTags(suppressor.Title)} | {enabledEmoji} ")
                       .AppendLine($"| {severityEmoji} | {codefixEmoji} |");
            }

            var expected = builder.ToString();
            DumpIfDebug(expected);
            var actual = GetTable(File.ReadAllText(Path.Combine(DocumentsDirectory.FullName, "index.md")), headerRow, tableNumber);
            CodeAssert.AreEqual(expected, actual);
        }

        private static string GetTable(string doc, string headerRow, int tableNumber = 0)
        {
            int startIndex = -1;
            do
            {
                startIndex = doc.IndexOf(headerRow, startIndex + 1, StringComparison.Ordinal);
            }
            while (startIndex >= 0 && tableNumber-- > 0);

            return startIndex < 0 ? string.Empty : doc.Substring(startIndex, TableLength());

            int TableLength()
            {
                var length = 0;
                while (startIndex + length < doc.Length)
                {
                    if (doc.Length > startIndex + length &&
                        doc[startIndex + length] == '\n' &&
                        doc.ElementAtOrDefault(startIndex + length + 1) != '|')
                    {
                        length++;
                        return length;
                    }

                    length++;
                }

                return length;
            }
        }

        [Conditional("DEBUG")]
        private static void DumpIfDebug(string text)
        {
            Console.Write(text);
            Console.WriteLine();
            Console.WriteLine();
        }

        private static string EscapeTags(LocalizableString str)
            => Replace(str.ToString(CultureInfo.InvariantCulture), "<", @"\<");

        private static string Replace(string originalString, string oldValue, string newValue)
        {
#if NET461
            return originalString?.Replace(oldValue, newValue);
#else
            return originalString?.Replace(oldValue, newValue, StringComparison.Ordinal);
#endif
        }

        public abstract class BaseInfo
        {
            protected BaseInfo(Type analyzerType, string id)
            {
                this.Id = id;
                this.DocumentationFile = new MarkdownFile(Path.Combine(DocumentsDirectory.FullName, id + ".md"));
                this.AnalyzerFile = CodeFile.Find(analyzerType);
            }

            public string Id { get; }

            public MarkdownFile DocumentationFile { get; }

            public CodeFile AnalyzerFile { get; }

            public string Stub { get; protected set; }

            public abstract string Title { get; }
        }

        public sealed class DescriptorInfo : BaseInfo
        {
            private DescriptorInfo(DiagnosticAnalyzer analyzer, DiagnosticDescriptor descriptor)
                : base(analyzer.GetType(), descriptor.Id)
            {
                this.Analyzer = analyzer;
                this.Descriptor = descriptor;
                this.Stub = CreateStub(analyzer, descriptor);
            }

            public DiagnosticAnalyzer Analyzer { get; }

            public DiagnosticDescriptor Descriptor { get; }

            public override string Title => (string)this.Descriptor.Title;

            public static IEnumerable<DescriptorInfo> Create(DiagnosticAnalyzer analyzer)
            {
                foreach (var descriptor in analyzer.SupportedDiagnostics)
                {
                    yield return new DescriptorInfo(analyzer, descriptor);
                }
            }

            public override string ToString() => this.Descriptor.Id;

            private static string CreateStub(DiagnosticAnalyzer analyzer, DiagnosticDescriptor descriptor)
            {
                var builder = new StringBuilder();
                builder.Append($"|{(builder.Length == 0 ? " Code     " : "          ")}| ");
                builder.Append($"[{analyzer.GetType().Name}]({CodeFile.Find(analyzer.GetType()).Uri})");

                var text = builder.ToString();
                var stub = $@"# {descriptor.Id}

## {EscapeTags(descriptor.Title)}

| Topic    | Value
| :--      | :--
| Id       | {descriptor.Id}
| Severity | {descriptor.DefaultSeverity}
| Enabled  | {(descriptor.IsEnabledByDefault ? "True" : "False")}
| Category | {descriptor.Category}
| Code     | [<TYPENAME>](<URL>)

## Description

{EscapeTags(descriptor.Description)}

## Motivation

ADD MOTIVATION HERE

## How to fix violations

ADD HOW TO FIX VIOLATIONS HERE

<!-- start generated config severity -->
## Configure severity

### Via ruleset file

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via .editorconfig file

```ini
# {descriptor.Id}: {descriptor.Title.ToString(CultureInfo.InvariantCulture)}
dotnet_diagnostic.{descriptor.Id}.severity = chosenSeverity
```

where `chosenSeverity` can be one of `none`, `silent`, `suggestion`, `warning`, or `error`.

### Via #pragma directive

```csharp
#pragma warning disable {descriptor.Id} // {descriptor.Title.ToString(CultureInfo.InvariantCulture)}
Code violating the rule here
#pragma warning restore {descriptor.Id} // {descriptor.Title.ToString(CultureInfo.InvariantCulture)}
```

Or put this at the top of the file to disable all instances.

```csharp
#pragma warning disable {descriptor.Id} // {descriptor.Title.ToString(CultureInfo.InvariantCulture)}
```

### Via attribute `[SuppressMessage]`

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage(""{descriptor.Category}"",
    ""{descriptor.Id}:{descriptor.Title.ToString(CultureInfo.InvariantCulture)}"",
    Justification = ""Reason..."")]
```
<!-- end generated config severity -->
";
                return Replace(stub, "| Code     | [<TYPENAME>](<URL>)", text);
            }
        }

        public sealed class SuppressorInfo : BaseInfo
        {
            private static readonly IEqualityComparer<SuppressionDescriptor> Comparer = new SuppressionDescriptorComparer();

            private SuppressorInfo(DiagnosticSuppressor suppressor, SuppressionDescriptor descriptor)
                : base(suppressor.GetType(), descriptor.Id)
            {
                this.Suppressor = suppressor;
                this.Descriptor = descriptor;
                var name = descriptor.Id + ".md";
                this.HelpLinkUri = $"https://github.com/nunit/nunit.analyzers/tree/master/documentation/{name}";
                this.Stub = CreateStub(suppressor, descriptor);
            }

            public DiagnosticSuppressor Suppressor { get; }

            public SuppressionDescriptor Descriptor { get; }

            public override string Title => (string)this.Descriptor.Justification;

            public string HelpLinkUri { get; }

            public static IEnumerable<SuppressorInfo> Create(DiagnosticSuppressor suppressor)
            {
                foreach (var descriptor in suppressor.SupportedSuppressions.Distinct(Comparer))
                {
                    yield return new SuppressorInfo(suppressor, descriptor);
                }
            }

            public override string ToString() => this.Descriptor.Id;

            private static string CreateStub(DiagnosticSuppressor suppressor, SuppressionDescriptor descriptor)
            {
                var builder = new StringBuilder();
                builder.Append("| Code     | ");
                builder.Append($"[{suppressor.GetType().Name}]({CodeFile.Find(suppressor.GetType()).Uri})");

                var text = builder.ToString();
                var stub = $@"# {descriptor.Id}

## {EscapeTags(descriptor.Justification)}

| Topic    | Value
| :--      | :--
| Id       | {descriptor.Id}
| Severity | Info
| Enabled  | True
| Category | Suppressor
| Code     | [<TYPENAME>](<URL>)

## Description

{EscapeTags(descriptor.Justification)}

## Motivation

ADD MOTIVATION HERE

## How to fix violations

ADD HOW TO FIX VIOLATIONS HERE

<!-- start generated config severity -->
## Configure severity

The rule has no severity, but can be disabled.

### Via ruleset file

To disable the rule for a project, you need to add a
[ruleset file](https://github.com/nunit/nunit.analyzers/blob/master/src/nunit.analyzers/DiagnosticSuppressors/NUnit.Analyzers.Suppressions.ruleset)

```xml
<?xml version=""1.0"" encoding=""utf-8""?>
<RuleSet Name=""NUnit.Analyzer Suppressions"" Description=""DiagnosticSuppression Rules"" ToolsVersion=""12.0"">
  <Rules AnalyzerId=""DiagnosticSuppressors"" RuleNamespace=""NUnit.NUnitAnalyzers"">
    <Rule Id=""NUnit3001"" Action=""Info"" /> <!-- Possible Null Reference -->
    <Rule Id=""NUnit3002"" Action=""Info"" /> <!-- NonNullableField/Property is Uninitialized -->
  </Rules>
</RuleSet>
```

and add it to the project like:

```xml
<PropertyGroup>
  <CodeAnalysisRuleSet>NUnit.Analyzers.Suppressions.ruleset</CodeAnalysisRuleSet>
</PropertyGroup>
```

For more info about rulesets see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via .editorconfig file

This is currently not working. Waiting for [Roslyn](https://github.com/dotnet/roslyn/issues/49727)

```ini
# {descriptor.Id}: {descriptor.Justification.ToString(CultureInfo.InvariantCulture)}
dotnet_diagnostic.{descriptor.Id}.severity = none
```
<!-- end generated config severity -->
";
                return Replace(stub, "| Code     | [<TYPENAME>](<URL>)", text);
            }
        }

        public sealed class SuppressionDescriptorComparer : IEqualityComparer<SuppressionDescriptor>
        {
            public bool Equals(SuppressionDescriptor x, SuppressionDescriptor y) => x?.Id == y?.Id;

            public int GetHashCode(SuppressionDescriptor obj) => obj?.Id.GetHashCode() ?? 0;
        }

        public class MarkdownFile
        {
            public MarkdownFile(string name)
            {
                this.Name = name;
                if (File.Exists(name))
                {
                    this.AllText = File.ReadAllText(name);
                    this.AllLines = File.ReadAllLines(name);
                }
            }

            public string Name { get; }

            public bool Exists => File.Exists(this.Name);

            public string AllText { get; }

            public IReadOnlyList<string> AllLines { get; }
        }

        public class CodeFile
        {
            private const string RepoOnMaster = "https://github.com/nunit/nunit.analyzers/blob/master";

            private static readonly ConcurrentDictionary<Type, CodeFile> cache = new();

            public CodeFile(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public string Uri => RepoOnMaster + this.Name.Substring(RepositoryDirectory.FullName.Length).Replace('\\', '/');

            public static CodeFile Find(Type type)
            {
                return cache.GetOrAdd(type, x => FindCore(x.Name + ".cs"));
            }

            private static CodeFile FindCore(string name)
            {
                var fileName = cache.Values
                                    .Select(x => Path.GetDirectoryName(x.Name))
                                    .Distinct()
                                    .SelectMany(d => Directory.EnumerateFiles(d, name, SearchOption.TopDirectoryOnly))
                                    .FirstOrDefault() ??
                               Directory.EnumerateFiles(RepositoryDirectory.FullName, name, SearchOption.AllDirectories)
                                        .First();
                return new CodeFile(fileName);
            }
        }
    }
}
