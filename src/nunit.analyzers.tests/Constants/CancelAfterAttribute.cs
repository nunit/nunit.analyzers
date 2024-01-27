#if !NUNIT4

using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class CancelAfterAttribute : Attribute
    {
    }
}

#endif
