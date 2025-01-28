#if NUNIT4
using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Analyzers.Constants;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NUnit.Analyzers.Tests.Constants
{
    /// <summary>
    /// Tests to ensure that the string constants in the analyzer project correspond
    /// to the NUnit concepts that they represent.
    /// </summary>
    [TestFixture]
    internal sealed class NUnitV4FrameworkConstantsTests : BaseNUnitFrameworkConstantsTests<NUnitV4FrameworkConstants>
    {
        public static readonly (string Constant, string TypeName)[] NameOfSource =
        [
            (nameof(NUnitV4FrameworkConstants.NameOfIsDefault), nameof(Is.Default)),
            (nameof(NUnitV4FrameworkConstants.NameOfMultipleAsync), nameof(Assert.MultipleAsync)),
            (nameof(NUnitV4FrameworkConstants.NameOfEnterMultipleScope), nameof(Assert.EnterMultipleScope)),

            (nameof(NUnitV4FrameworkConstants.NameOfCancelAfterAttribute), nameof(CancelAfterAttribute)),
        ];

        public static readonly (string Constant, Type Type)[] FullNameOfTypeSource =
        [
            (nameof(NUnitV4FrameworkConstants.FullNameOfCancelAfterAttribute), typeof(CancelAfterAttribute)),
            (nameof(NUnitV4FrameworkConstants.FullNameOfCancellationToken), typeof(CancellationToken)),
        ];

        protected override IEnumerable<string> Names => Constant(NameOfSource);

        protected override IEnumerable<string> FullNames => Constant(FullNameOfTypeSource);
    }
}

#endif
