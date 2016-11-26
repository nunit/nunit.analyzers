using System;
using NF = NUnit.Framework;

namespace NUnit.Analyzers.IntegrationTests.ClassicModelAssertUsage
{
	public sealed class UsingAliasTestsCases
	{
		public void UseIsTrueAndTrue(string[] args)
		{
			NF.Assert.IsTrue(args.Length == 0);
			NF.Assert.IsTrue(args.Length == 0, "message {0}");
			NF.Assert.IsTrue(args.Length == 0, "message {0}", Guid.NewGuid());

			NF.Assert.True(args.Length == 0);
			NF.Assert.True(args.Length == 0, "message {0}");
			NF.Assert.True(args.Length == 0, "message {0}", Guid.NewGuid());
		}

		public void UseIsFalseAndFalse(string[] args)
		{
			NF.Assert.IsFalse(args.Length == 0);
			NF.Assert.IsFalse(args.Length == 0, "message {0}");
			NF.Assert.IsFalse(args.Length == 0, "message {0}", Guid.NewGuid());

			NF.Assert.False(args.Length == 0);
			NF.Assert.False(args.Length == 0, "message {0}");
			NF.Assert.False(args.Length == 0, "message {0}", Guid.NewGuid());
		}

		public void UseAreEqual(string[] args)
		{
			NF.Assert.AreEqual("a", args[0]);
			NF.Assert.AreEqual("a", args[0], "message");
			NF.Assert.AreEqual("a", args[0], "message {0}", Guid.NewGuid());

			NF.Assert.AreEqual(4.5d, double.Parse(args[0]), 0.0000001d, "message");
			NF.Assert.AreEqual(4.5d, double.Parse(args[0]), 0.0000001d, "message {0}", Guid.NewGuid());
		}

		public void UseAreNotEqual(string[] args)
		{
			NF.Assert.AreNotEqual("a", args[0]);
			NF.Assert.AreNotEqual("a", args[0], "message");
			NF.Assert.AreNotEqual("a", args[0], "message {0}", Guid.NewGuid());
		}
	}
}
