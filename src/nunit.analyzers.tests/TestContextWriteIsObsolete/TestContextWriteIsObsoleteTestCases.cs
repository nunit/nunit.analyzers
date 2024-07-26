namespace NUnit.Analyzers.Tests.TestContextWriteIsObsolete
{
    internal static class TestContextWriteIsObsoleteTestCases
    {
        public static readonly string[] WriteInvocations =
        {
            "Write(true)",
            "Write('!')",
            "Write(new char[] { '!', '!' })",
            "Write(default(char[]))",
            "Write(1D)",
            "Write(1)",
            "Write(1L)",
            "Write(1M)",
            "Write(default(object))",
            "Write(1F)",
            "Write(\"NUnit\")",
            "Write(default(string))",
            "Write(1U)",
            "Write(1UL)",
            "Write(\"{0}\", 1)",
            "Write(\"{0} + {1}\", 1, 2)",
            "Write(\"{0} + {1} = {2}\", 1, 2, 3)",
            "Write(\"{0} + {1} = {2} + {3}\", 1, 2, 2, 1)",
            "WriteLine()",
            "WriteLine(true)",
            "WriteLine('!')",
            "WriteLine(new char[] { '!', '!' })",
            "WriteLine(default(char[]))",
            "WriteLine(1D)",
            "WriteLine(1)",
            "WriteLine(1L)",
            "WriteLine(1M)",
            "WriteLine(default(object))",
            "WriteLine(1F)",
            "WriteLine(\"NUnit\")",
            "Write(default(string))",
            "WriteLine(1U)",
            "WriteLine(1UL)",
            "WriteLine(\"{0}\", 1)",
            "WriteLine(\"{0} + {1}\", 1, 2)",
            "WriteLine(\"{0} + {1} = {2}\", 1, 2, 3)",
            "WriteLine(\"{0} + {1} = {2} + {3}\", 1, 2, 2, 1)",
        };
    }
}
