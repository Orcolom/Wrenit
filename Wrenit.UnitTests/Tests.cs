using System;
using NUnit.Framework;

namespace Wren.it.UnitTests
{
	[TestFixture]
	public class Tests
	{
		[Test]
		public void VmLifetime()
		{
			WrenVm vm = new WrenVm();
			Assert.True(vm.IsAlive);
			vm.Dispose();
			Assert.False(vm.IsAlive);
		}

		[Test]
		public void VmWrite()
		{
			WrenConfig config = WrenConfig.GetDefaults();
			string msg = "abc";
			
			config.WriteHandler += (_, text) =>
			{
				Assert.AreEqual(text, msg);
			};

			config.ErrorHandler += (vm, result, module, line, message) =>
			{
				Assert.IsTrue(false, "script execution failed");
			};

			new WrenVm(config).Interpret("m", $"System.write(\"{msg}\")");
		}

		[Test]
		public void VmCompileError()
		{
			WrenConfig config = WrenConfig.GetDefaults();
			string msg = ";";

			config.WriteHandler += (_, text) =>
			{
				Assert.IsTrue(false, "should not be hit");
			};

			config.ErrorHandler = (vm, result, module, line, message) =>
			{
				Assert.AreEqual(WrenErrorType.CompileError, result);
				Assert.IsTrue(message.Contains(msg));
			};
			
			new WrenVm(config).Interpret("m", $"System.write(\"abc\"){msg}");
		}

		[Test]
		public void VmRuntimeError()
		{
			WrenConfig config = WrenConfig.GetDefaults();
			string msg = "writ";

			config.WriteHandler += (_, text) =>
			{
				Assert.IsTrue(false, "should not be hit");
			};

			bool first = true;
			config.ErrorHandler = (vm, result, module, line, message) =>
			{
				if (first)
				{
					Assert.AreEqual(WrenErrorType.RuntimeError, result);
					Assert.IsTrue(message.Contains(msg));
					Assert.AreEqual(-1, line);
					first = false;
				}
				else
				{
					Assert.AreEqual(WrenErrorType.StackTrace, result);
					Assert.AreEqual(1, line);
				}
			};

			new WrenVm(config).Interpret("m", $"System.{msg}(\"abc\")");
		}
	}
}
