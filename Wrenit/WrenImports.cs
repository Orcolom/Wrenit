
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
		internal extern static void wrenInitConfiguration([Out] WrenConfig config);

		[DllImport(wrenDll)]
		internal extern static IntPtr wrenNewVM(WrenConfig config);

		[DllImport(wrenDll)]
		internal extern static IntPtr wrenFreeVM(IntPtr vm);

		[DllImport(wrenDll)]
		internal extern static IntPtr wrenCollectGarbage(IntPtr vm);

		[DllImport(wrenDll)]
		internal extern static IntPtr wrenReallocate(IntPtr vm, IntPtr memory, UIntPtr oldSize, UIntPtr newSize);

		[DllImport(wrenDll)]
		internal extern static WrenInterpretResult wrenInterpret(IntPtr vm, string module, string source);

		[DllImport(wrenDll)]
		internal extern static WrenValueType wrenGetSlotType(IntPtr vm, int slot);
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


	internal delegate IntPtr WrenBindForeignMethodFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		[MarshalAs(UnmanagedType.LPStr)] string className,
		[MarshalAs(UnmanagedType.I1)] bool isStatic,
		[MarshalAs(UnmanagedType.LPStr)] string signature);

	internal delegate void WrenForeignMethodFn(IntPtr vm);
	
	public delegate void WrenFinalizerFn(IntPtr data);

	internal delegate WrenForeignClassMethods WrenBindForeignClassFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		[MarshalAs(UnmanagedType.LPStr)] string className);

	internal struct WrenForeignClassMethods
	{
		public IntPtr allocate;

		public IntPtr finalize;
	}

	internal struct LoadModuleResult
	{
		public IntPtr source;

		public IntPtr onComplete;

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

	internal enum WrenValueType
	{
		WREN_TYPE_BOOL,
		WREN_TYPE_NUM,
		WREN_TYPE_FOREIGN,
		WREN_TYPE_LIST,
		WREN_TYPE_MAP,
		WREN_TYPE_NULL,
		WREN_TYPE_STRING,

		// The object is of a type that isn't accessible by the C API.
		WREN_TYPE_UNKNOWN
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
