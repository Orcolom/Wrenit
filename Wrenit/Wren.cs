using System;
using System.Runtime.InteropServices;
using Wrenit.Interop;

namespace Wrenit
{
	public static class Wren
	{
		#if DEBUG
		internal const string DllName = "wren_d.dll";
		#else
		internal const string DllName = "wren.dll";
		#endif
		
		public static readonly int[] WrenVersion = {0, 4, 0};
		public static readonly string WrenVersionString = $"{WrenVersion[0]}.{WrenVersion[1]}.{WrenVersion[2]}";
		public static readonly int WrenVersionNumber = WrenVersion[0] * 1000000 + WrenVersion[1] * 1000 + WrenVersion[2];

		private static bool _didInitializeCheck = false;

		internal static InteropWrenConfiguration DefaultConfig;

		/// <summary>
		/// initialize checks if we can communicate with the native code
		/// </summary>
		/// <returns>returns true if it was successfull</returns>
		/// <exception cref="NotSupportedException"></exception>
		public static bool Initialize()
		{
			if (_didInitializeCheck) return true;

			int version = WrenImport.wrenGetVersionNumber();

			int patch = version % 1000;
			int minor = ((version - patch) / 1000) % 1000;
			int major = ((version - (minor * 1000) - patch) / 1000000) % 1000;

			_didInitializeCheck = true;
			if (version != WrenVersionNumber) 
				throw new NotSupportedException(
					$"{DllName} with version {major}.{minor}.{patch} is not supported. Dll with version {WrenVersionString} needed");

			// load in defaults from unmanaged code, if not already done
			DefaultConfig = new InteropWrenConfiguration();
			WrenImport.wrenInitConfiguration(DefaultConfig);
			
			return true;
		}

		#region Bindings

