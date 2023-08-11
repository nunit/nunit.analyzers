using System;
using System.IO;

namespace NUnit.Analyzers.Tests
{
    internal static class PathHelper
    {
        public static string GetNuGetPackageDirectory()
        {
            return Environment.GetEnvironmentVariable("NUGET_PACKAGES") ??
                   Path.Combine(GetHomeDirectory(), ".nuget/packages");
        }

        public static string GetHomeDirectory()
        {
            return Environment.GetEnvironmentVariable("HOME") ?? // Linux
                   Environment.GetEnvironmentVariable("USERPROFILE") ?? // Windows
                   throw new NotSupportedException("Cannot determine home directory");
        }
    }
}
