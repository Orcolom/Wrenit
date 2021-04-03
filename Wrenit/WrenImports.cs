
using System;
using System.Runtime.InteropServices;

namespace Wrenit
{
	internal static class WrenImport
	{

#if DEBUG
		private const string wrenDll = "wren_d.dll";
#else
		private const string wrenDll  = "wren.dll";
#endif

		#region Imports
		
		[DllImport(wrenDll)]
		internal extern static void xWrenInitConfiguration([Out] WrenConfig config);

		[DllImport(wrenDll)]
		internal extern static IntPtr xWrenNewVM(WrenConfig config);

		[DllImport(wrenDll)]
		internal extern static IntPtr xWrenFreeVM(IntPtr vm);

		[DllImport(wrenDll)]
		internal extern static IntPtr xWrenCollectGarbage(IntPtr vm);

		[DllImport(wrenDll)]
		internal extern static IntPtr xWrenReallocate(IntPtr vm, IntPtr memory, UIntPtr oldSize, UIntPtr newSize);

		[DllImport(wrenDll)]
		internal extern static WrenInterpretResult xWrenInterpret(IntPtr vm, string module, string source);

		#endregion

	}

	#region Types/Delegates

	internal delegate IntPtr WrenReallocateFn(IntPtr memory, UIntPtr size, IntPtr userData);

	internal delegate void WrenWriteFn(IntPtr vm,
	[MarshalAs(UnmanagedType.LPStr)] string text);

	internal delegate void WrenErrorFn(IntPtr vm,
		WrenErrorType type,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		int line,
		[MarshalAs(UnmanagedType.LPStr)] string message);

	internal delegate IntPtr WrenResolveModuleFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string importer,
		IntPtr name);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate LoadModuleResult WrenLoadModuleFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string name);

	internal delegate void WrenLoadModuleCompleteFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string name,
		LoadModuleResult result);

	[StructLayout(LayoutKind.Sequential)]
	internal class LoadModuleResult
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public string source;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenLoadModuleCompleteFn onComplete;

		public IntPtr userData;
	}

	internal enum WrenErrorType
	{
		WREN_ERROR_COMPILE,
		WREN_ERROR_RUNTIME,
		WREN_ERROR_STACK_TRACE
	}

	internal enum WrenInterpretResult
	{
		SUCCESS,
		WREN_ERROR_COMPILE,
		WREN_ERROR_RUNTIME,
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class WrenConfig
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
	#endregion

}
