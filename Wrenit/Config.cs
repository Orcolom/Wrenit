using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{





	[return: MarshalAs(UnmanagedType.FunctionPtr)]
	internal delegate WrenForeignMethodFn WrenBindForeignMethodFn(IntPtr vm,
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


	
	[StructLayout(LayoutKind.Sequential)]
	public struct WrenForeignClassMethods
	{
		[MarshalAs(UnmanagedType.FunctionPtr)]
		WrenForeignMethodFn allocate;
		
		[MarshalAs(UnmanagedType.FunctionPtr)]
		WrenFinalizerFn finalize;
	}
}
