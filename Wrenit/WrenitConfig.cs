using System;
using System.Runtime.InteropServices;

namespace Wrenit
{
	public struct WrenitConfig
	{
		// TODO: static nullables broke unit tests?
		private static WrenitConfig _default;
		private static bool _hasDefaults;

		public ulong InitialHeapSize;

		public ulong MinHeapSize;

		public int HeapGrowthPercent;

		public WrenitWrite WriteHandler;
		
		public WrenitError ErrorHandler;
		
		public WrenitResolveModule ResolveModuleHandler;
		
		public WrenitLoadModule LoadModuleHandler;

		public WrenitBindForeignMethod BindForeignMethod;

		public WrenitBindForeignClass BindForeignClass;

		public static WrenitConfig GetDefaults()
		{
			if (_hasDefaults) return _default;
			WrenConfig wrenConfig = new WrenConfig();
			WrenImport.wrenInitConfiguration(wrenConfig);

			_hasDefaults = true;
			_default = new WrenitConfig()
			{
				HeapGrowthPercent = wrenConfig.heapGrowthPercent,
				InitialHeapSize = wrenConfig.initialHeapSize.ToUInt64(),
				MinHeapSize = wrenConfig.initialHeapSize.ToUInt64(),
			};

			return _default;
		}
	}
}
