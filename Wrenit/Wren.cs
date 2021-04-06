using System;
using System.Runtime.InteropServices;
using Wrenit.Interlop;
using Wrenit.Utilities;

namespace Wrenit
{
	public static class Wren
	{
		#if DEBUG
		internal const string DllName = "wren_d.dll";
		#else
		internal const string DllName = "wren.dll";
		#endif

		private static readonly int[] WrenVersion = {0, 4, 0};
		private static readonly string WrenVersionString = $"{WrenVersion[0]}.{WrenVersion[1]}.{WrenVersion[2]}";
		private static readonly int WrenVersionNumber = WrenVersion[0] * 1000000 + WrenVersion[1] * 1000 + WrenVersion[2];

		private static bool _didInitializeCheck = false;
		
		public static bool Initialize()
		{
			if (_didInitializeCheck) return true;

			int version = WrenImport.wrenGetVersionNumber();

			int patch = version % 1000;
			int minor = ((version - patch) / 1000) % 1000;
			int major = ((version - (minor * 1000) - patch) / 1000000) % 1000;

			_didInitializeCheck = true;
			if (version == WrenVersionNumber) return true;

			throw new NotSupportedException(
				$"{DllName} with version {major}.{minor}.{patch} is not supported. Dll with version {WrenVersionString} needed");
		}

		public static string CreateSignature(MethodType type, string name, int argumentCount)
		{
			argumentCount = CorrectArgumentCount(type, argumentCount);
			name = CorrectName(type, name);
			string arguments = CreateArgumentList(argumentCount);
			
			switch (type)
			{
				case MethodType.Method:
				case MethodType.MethodStatic:
					return $"{name}({arguments})";
				case MethodType.MethodConstruct:
					return $"init {name}({arguments})";
				
				case MethodType.FieldGetter:
					return $"{name}";
				case MethodType.FieldSetter:
					return $"{name}=({arguments})";
				case MethodType.SubScriptGetter:
					return $"[{arguments}]";
				case MethodType.SubScriptSetter:
					return $"[{arguments}]=({arguments})";
				
				case MethodType.OperatorPrefixMinus:
				case MethodType.OperatorPrefixNot:
				case MethodType.OperatorPrefixTilda:
					return name;
			}

			return null;
		}

		public static string CorrectName(MethodType type, string name)
		{
			switch (type)
			{
				default: return name;
				
				case MethodType.OperatorPrefixMinus:
					return "-";
				case MethodType.OperatorPrefixNot:
					return "!";
				case MethodType.OperatorPrefixTilda:
					return "~";
			}
		}

		internal static string CreateArgumentList(int argumentCount)
		{
			string arguments = null;
			for (int i = 0; i < argumentCount; i++)
			{
				if (i + 1 < argumentCount) arguments += "_,";
				else arguments += "_";
			}

			return arguments;
		}

		public static int CorrectArgumentCount(MethodType type, int argumentCount)
		{
			switch (type)
			{
				case MethodType.SubScriptGetter:
				case MethodType.SubScriptSetter:
				case MethodType.MethodStatic:
				case MethodType.MethodConstruct:
				case MethodType.Method: 
					return argumentCount;

				case MethodType.OperatorPrefixMinus:
				case MethodType.OperatorPrefixNot:
				case MethodType.OperatorPrefixTilda:
				case MethodType.FieldGetter: 
					return 0;
				
				default: 
					return 1;
			}
		}
	}
		
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
