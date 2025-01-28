using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NUnit.Analyzers.Tests.Constants
{
    /// <summary>
    /// Tests to ensure that the string constants in the analyzer project correspond
    /// to the NUnit concepts that they represent.
    /// </summary>
    /// <typeparam name="T">The type of the constants class to test.</typeparam>
    [TestFixture]
    internal abstract class BaseNUnitFrameworkConstantsTests<T>
        where T : class
    {
        private static readonly (string Constant, string TypeName)[] NameOfSource = [];

        private static readonly (string Constant, Type Type)[] FullNameOfTypeSource = [];

        protected abstract IEnumerable<string> Names { get; }

        protected abstract IEnumerable<string> FullNames { get; }

        [TestCaseSource(nameof(NameOfSource))]
        public void TestNameOfConstants((string Constant, string TypeName) pair)
        {
            Assert.That(GetValue(pair.Constant), Is.EqualTo(pair.TypeName), pair.Constant);
        }

        [TestCaseSource(nameof(FullNameOfTypeSource))]
        public void TestFullNameOfConstants((string Constant, Type Type) pair)
        {
            Assert.That(GetValue(pair.Constant), Is.EqualTo(pair.Type.FullName), pair.Constant);
        }

        [Test]
        public void EnsureAllNameOfDefinitionsAreTested()
        {
            EnsureAllNameDefinitionsAreTested("NameOf", this.Names);
        }

        [Test]
        public void EnsureAllFullNameOfTypeDefinitionsAreTested()
        {
            EnsureAllNameDefinitionsAreTested("FullNameOf", this.FullNames);
        }

        protected static IEnumerable<string> Constant<TType>(IEnumerable<(string Constant, TType Type)> source) =>
            source.Select(pair => pair.Constant);

        private static void EnsureAllNameDefinitionsAreTested(string prefix, IEnumerable<string> testedNames)
        {
            IEnumerable<string> allNames =
                typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral)
                .Select(f => f.Name)
                .Where(name => !name.EndsWith("Parameter", StringComparison.Ordinal))
                .Where(name => name.StartsWith(prefix, StringComparison.Ordinal));

            Assert.That(testedNames, Is.EquivalentTo(allNames));
        }

        private static string? GetValue(string fieldName) =>
            typeof(T).GetField(fieldName)?.GetRawConstantValue() as string;
    }
}
