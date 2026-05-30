#if !NUNIT5
using System;
using System.Collections.Generic;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;

namespace NUnit.Analyzers.Tests.Constants
{
    /// <summary>
    /// Tests to ensure that the string constants in the analyzer project correspond
    /// to the NUnit concepts that they represent.
    /// </summary>
    [TestFixture]
    internal sealed class NUnitPreV5FrameworkConstantsTests : BaseNUnitFrameworkConstantsTests<NUnitPreV5FrameworkConstants>
    {
        public static readonly (string Constant, string TypeName)[] NameOfSource =
        [
        ];

        public static readonly (string Constant, Type Type)[] FullNameOfTypeSource =
        [
            (nameof(NUnitPreV5FrameworkConstants.FullNameOfSameAsConstraint), typeof(SameAsConstraint)),

            (nameof(NUnitPreV5FrameworkConstants.FullNameOfActualValueDelegate), typeof(ActualValueDelegate<>)),
            (nameof(NUnitPreV5FrameworkConstants.FullNameOfTestDelegate), typeof(TestDelegate)),
            (nameof(NUnitPreV5FrameworkConstants.FullNameOfAsyncTestDelegate), typeof(AsyncTestDelegate)),
        ];

        protected override IEnumerable<string> Names => Constant(NameOfSource);

        protected override IEnumerable<string> FullNames => Constant(FullNameOfTypeSource);
    }
}

#endif
