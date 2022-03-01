using Microsoft.CodeAnalysis;

namespace NUnit.Analyzers.SourceCommon
{
    internal sealed class SourceAttributeInformation
    {
        public SourceAttributeInformation(
            INamedTypeSymbol sourceType,
            string? sourceName,
            SyntaxNode? syntaxNode,
            bool isStringLiteral,
            int? numberOfMethodParameters)
        {
            this.SourceType = sourceType;
            this.SourceName = sourceName;
            this.SyntaxNode = syntaxNode;
            this.IsStringLiteral = isStringLiteral;
            this.NumberOfMethodParameters = numberOfMethodParameters;
        }

        public INamedTypeSymbol SourceType { get; }
        public string? SourceName { get; }
        public SyntaxNode? SyntaxNode { get; }
        public bool IsStringLiteral { get; }
        public int? NumberOfMethodParameters { get; }
    }
}
