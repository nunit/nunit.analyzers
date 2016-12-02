using NUnit.Framework;
using System;
using static NUnit.Framework.Assert;
using NF = NUnit.Framework;

namespace NUnit.Analyzers.Playground
{
	class Program
	{
		static void Main(string[] args)
		{
			Assert.IsTrue(args.Length == 0);
			Assert.That(args.Length == 0, Is.True);

			IsTrue(args.Length == 0);
			That(args.Length == 0);

			var x = new Action(() => { Assert.IsTrue(true); });

			NF.Assert.IsTrue(args.Length == 0);
			NF.Assert.That(args.Length == 0);

			Assert.IsTrue(args.Length == 0, "message", new object());
			Assert.That(args.Length == 0, Is.True, "message", new object());

			Assert.That(4.500000000000001d, Is.EqualTo(4.5d).Within(0.0000001d), "x is {0}", "blah");
			Program.Foo(22);
		}

		public static void Foo(short a) { }

		[TestCase(3d, "a value", 123, ExpectedResult = 22)]
		public int RunTest(double a, string b, int c) { return 22; }
	}
}
