
using System;
using System.Runtime.InteropServices;

namespace Wrenit
{
	public static class WrenImport
	{

#if DEBUG
		private const string wrenDll = "wren_d.dll";
#else
		private const string wrenDll  = "wren.dll";
#endif

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
	}
}
