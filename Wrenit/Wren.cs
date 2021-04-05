using System;
using System.Runtime.InteropServices;
using Wren.it.Interlop;

namespace Wren.it
{
	/// <summary>
	/// Method that will be called from wren 
	/// </summary>
	public delegate void WrenForeignMethod(WrenVm vm);

	/// <inheritdoc cref="InterlopWrenForeignClassMethods.FinalizeFn"/>
	public delegate void WrenFinalizer(IntPtr data);

	/// <inheritdoc cref="WrenWriteFn"/>
	public delegate void WrenWrite(WrenVm vm, string text);

	/// <inheritdoc cref="WrenErrorFn"/>
	public delegate void WrenError(WrenVm vm, WrenErrorType result, string module, int line, string message);

	/// <inheritdoc cref="WrenResolveModuleFn"/>
	public delegate string WrenResolveModule(WrenVm vm, string importer, string name);

	/// <inheritdoc cref="WrenLoadModuleFn"/>
	public delegate string WrenLoadModule(WrenVm vm, string name);

	/// <inheritdoc cref="WrenBindForeignMethodFn"/>
	public delegate WrenForeignMethodBinding WrenBindForeignMethod(WrenVm vm, string module, string className,
		bool isStatic, string signature);

	/// <inheritdoc cref="WrenBindForeignClassFn"/>
	public delegate WrenForeignClass WrenBindForeignClass(WrenVm vm, string module, string className);

	public class WrenForeignClass
	{
		public readonly WrenForeignMethodBinding Allocator;
		public readonly WrenFinalizerMethodBinding Finalizer;

		public WrenForeignClass(WrenForeignMethodBinding allocator)
		{
			Allocator = allocator;
			Finalizer = new WrenFinalizerMethodBinding(null);
		}
		
		public WrenForeignClass(WrenForeignMethodBinding allocator, WrenFinalizerMethodBinding finalizer)
		{
			Allocator = allocator;
			Finalizer = finalizer;
		}
	}

	/// <summary>
	/// binding of c# finalizer delegate to a native finalizer function 
	/// </summary>
	public class WrenFinalizerMethodBinding
	{
		public readonly IntPtr MethodPtr;
		private readonly WrenFinalizer _method;

		public WrenFinalizerMethodBinding(WrenFinalizer method)
		{
			_method = method;
			MethodPtr = Marshal.GetFunctionPointerForDelegate<WrenFinalizer>(OnFinalize);
		}

		private void OnFinalize(IntPtr data)
		{
			IntPtr vm = Marshal.ReadIntPtr(data);
			IntPtr id = Marshal.ReadIntPtr(data, IntPtr.Size);
			WrenVm.GetVm(vm)?.FreeForeignObject(id);
			_method?.Invoke(id);
		}
	}

	/// <summary>
	/// binding of a c# delegate to a native function 
	/// </summary>
	public class WrenForeignMethodBinding
	{
		public readonly IntPtr MethodPtr;
		private readonly WrenForeignMethod _method;

		public WrenForeignMethodBinding(WrenForeignMethod method)
		{
			_method = method;
			MethodPtr = Marshal.GetFunctionPointerForDelegate<InterlopWrenForeignMethodFn>(OnWrenCall);
		}

		private void OnWrenCall(IntPtr ptr)
		{
			WrenVm vm = WrenVm.GetVm(ptr);
			if (vm != null)
			{
				_method?.Invoke(vm);
			}
		}
	}

	/// <summary>
	/// error message type
	/// </summary>
	public enum WrenErrorType
	{
		CompileError,
		RuntimeError,
		StackTrace,
		WrenitRuntimeError,
	}

	/// <summary>
	/// result of interpreting source code
	/// </summary>
	public enum WrenInterpretResult
	{
		Success,
		CompileError,
		RuntimeError,
	}

	/// <summary>
	/// types a WrenValue can have
	/// </summary>
	public enum WrenValueType
	{
		Bool = 0,
		Number = 1,
		Foreign = 2,
		List = 3,
		Map = 4,
		Null = 5,
		String = 6,

		// The object is of a type that isn't accessible by the C API.
		Unknown = 7,
	}
}
