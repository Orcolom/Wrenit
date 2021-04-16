using System;
using NUnit.Framework;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class VmTests : TestsBase
	{
		[Test]
		public void Vm()
		{
			VmX(out var weakVm);
			
			GC.Collect();
			
			Assert.False(weakVm.TryGetTarget(out _));
		}

		private void VmX(out WeakReference<WrenVm> weakVm)
		{
			WrenVm vm = new WrenVm();
			weakVm = new WeakReference<WrenVm>(vm);
			Assert.True(vm.IsAlive);
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
			CallX(out var weakVm, out var weakHandle);
			
			GC.Collect();

			Assert.False(weakHandle.TryGetTarget(out _));
			Assert.False(weakVm.TryGetTarget(out _));
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
		}
	}
}
