using System.Globalization;
using Wrenit.Utilities;

namespace Wrenit
{
	public static class WrenExtensions
	{
		public static void GetVariable<TModule, TName>(this WrenVm vm, int slot)
		{
			vm.GetVariable(WrenBuilder.GetResolvedName<TModule>(), WrenBuilder.GetResolvedName<TName>(), slot);
		}	

		public static bool HasVariable<TModule, TName>(this WrenVm vm)
		{
			return vm.HasVariable(WrenBuilder.GetResolvedName<TModule>(), WrenBuilder.GetResolvedName<TName>());
		}	

		public static bool HasModule<TModule>(this WrenVm vm)
		{
			return vm.HasModule(WrenBuilder.GetResolvedName<TModule>());
		}	
		
		public static void AbortFiber(this WrenVm vm, string message)
		{
			vm.EnsureSlots(1);
			vm.SetSlotString(0, message);
			vm.AbortFiber(0);
		}	
		
		public static WrenForeignObject<T> GetSlotForeign<T>(this WrenVm vm, int slot)
		{
			return vm.GetSlotForeign(slot).As<T>();
		}
		
		public static void SetSlotInt(this WrenVm vm, int slot, int value)
		{
			vm.SetSlotDouble(slot, value);
		}
		
		public static int GetSlotInt(this WrenVm vm, int slot)
		{
			return (int)vm.GetSlotDouble(slot);
		}

		public static void BindModule<T>(this WrenConfig config)
		{
			WrenBuilder.GetModule<T>().Bind(config);
		}
		
		public static void UnBindModule<T>(this WrenConfig config)
		{
			WrenBuilder.GetModule<T>().UnBind(config);
		}

		public static string ToWrenSafeString(this double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		
		public static string ToWrenSafeString(this float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
	}
}
