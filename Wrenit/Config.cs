using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{
	public delegate IntPtr WrenReallocateFn(IntPtr memory, UIntPtr size, IntPtr userData);

	public delegate IntPtr WrenResolveModuleFn(IntPtr vm, 
		[MarshalAs(UnmanagedType.LPStr)]string importer, 
		[MarshalAs(UnmanagedType.LPStr)] string name);

	[return: MarshalAs(UnmanagedType.LPWStr)]
	public delegate string WrenLoadModuleFn(IntPtr vm, 
		[MarshalAs(UnmanagedType.LPStr)] string name);

	public delegate void WrenLoadModuleCompleteFn(IntPtr vm, 
		[MarshalAs(UnmanagedType.LPStr)] string name, 
		LoadModuleResult result);


	[return: MarshalAs(UnmanagedType.FunctionPtr)]
	public delegate WrenForeignMethodFn WrenBindForeignMethodFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		[MarshalAs(UnmanagedType.LPStr)] string className,
		[MarshalAs(UnmanagedType.I1)] bool isStatic,
		[MarshalAs(UnmanagedType.LPStr)] string signature,
		LoadModuleResult result);

	public delegate void WrenForeignMethodFn(IntPtr vm);
	public delegate void WrenFinalizerFn(IntPtr data);

	public delegate WrenForeignClassMethods WrenBindForeignClassFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		[MarshalAs(UnmanagedType.LPStr)] string className);


	public delegate void WrenWriteFn(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string text);

	public delegate void WrenErrorFn(IntPtr vm,
		WrenErrorType type,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		int line,
		[MarshalAs(UnmanagedType.LPStr)] string message);

	[StructLayout(LayoutKind.Sequential)]

	public class InternalConfig
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

	[StructLayout(LayoutKind.Sequential)]
	public struct LoadModuleResult
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public string source;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WrenLoadModuleCompleteFn onComplete;

		public IntPtr userData;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct WrenForeignClassMethods
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		WrenForeignMethodFn allocate;
		
		[MarshalAs(UnmanagedType.FunctionPtr)]
		WrenFinalizerFn finalize;
	}
}
