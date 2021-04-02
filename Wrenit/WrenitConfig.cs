using System;
using System.Runtime.InteropServices;

namespace Wrenit
{
	public class WrenitConfig
	{
		private static WrenitConfig _default;

		public ulong InitialHeapSize;

		public ulong MinHeapSize;

		public int HeapGrowthPercent;

		internal WrenReallocateFn ReallocateFn;

		private WrenitConfig() { }

		public static WrenitConfig GetDefaults()
		{
			if (_default != null) return _default;
			WrenConfig wrenConfig = new WrenConfig();
			WrenImport.xWrenInitConfiguration(wrenConfig);

			_default = new WrenitConfig()
			{
				HeapGrowthPercent = wrenConfig.heapGrowthPercent,
				InitialHeapSize = wrenConfig.initialHeapSize.ToUInt64(),
				MinHeapSize = wrenConfig.initialHeapSize.ToUInt64(),
			};
			_default.ReallocateFn = wrenConfig.ReallocateFn;

			return _default;
		}
	}
}
