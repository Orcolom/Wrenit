using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Wrenit
{
	/// <summary>
	/// class containing all safety checks
	/// </summary>
	internal static class WrenAssert
	{
		[Conditional("DEBUG")]
		internal static void AssertAlive(this WrenVm vm)
		{
			if (vm.IsAlive ==false)
				throw new ObjectDisposedException("Tried to Interpret module in a disposed VM");
		}
		
		[Conditional("DEBUG")]
		internal static void AssertAlive(this WrenHandle handle)
		{
			if (handle.IsAlive ==false)
				throw new ObjectDisposedException("Tried to use disposed handle");
		}
		
		[Conditional("DEBUG")]
		internal static void AssertAlive(this WrenForeignObject foreign)
		{
			if (foreign.IsAlive == false)
				throw new ObjectDisposedException("Tried to use disposed foreign");
		}
		
		[Conditional("DEBUG")]
		internal static void AssertSlotType(this WrenVm vm, int slot, WrenValueType type)
		{
			vm.AssertAlive();
			var actualType = vm.GetSlotType(slot);
			if (actualType != type) throw new TypeAccessException($"slot {slot} is of type {actualType} not of type {type}");
		}
		
		[Conditional("DEBUG")]
		internal static void AssertSlotCount(this WrenVm vm, int slot)
		{
			vm.AssertAlive();
			if (slot < 0) throw new IndexOutOfRangeException("slots cant be below 0");
			int actualCount = vm.GetSlotCount();
			if (actualCount <= slot) throw new IndexOutOfRangeException($"slot index {slot} is out of slot count {actualCount}");
		}
		
		[Conditional("DEBUG")]
		internal static void AssertSlot(this WrenVm vm, int slot, WrenValueType type)
		{
			vm.AssertAlive();
			vm.AssertSlotCount(slot);
			vm.AssertSlotType(slot, type);
		}
		
		[Conditional("DEBUG")]
		internal static void AssertSlot(this WrenVm vm, int slot)
		{
			vm.AssertAlive();
			vm.AssertSlotCount(slot);
		}
		
		[Conditional("DEBUG")]
		internal static void AssertModule(this WrenVm vm, string module)
		{
			vm.AssertAlive();
			if (string.IsNullOrEmpty(module)) throw new ArgumentException("module name cant be null or empty");
			if (vm.HasModule(module) == false) throw new AccessViolationException($"Module '{module}' does not exist. This should be checked");
		}
		
		[Conditional("DEBUG")]
		internal static void AssertVariable(this WrenVm vm, string module, string name)
		{
			vm.AssertAlive();
			vm.AssertModule(module);
			if (string.IsNullOrEmpty(module)) throw new ArgumentException("module name cant be null or empty");
			if (vm.HasVariable(module, name) == false) throw new AccessViolationException($"Variable '{name}' does not exist. This should be checked");
		}
	}
}
