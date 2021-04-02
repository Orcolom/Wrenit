using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{

	internal delegate void WrenWriteFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string text);

	internal delegate void WrenErrorFn(IntPtr vm,
		WrenErrorType type,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		int line,
		[MarshalAs(UnmanagedType.LPStr)] string message);

	internal delegate IntPtr WrenReallocateFn(IntPtr memory, UIntPtr size, IntPtr userData);

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
}
