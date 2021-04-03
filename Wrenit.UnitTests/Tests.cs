using System;
using NUnit.Framework;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class Tests
	{
		[Test]
		public void VmLifetime()
		{
			WrenitVM vm = new WrenitVM();
			Assert.True(vm.IsAlive);
			vm.Dispose();
			Assert.False(vm.IsAlive);
		}

		[Test]
		public void VmWrite()
		{
			WrenitConfig config = WrenitConfig.GetDefaults();
			string msg = "abc";

			
			config.WriteHandler += (WrenitVM _, string text) =>
			{
				Assert.AreEqual(text, msg);
			};

			config.ErrorHandler += (WrenitVM vm, WrenitResult result, string module, int line, string message) =>
			{
				Assert.IsTrue(false, "script execution failed");
			};

			new WrenitVM(config).Interpret("m", $"System.write(\"{msg}\")");
		}

		[Test]
		public void VmCompileError()
		{
			WrenitConfig config = WrenitConfig.GetDefaults();
			string msg = ";";

			config.WriteHandler += (WrenitVM _, string text) =>
			{
				Assert.IsTrue(false, "should not be hit");
			};

			config.ErrorHandler = (WrenitVM vm, WrenitResult result, string module, int line, string message) =>
			{
				Assert.AreEqual(WrenitResult.CompileError, result);
				Assert.IsTrue(message.Contains(msg));
			};
			
			new WrenitVM(config).Interpret("m", $"System.write(\"abc\"){msg}");
		}

		[Test]
		public void VmRuntimeError()
		{
			WrenitConfig config = WrenitConfig.GetDefaults();
			string msg = "writ";

			config.WriteHandler += (WrenitVM _, string text) =>
			{
				Assert.IsTrue(false, "should not be hit");
			};

			bool first = true;
			config.ErrorHandler = (WrenitVM vm, WrenitResult result, string module, int line, string message) =>
			{
				if (first)
				{
					Assert.AreEqual(WrenitResult.RuntimeError, result);
					Assert.IsTrue(message.Contains(msg));
					Assert.AreEqual(-1, line);
					first = false;
				}
				else
				{
					Assert.AreEqual(WrenitResult.StackTrace, result);
					Assert.AreEqual(1, line);
				}
			};

			new WrenitVM(config).Interpret("m", $"System.{msg}(\"abc\")");
		}
	}
}
