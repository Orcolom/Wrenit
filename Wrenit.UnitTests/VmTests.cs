using System;
using NUnit.Framework;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class VmTests
	{
		[Test]
		public void Vm()
		{
			WrenVm vm = new WrenVm();
			Assert.True(vm.IsAlive);
			vm.Dispose();
			Assert.False(vm.IsAlive);
		}
		
		[Test]
		public void Interpret()
		{
			WrenInterpretResult result = new WrenVm().Interpret("main", "System.write(\"hello\")");
			Assert.AreEqual(WrenInterpretResult.Success, result);
			
			result = new WrenVm().Interpret("main", "System.write(\"hello\");");
			Assert.AreEqual(WrenInterpretResult.CompileError, result);
		
			result = new WrenVm().Interpret("main", "System.writ(\"hello\")");
			Assert.AreEqual(WrenInterpretResult.RuntimeError, result);
		}
		
		[Test]
		public void Call()
		{
			WeakReference<WrenVm> weakVm = null;
			WeakReference<WrenHandle> weakHandle = null;

			CallX(out weakVm, out weakHandle);
			GC.WaitForPendingFinalizers();
			
			if (weakHandle.TryGetTarget(out WrenHandle handle))
			{
				if (handle.IsAlive) Assert.Fail();
			}
			if (weakVm.TryGetTarget(out WrenVm vm))
			{
				if (vm.IsAlive) Assert.Fail();
			}
		}

		private void CallX(out WeakReference<WrenVm> weakVm, out WeakReference<WrenHandle> weakHandle)
		{
			using WrenVm vm = new WrenVm();
			vm.Interpret("main", "var c = Fn.new {}");
				
			using WrenSignatureHandle handle = vm.MakeCallHandle("call()");
				
			weakVm = new WeakReference<WrenVm>(vm);
			weakHandle = new WeakReference<WrenHandle>(handle);
				
			vm.EnsureSlots(1);
			vm.GetVariable("main", "c", 0);
			WrenInterpretResult result = vm.Call(handle);
			Assert.AreEqual(WrenInterpretResult.Success, result);

			GC.Collect();
		}
	}
}
