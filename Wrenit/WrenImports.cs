
using System;
using System.Runtime.InteropServices;

namespace Wrenit
{
	public static class WrenImport
	{

#if DEBUG
		private const string dll = "wren_d.dll";
#else
		private const string dll = "wren.dll";
#endif

		[DllImport(dll)]
		internal extern static void xWrenInitConfiguration([Out] WrenConfig config);

		[DllImport(dll)]
		internal extern static IntPtr xWrenNewVM(WrenConfig config);

		[DllImport(dll)]
		internal extern static IntPtr xWrenFreeVM(IntPtr vm);

		[DllImport(dll)]
		internal extern static IntPtr xWrenCollectGarbage(IntPtr vm);

		[DllImport(dll)]
		internal extern static WrenInterpretResult xWrenInterpret(IntPtr vm, string module, string source);
	}
}
