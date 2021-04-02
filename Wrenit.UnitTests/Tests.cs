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
			WrenitVM wrenitVM = new WrenitVM();
			string msg = "abc";

			wrenitVM.WriteHandler = (WrenitVM _, string text) =>
			{
				Assert.AreEqual(text, msg);
			};

			wrenitVM.ErrorHandler = (WrenitVM vm, WrenitResult result, string module, int line, string message) =>
			{
				Assert.IsTrue(false, "script execution failed");
			};

			wrenitVM.Interpret("m", $"System.write(\"{msg}\")");
		}

		[Test]
		public void VmCompileError()
		{
			WrenitVM wrenitVM = new WrenitVM();
			string msg = ";";

			wrenitVM.WriteHandler += (WrenitVM _, string text) =>
			{
				Assert.IsTrue(false, "should not be hit");
			};

			wrenitVM.ErrorHandler = (WrenitVM vm, WrenitResult result, string module, int line, string message) =>
			{
				Assert.AreEqual(WrenitResult.CompileError, result);
				Assert.IsTrue(message.Contains(msg));
			};
			
			wrenitVM.Interpret("m", $"System.write(\"abc\"){msg}");
		}

		[Test]
		public void VmRuntimeError()
		{
			WrenitVM wrenitVM = new WrenitVM();
			string msg = "writ";

			wrenitVM.WriteHandler += (WrenitVM _, string text) =>
			{
				Assert.IsTrue(false, "should not be hit");
			};

			bool first = true;
			wrenitVM.ErrorHandler = (WrenitVM vm, WrenitResult result, string module, int line, string message) =>
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

			wrenitVM.Interpret("m", $"System.{msg}(\"abc\")");
		}
	}
}
