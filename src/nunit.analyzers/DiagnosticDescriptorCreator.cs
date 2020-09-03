using System;
using Microsoft.CodeAnalysis;

namespace NUnit.Analyzers
{
    internal static class DiagnosticDescriptorCreator
    {
        internal static DiagnosticDescriptor Create(
            string id,
            string title,
            string messageFormat,
            string category,
            DiagnosticSeverity defaultSeverity,
            string description) =>
            Create(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: true,
                description: description);

        internal static DiagnosticDescriptor Create(
            string id,
            string title,
            string messageFormat,
            string category,
            DiagnosticSeverity defaultSeverity,
            bool isEnabledByDefault,
            string description) =>
            new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: isEnabledByDefault,
                description: description,
                helpLinkUri: CreateLink(id),
                customTags: Array.Empty<string>());

        private static string CreateLink(string id) =>
            $"https://github.com/nunit/nunit.analyzers/tree/master/documentation/{id}.md";
    }
}
