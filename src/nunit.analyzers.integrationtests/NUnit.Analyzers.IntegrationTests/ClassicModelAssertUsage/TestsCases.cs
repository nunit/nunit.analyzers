using NUnit.Framework;
using System;

namespace NUnit.Analyzers.IntegrationTests.ClassicModelAssertUsage
{
	public sealed class TestsCases
	{
		public void UseIsTrueAndTrue(string[] args)
		{
			Assert.IsTrue(args.Length == 0);
			Assert.IsTrue(args.Length == 0, "message");
			Assert.IsTrue(args.Length == 0, "message {0}", Guid.NewGuid());

			Assert.True(args.Length == 0);
			Assert.True(args.Length == 0, "message");
			Assert.True(args.Length == 0, "message {0}", Guid.NewGuid());
		}

		public void UseIsFalseAndFalse(string[] args)
		{
			Assert.IsFalse(args.Length == 0);
			Assert.IsFalse(args.Length == 0, "message");
			Assert.IsFalse(args.Length == 0, "message {0}", Guid.NewGuid());

			Assert.False(args.Length == 0);
			Assert.False(args.Length == 0, "message");
			Assert.False(args.Length == 0, "message {0}", Guid.NewGuid());
		}

		public void UseAreEqual(string[] args)
		{
			Assert.AreEqual("a", args[0]);
			Assert.AreEqual("a", args[0], "message");
			Assert.AreEqual("a", args[0], "message {0}", Guid.NewGuid());

			Assert.AreEqual(4.5d, double.Parse(args[0]), 0.0000001d, "message");
			Assert.AreEqual(4.5d, double.Parse(args[0]), 0.0000001d, "message {0}", Guid.NewGuid());
		}

		public void UseAreNotEqual(string[] args)
		{
			Assert.AreNotEqual("a", args[0]);
			Assert.AreNotEqual("a", args[0], "message");
			Assert.AreNotEqual("a", args[0], "message {0}", Guid.NewGuid());
		}
	}
}
