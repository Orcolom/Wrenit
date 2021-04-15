using Wrenit.Utilities;

namespace Wrenit
{
	public static class WrenExtensions
	{
		public static void GetVariable<TModule, TName>(this WrenVm vm, int slot)
		{
			vm.GetVariable(WrenBuilder.GetName<TModule>(), WrenBuilder.GetName<TName>(), slot);
		}	

		public static bool HasVariable<TModule, TName>(this WrenVm vm)
		{
			return vm.HasVariable(WrenBuilder.GetName<TModule>(), WrenBuilder.GetName<TName>());
		}	

		public static bool HasModule<TModule>(this WrenVm vm)
		{
			return vm.HasModule(WrenBuilder.GetName<TModule>());
		}	
		
		public static void AbortFiber(this WrenVm vm, string message)
		{
			vm.EnsureSlots(1);
			vm.SetSlotString(0, message);
			vm.AbortFiber(0);
		}	
		
		public static WrenForeignObject<T> SetSlotNewForeign<T>(this WrenVm vm, int slot, int classSlot)
		{
			return vm.SetSlotNewForeign(slot, classSlot).As<T>();
		}

		public static WrenForeignObject<T> GetSlotForeign<T>(this WrenVm vm, int slot)
		{
			return vm.GetSlotForeign(slot).As<T>();
		}
	}
}
