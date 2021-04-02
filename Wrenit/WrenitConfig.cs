using System;
using System.Runtime.InteropServices;

namespace Wrenit
{
	public struct WrenitConfig
	{
		#region Fields

		internal WrenReallocateFn ReallocateFn;

		public WrenResolveModuleFn ResolveModuleFn;

		public WrenLoadModuleFn LoadModuleFn;

		public WrenBindForeignMethodFn bindForeignMethodFn;

		public WrenBindForeignClassFn bindForeignClassFn;

		public ulong initialHeapSize;

		public ulong minHeapSize;

		public int heapGrowthPercent;

		#endregion

		public static WrenitConfig CreateInitialized()
		{
			WrenConfig wrenConfig = new WrenConfig();
			WrenImport.xWrenInitConfiguration(wrenConfig);
			return ToWrenit(wrenConfig);
		}

		internal static WrenConfig ToWren(WrenitConfig wrenitConfig)
		{
			return new WrenConfig()
			{
				bindForeignClassFn = wrenitConfig.bindForeignClassFn,
				bindForeignMethodFn = wrenitConfig.bindForeignMethodFn,
				heapGrowthPercent = wrenitConfig.heapGrowthPercent,
				initialHeapSize = new UIntPtr(wrenitConfig.initialHeapSize),
				LoadModuleFn = wrenitConfig.LoadModuleFn,
				minHeapSize = new UIntPtr(wrenitConfig.minHeapSize),
				ReallocateFn = wrenitConfig.ReallocateFn,
				ResolveModuleFn = wrenitConfig.ResolveModuleFn,
				WrenErrorFn = null,
				WrenWriteFn = null,
				userData = IntPtr.Zero,
			};
		}

		internal static WrenitConfig ToWrenit(WrenConfig wrenConfig)
		{
			return new WrenitConfig()
			{
				bindForeignClassFn = wrenConfig.bindForeignClassFn,
				bindForeignMethodFn = wrenConfig.bindForeignMethodFn,
				heapGrowthPercent = wrenConfig.heapGrowthPercent,
				initialHeapSize = wrenConfig.initialHeapSize.ToUInt64(),
				LoadModuleFn = wrenConfig.LoadModuleFn,
				minHeapSize = wrenConfig.minHeapSize.ToUInt64(),
				ReallocateFn = wrenConfig.ReallocateFn,
				ResolveModuleFn = wrenConfig.ResolveModuleFn,
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class WrenConfig
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenReallocateFn ReallocateFn;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenResolveModuleFn ResolveModuleFn;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenLoadModuleFn LoadModuleFn;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenBindForeignMethodFn bindForeignMethodFn;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenBindForeignClassFn bindForeignClassFn;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenWriteFn WrenWriteFn;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenErrorFn WrenErrorFn;

		public UIntPtr initialHeapSize;

		public UIntPtr minHeapSize;

		public int heapGrowthPercent;

		public IntPtr userData;
	}
}
