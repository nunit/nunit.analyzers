using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Analyzers.ClassicModelAssertUsage;
using NUnit.Framework;

namespace NUnit.Analyzers.Tests.ClassicModelAssertUsage
{
    [TestFixture]
    public sealed class ClassicModelAssertUsageCodeFixTests
    {
        [Test]
        public void GetFixAllProvider()
        {
            var codeFix = new TestableClassicModelAssertUsageCodeFix();
            Assert.That(codeFix.GetFixAllProvider(), Is.EqualTo(WellKnownFixAllProviders.BatchFixer));
        }

        private sealed class TestableClassicModelAssertUsageCodeFix : ClassicModelAssertUsageCodeFix
        {
            public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray<string>.Empty;

            protected override (ArgumentSyntax ActualArgument, ArgumentSyntax? ConstraintArgument) ConstructActualAndConstraintArguments(
                Diagnostic diagnostic,
                IReadOnlyDictionary<string, ArgumentSyntax> argumentNamesToArguments) =>
                throw new NotImplementedException();
        }
    }
}
