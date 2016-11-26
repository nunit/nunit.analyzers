using System;
using static NUnit.Framework.Assert;

namespace NUnit.Analyzers.IntegrationTests.ClassicModelAssertUsage
{
	public sealed class UsingStaticTestsCases
	{
		public void UseIsTrueAndTrue(string[] args)
		{
			IsTrue(args.Length == 0);
			IsTrue(args.Length == 0, "message {0}");
			IsTrue(args.Length == 0, "message {0}", Guid.NewGuid());

			True(args.Length == 0);
			True(args.Length == 0, "message {0}");
			True(args.Length == 0, "message {0}", Guid.NewGuid());
		}

		public void UseIsFalseAndFalse(string[] args)
		{
			IsFalse(args.Length == 0);
			IsFalse(args.Length == 0, "message {0}");
			IsFalse(args.Length == 0, "message {0}", Guid.NewGuid());

			False(args.Length == 0);
			False(args.Length == 0, "message {0}");
			False(args.Length == 0, "message {0}", Guid.NewGuid());
		}

		public void UseAreEqual(string[] args)
		{
			AreEqual("a", args[0]);
			AreEqual("a", args[0], "message");
			AreEqual("a", args[0], "message {0}", Guid.NewGuid());

			AreEqual(4.5d, double.Parse(args[0]), 0.0000001d, "message");
			AreEqual(4.5d, double.Parse(args[0]), 0.0000001d, "message {0}", Guid.NewGuid());
		}

		public void UseAreNotEqual(string[] args)
		{
			AreNotEqual("a", args[0]);
			AreNotEqual("a", args[0], "message");
			AreNotEqual("a", args[0], "message {0}", Guid.NewGuid());
		}
	}
}