		/// <inheritdoc cref="InteropWrenWrite"/>
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenWrite))]
		#endif
		internal static void OnWrenWrite(IntPtr vmPtr, string text)
		{
			var vm = WrenCache.GetVm(vmPtr);
			var list = vm?.Config.WriteHandler?.GetInvocationList();
			if (list == null) return;

			for (int i = 0; i < list.Length; i++)
			{
				WrenWrite write = list[i] as WrenWrite;
				write?.Invoke(vm, text);
			}
		}

		/// <inheritdoc cref="InteropWrenError"/>
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenError))]
		#endif
		internal static void OnWrenError(IntPtr vmPtr, WrenErrorType type, string module, int line, string message)
		{
			var vm = WrenCache.GetVm(vmPtr);
			var list = vm?.Config.ErrorHandler?.GetInvocationList();
			if (list == null) return;

			for (int i = 0; i < list.Length; i++)
			{
				WrenError error = list[i] as WrenError;
				error?.Invoke(vm, type, module, line, message);
			}
		}

		/// <inheritdoc cref="InteropWrenResolveModule"/>
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenResolveModule))]
		#endif
		internal static IntPtr OnWrenResolveModule(IntPtr vmPtr, string importer, IntPtr namePtr)
		{
			var vm = WrenCache.GetVm(vmPtr);
			var list = vm?.Config.ResolveModuleHandler?.GetInvocationList();
			if (list == null) return namePtr;

			string name = Marshal.PtrToStringAnsi(namePtr);
			string resolved = null;

			for (int i = 0; i < list.Length; i++)
			{
				WrenResolveModule resolve = list[i] as WrenResolveModule;
				resolved = resolve?.Invoke(vm, importer, name);
				if (string.IsNullOrEmpty(resolved) == false) break;
			}

			if (resolved == name || string.IsNullOrEmpty(resolved)) return namePtr;

			// the name needs to be given in wren's managed memory
			// 1. create an char* string
			IntPtr unmanagedName = Marshal.StringToHGlobalAnsi(resolved);
			UIntPtr size = new UIntPtr((uint)(resolved.Length + 1) * (uint)IntPtr.Size);

			// 2. create pointer using same allocator that wren uses 
			IntPtr ptr = DefaultConfig.ReallocateFn.Invoke(IntPtr.Zero, size, IntPtr.Zero);

			// 3. copy char* string over
			unsafe
			{
				Buffer.MemoryCopy(unmanagedName.ToPointer(), ptr.ToPointer(), size.ToUInt64(), size.ToUInt64());
			}

			Marshal.FreeHGlobal(unmanagedName);

			// 4. return wren managed pointer
			return ptr;
		}

		/// <inheritdoc cref="InteropWrenLoadModule"/>
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenLoadModule))]
		#endif
		internal static InteropWrenLoadModuleResult OnWrenLoadModule(IntPtr vmPtr, string name)
		{
			var vm = WrenCache.GetVm(vmPtr);
			if (vm == null) return new InteropWrenLoadModuleResult();
			
			var list = vm.Config.LoadModuleHandler?.GetInvocationList();
			string result = null;

			if (list != null)
			{
				for (int i = 0; i < list.Length; i++)
				{
					var load = list[i] as WrenLoadModule;

					result = load?.Invoke(vm, name);
					if (string.IsNullOrEmpty(result) == false) break;
				}
			}

			// return fake empty modules for not wanted modules
			if (name == "meta" && vm.Config.OptionalModules.HasFlag(WrenModules.ModuleMeta) == false) result = "// fake meta";
			if (name == "random" && vm.Config.OptionalModules.HasFlag(WrenModules.ModuleRandom) == false) result = "// fake random";

			if (string.IsNullOrEmpty(result)) return new InteropWrenLoadModuleResult();

			IntPtr ptr = Marshal.StringToCoTaskMemAnsi(result);
			return new InteropWrenLoadModuleResult()
			{
				Source = ptr,
				UserData = ptr,
				OnComplete = Marshal.GetFunctionPointerForDelegate<InteropWrenLoadModuleComplete>(OnWrenLoadComplete),
			};
		}

		/// <inheritdoc cref="InteropWrenLoadModuleComplete"/>
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenLoadModuleComplete))]
		#endif
		private static void OnWrenLoadComplete(IntPtr vm, string name, InteropWrenLoadModuleResult result)
		{
			Marshal.FreeHGlobal(result.UserData);
		}

		/// <inheritdoc cref="InteropWrenBindForeignMethod"/>
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenBindForeignClass))]
		#endif
		internal static InteropWrenForeignClassMethods OnWrenBindForeignClass(IntPtr vmPtr, string module, string className)
		{
			var vm = WrenCache.GetVm(vmPtr);
			var list = vm?.Config.BindForeignClassHandler?.GetInvocationList();
			if (list == null) return new InteropWrenForeignClassMethods();

			for (int i = 0; i < list.Length; i++)
			{
				WrenBindForeignClass foreign = list[i] as WrenBindForeignClass;
				WrenForeignClass classBinding = foreign?.Invoke(vm, module, className);
				if (classBinding?.Allocator == null) continue;

				IntPtr ptr = vm.Cache.GetNewForeignClassId(classBinding);

				return new InteropWrenForeignClassMethods()
				{
					AllocateFn = Marshal.GetFunctionPointerForDelegate<InteropWrenForeignMethod>(OnWrenCallForeignAllocator),
					AllocateUserData = ptr,
					FinalizeFn = Marshal.GetFunctionPointerForDelegate<InteropWrenForeignFinalizer>(OnWrenCallForeignFinalizer),
					FinalizeUserData = ptr,
				};
			}

			// wren defaults to aborting when no allocator is defined
			// to avoid sudden aborts we pass a dummy allocator, the following construct **will** fail
			// resulting in a safe WrenInterpretError.RuntimeError
			vm.Config.ErrorHandler?.Invoke(vm, WrenErrorType.WrenitRuntimeError, module, -1,
				$"Allocator for foreign {className} not defined in bindings");
			return new InteropWrenForeignClassMethods()
			{
				AllocateFn = Marshal.GetFunctionPointerForDelegate<InteropWrenForeignMethod>(OnWrenCallCatch),
			};
		}

		/// <inheritdoc cref="InteropWrenBindForeignMethod"/>
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenBindForeignMethod))]
		#endif
		internal static InteropBindForeignMethodResult OnWrenBindForeignMethod(IntPtr vmPtr, string module, string className, bool isStatic, string signature)
		{
			var vm = WrenCache.GetVm(vmPtr);
			var list = vm?.Config.BindForeignMethodHandler?.GetInvocationList();
			if (list == null) return new InteropBindForeignMethodResult();

			for (int i = 0; i < list.Length; i++)
			{
				WrenBindForeignMethod foreign = list[i] as WrenBindForeignMethod;
				WrenForeignMethod methodBinding = foreign?.Invoke(vm, module, className, isStatic, signature);
				if (methodBinding == null) continue;

				IntPtr id = vm.Cache.GetNewForeignMethodId(methodBinding);
				return new InteropBindForeignMethodResult()
				{
					ExecuteFn = Marshal.GetFunctionPointerForDelegate<InteropWrenForeignMethod>(OnWrenCallForeign),
					UserData = id,
				};
			}

			return new InteropBindForeignMethodResult();
		}

		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenForeignMethod))]
		#endif
		private static void OnWrenCallCatch(IntPtr ptr, IntPtr userData) { }

		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenForeignMethod))]
		#endif
		private static void OnWrenCallForeign(IntPtr ptr, IntPtr userData)
		{
			WrenVm vm = WrenCache.GetVm(ptr);
			vm?.Cache.GetForeignMethodById(userData).Invoke(vm);
		}
		
		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenForeignMethod))]
		#endif
		private static void OnWrenCallForeignAllocator(IntPtr ptr, IntPtr userData)
		{
			WrenVm vm = WrenCache.GetVm(ptr);
			vm?.Cache.GetForeignClassById(userData)?.Allocator?.Invoke(vm);
		}

		#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(InteropWrenForeignFinalizer))]
		#endif
		private static void OnWrenCallForeignFinalizer(IntPtr data, IntPtr userData)
		{
			IntPtr vmPtr = Marshal.ReadIntPtr(data);
			WrenVm vm = WrenCache.GetVm(vmPtr);
			WrenForeignObject obj = vm?.Cache.GetForeignById(data);
			if (obj == null) return;
			vm.Cache.GetForeignClassById(data)?.Finalizer?.Invoke(obj);
		}

		#endregion

	}

	#region Delagates

	/// <summary>
	/// Method that will be called from wren 
	/// </summary>
	public delegate void WrenForeignMethod(WrenVm vm);

	/// <inheritdoc cref="InteropWrenForeignClassMethods.FinalizeFn"/>
	public delegate void WrenForeignFinalizer(WrenForeignObject data);

	/// <inheritdoc cref="InteropWrenWrite"/>
	public delegate void WrenWrite(WrenVm vm, string text);

	/// <inheritdoc cref="InteropWrenError"/>
	public delegate void WrenError(WrenVm vm, WrenErrorType result, string module, int line, string message);

	/// <inheritdoc cref="InteropWrenResolveModule"/>
	public delegate string WrenResolveModule(WrenVm vm, string importer, string name);

	/// <inheritdoc cref="InteropWrenLoadModule"/>
	public delegate string WrenLoadModule(WrenVm vm, string name);

	/// <inheritdoc cref="InteropWrenBindForeignMethod"/>
	public delegate WrenForeignMethod WrenBindForeignMethod(WrenVm vm, string module, string className,
		bool isStatic, string signature);

	/// <inheritdoc cref="InteropWrenBindForeignClass"/>
	public delegate WrenForeignClass WrenBindForeignClass(WrenVm vm, string module, string className);

	#endregion

	#region Bindings

	/// <summary>
	/// defines how to create and finalize a foreign class
	/// </summary>
	public class WrenForeignClass
	{
		/// <summary>
		/// a foreign method binding for the allocator
		/// </summary>
		public readonly WrenForeignMethod Allocator;
	
		/// <summary>
		/// a foreign method binding for the finalizer
		/// </summary>
		public readonly WrenForeignFinalizer Finalizer;
	
		/// <summary>
		/// create a class binding with only an allocator
		/// </summary>
		public WrenForeignClass(WrenForeignMethod allocator)
		{
			Allocator = allocator;
			Finalizer = null;
		}
	
		/// <summary>
		/// create a class binding
		/// </summary>
		public WrenForeignClass(WrenForeignMethod allocator, WrenForeignFinalizer finalizer)
		{
			Allocator = allocator;
			Finalizer = finalizer;
		}
	}

	#endregion

	#region Enums

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

		#endregion
	}
}
