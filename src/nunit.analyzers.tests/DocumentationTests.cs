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

        private static IReadOnlyList<DescriptorInfo> DescriptorsWithDocs =>
            descriptorInfos.Where(d => d.DocumentationFile.Exists).ToArray();

        private static DirectoryInfo RepositoryDirectory =>
            SolutionFile.Find("nunit.analyzers.sln").Directory.Parent;

        private static DirectoryInfo DocumentsDirectory =>
            RepositoryDirectory.EnumerateDirectories("documentation", SearchOption.TopDirectoryOnly).Single();

        private static Dictionary<DiagnosticSeverity, string> SeverityEmoji => new Dictionary<DiagnosticSeverity, string>
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

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void EnsureThatFirstLineMatchesId(DescriptorInfo descriptorInfo)
        {
            var firstLine = descriptorInfo.DocumentationFile.AllLines[0];
            Assert.That(firstLine, Is.EqualTo($"# {descriptorInfo.Descriptor.Id}"));
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void EnsureThatTitleIsAsExpected(DescriptorInfo descriptorInfo)
        {
            var expected = new[] { "", $"## {descriptorInfo.Descriptor.Title}" };
            var actual = descriptorInfo
                .DocumentationFile.AllLines
                .Skip(1)
                .Select(l => l.Replace(@"\<", "<", StringComparison.Ordinal))
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
            var actual =
                descriptorInfo.DocumentationFile.AllLines
                              .SkipWhile(l => !l.StartsWith("## Description", StringComparison.OrdinalIgnoreCase))
                              .Skip(1)
                              .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))
                              ?.Replace("`", string.Empty, StringComparison.Ordinal);

            DumpIfDebug(expected);
            DumpIfDebug(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void EnsureThatTableIsAsExpected(DescriptorInfo descriptorInfo)
        {
            const string headerRow = "| Topic    | Value";
            var expected = GetTable(descriptorInfo.Stub, headerRow);
            DumpIfDebug(expected);
            var actual = GetTable(descriptorInfo.DocumentationFile.AllText, headerRow);
            CodeAssert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(DescriptorsWithDocs))]
        public void EnsureThatConfigSeverityIsAsExpected(DescriptorInfo descriptorInfo)
        {
            var expected = GetConfigSeverity(descriptorInfo.Stub);
            DumpIfDebug(expected);
            var actual = GetConfigSeverity(descriptorInfo.DocumentationFile.AllText);
            CodeAssert.AreEqual(expected, actual);

            string GetConfigSeverity(string doc)
            {
                return GetSection(doc, "<!-- start generated config severity -->", "<!-- end generated config severity -->");
            }

            string GetSection(string doc, string startToken, string endToken)
            {
                var start = doc.IndexOf(startToken, StringComparison.Ordinal);
                var end = doc.IndexOf(endToken, StringComparison.Ordinal) + endToken.Length;
                return doc.Substring(start, end - start);
            }
        }

        [TestCase(Categories.Structure, true)]
        [TestCase(Categories.Assertion, false)]
        public void EnsureThatIndexIsAsExpected(string category, bool firstTable)
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
            var actual = GetTable(File.ReadAllText(Path.Combine(DocumentsDirectory.FullName, "index.md")), headerRow, firstTable);
            CodeAssert.AreEqual(expected, actual);
        }

        private static string GetTable(string doc, string headerRow, bool firstTable = true)
        {
            var startIndex = firstTable
                ? doc.IndexOf(headerRow, StringComparison.Ordinal)
                : doc.LastIndexOf(headerRow, StringComparison.Ordinal);

            if (startIndex < 0)
            {
                return string.Empty;
            }

            return doc.Substring(startIndex, TableLength());

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
            => str.ToString(CultureInfo.InvariantCulture).Replace("<", @"\<", StringComparison.Ordinal);

        public class DescriptorInfo
        {
            private DescriptorInfo(DiagnosticAnalyzer analyzer, DiagnosticDescriptor descriptor)
            {
                this.Analyzer = analyzer;
                this.Descriptor = descriptor;
                this.DocumentationFile = new MarkdownFile(Path.Combine(DocumentsDirectory.FullName, descriptor.Id + ".md"));
                this.AnalyzerFile = CodeFile.Find(analyzer.GetType());
                this.Stub = CreateStub(descriptor);
            }

            public DiagnosticAnalyzer Analyzer { get; }

            public DiagnosticDescriptor Descriptor { get; }

            public MarkdownFile DocumentationFile { get; }

            public CodeFile AnalyzerFile { get; }

            public string Stub { get; }

            public static IEnumerable<DescriptorInfo> Create(DiagnosticAnalyzer analyzer)
            {
                foreach (var descriptor in analyzer.SupportedDiagnostics)
                {
                    yield return new DescriptorInfo(analyzer, descriptor);
                }
            }

            public override string ToString() => this.Descriptor.Id;

            private static string CreateStub(DiagnosticDescriptor descriptor)
            {
                var builder = new StringBuilder();
                foreach (var analyzer in analyzers.Where(x => x.SupportedDiagnostics.Any(d => d.Id == descriptor.Id)))
                {
                    _ = builder.Append($"|{(builder.Length == 0 ? " Code     " : "          ")}| ")
                               .AppendLine($"[{analyzer.GetType().Name}]({CodeFile.Find(analyzer.GetType()).Uri})");
                }

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

                return stub.Replace("| Code     | [<TYPENAME>](<URL>)\r\n", text, StringComparison.Ordinal)
                           .Replace("| Code     | [<TYPENAME>](<URL>)\n", text, StringComparison.Ordinal);
            }
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

            private static readonly ConcurrentDictionary<Type, CodeFile> cache =
                new ConcurrentDictionary<Type, CodeFile>();

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
