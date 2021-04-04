using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{
	public delegate void WrenitWrite(WrenitVM vm, string text);

	public delegate void WrenitError(WrenitVM vm, WrenitResult result, string module, int line, string message);

	public delegate string WrenitResolveModule(WrenitVM vm, string importer, string name);

	public delegate WrenitLoadModuleResult WrenitLoadModule(WrenitVM vm, string name);

	public delegate WrenitFunction WrenitBindForeignMethod(WrenitVM vm, string module, string className, bool isStatic, string signature);
	
	public delegate WrenitClass WrenitBindForeignClass(WrenitVM vm, string module, string className);

	public enum WrenitResult
	{
		Unknown,
		Success,
		CompileError,
		RuntimeError,
		StackTrace,
		Disposed,
	}

	public enum WrenitValueType
	{
		Bool,
		Number,
		Foreign,
		List,
		Map,
		Null,
		String,
		Unknown,
	}

	public class WrenitLoadModuleResult
	{
		public string Source;
	}

	public struct WrenitForeignClass
	{
		public WrenitFunction Allocator;
		public WrenitFinalizerFunction Finalizer;
	}

	public class WrenitVM : IDisposable
	{
		private static Dictionary<IntPtr, WeakReference<WrenitVM>> _vms = new Dictionary<IntPtr, WeakReference<WrenitVM>>();

		private IntPtr _vm;

		private WrenitConfig _config;

		public bool IsAlive => _vm != IntPtr.Zero;

		#region Lifetime

		public WrenitVM()
		{
			_vm = WrenImport.wrenNewVM(null);
			_vms.Add(_vm, new WeakReference<WrenitVM>(this));
		}

		public WrenitVM(WrenitConfig wrenitConfig)
		{
			_config = wrenitConfig;

			WrenConfig wrenConfig = null;
			wrenConfig = new WrenConfig
			{
				initialHeapSize = new UIntPtr(wrenitConfig.InitialHeapSize),
				minHeapSize = new UIntPtr(wrenitConfig.MinHeapSize),
				heapGrowthPercent = wrenitConfig.HeapGrowthPercent
			};

			// make native "bindings" for wanted functions
			if (wrenitConfig.WriteHandler != null) wrenConfig.WrenWriteFn = OnWrenWrite;
			if (wrenitConfig.ErrorHandler != null) wrenConfig.WrenErrorFn = OnWrenError;
			if (wrenitConfig.ResolveModuleHandler != null) wrenConfig.ResolveModuleFn = OnWrenResolveModuleName;
			if (wrenitConfig.LoadModuleHandler != null) wrenConfig.LoadModuleFn = onWrenLoadModule;
			if (wrenitConfig.BindForeignMethod != null) wrenConfig.bindForeignMethodFn = onWrenBindForeignMethod;
			if (wrenitConfig.BindForeignClass != null) wrenConfig.bindForeignClassFn= onWrenBindForeignClass;

			_vm = WrenImport.wrenNewVM(wrenConfig);
			_vms.Add(_vm, new WeakReference<WrenitVM>(this));
		}

		~WrenitVM()
		{
			_vms.Remove(_vm);

			WrenImport.wrenFreeVM(_vm);
			_vm = IntPtr.Zero;
		}

		public void Dispose()
		{
			if (IsAlive == false) return;
			
			_vms.Remove(_vm);
			
			WrenImport.wrenFreeVM(_vm);
			_vm = IntPtr.Zero;

			GC.SuppressFinalize(this);
		}

		internal static WrenitVM GetVM(IntPtr ptr)
		{
			if (_vms.TryGetValue(ptr, out WeakReference<WrenitVM> weakRef))
			{
				if (weakRef.TryGetTarget(out WrenitVM vm))
				{
					return vm;
				}
				return null;
			}
			return null;
		}

		#endregion

		public WrenitResult Interpret(string module, string source)
		{
			if (IsAlive == false)
				throw new ObjectDisposedException("Tried to Interpret module in disposed VM");

			WrenInterpretResult error = WrenImport.wrenInterpret(_vm, module, source);
			return ProcessEnum(error);
		}

		private WrenitResult ProcessEnum(WrenInterpretResult error)
		{
			switch (error)
			{
				default:
					return WrenitResult.Unknown;

				case WrenInterpretResult.SUCCESS:
					return WrenitResult.Success;

				case WrenInterpretResult.WREN_ERROR_COMPILE:
					return WrenitResult.CompileError;

				case WrenInterpretResult.WREN_ERROR_RUNTIME:
					return WrenitResult.RuntimeError;
			}
		}

		private WrenitResult ProcessEnum(WrenErrorType error)
		{
			switch (error)
			{
				default:
					return WrenitResult.Unknown;

				case WrenErrorType.WREN_ERROR_COMPILE:
					return WrenitResult.CompileError;

				case WrenErrorType.WREN_ERROR_RUNTIME:
					return WrenitResult.RuntimeError;

				case WrenErrorType.WREN_ERROR_STACK_TRACE:
					return WrenitResult.StackTrace;
			}
		}

		private WrenitValueType ProcessEnum(WrenValueType type)
		{
			int typeAsInt = (int)type;
			if (typeAsInt > (int)WrenitValueType.Unknown) typeAsInt = (int)WrenitValueType.Unknown;
			return (WrenitValueType)typeAsInt;
		}

		#region Bindings

		private void OnWrenWrite(IntPtr vm, string text)
		{
			var list = _config.WriteHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenitWrite write = list[i] as WrenitWrite;
				write.Invoke(this, text);
			}
		}

		private void OnWrenError(IntPtr vm, WrenErrorType type, string module, int line, string message)
		{
			var list = _config.ErrorHandler.GetInvocationList();
			WrenitResult result = ProcessEnum(type);
			for (int i = 0; i < list.Length; i++)
			{
				WrenitError error = list[i] as WrenitError;
				error.Invoke(this, result, module, line, message);
			}
		}
		
		private IntPtr OnWrenResolveModuleName(IntPtr vm, string importer, IntPtr namePtr)
		{
			string name = Marshal.PtrToStringAnsi(namePtr);
			string resolved = null;

			var list = _config.ResolveModuleHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenitResolveModule resolve = list[i] as WrenitResolveModule;
				resolved = resolve.Invoke(this, importer, name);
				if (string.IsNullOrEmpty(resolved) == false) break;
			}

			if (resolved == name || string.IsNullOrEmpty(resolved)) return namePtr;

			// the name needs to be given in wren's managed memory
			// 1. create an char* string
			IntPtr unmanagedName = Marshal.StringToHGlobalAnsi(resolved);
			UIntPtr size = new UIntPtr((uint)(resolved.Length + 1) * (uint)IntPtr.Size);

			// 2. create pointer in wren managed memory 
			IntPtr ptr = WrenImport.wrenReallocate(_vm, IntPtr.Zero, UIntPtr.Zero, size);

			// 3. copy char* string over
			unsafe
			{
				Buffer.MemoryCopy(unmanagedName.ToPointer(), ptr.ToPointer(), size.ToUInt64(), size.ToUInt64());
			}

			Marshal.FreeHGlobal(unmanagedName);


			// 4. return wren managed pointer
			return ptr;
		}

		private LoadModuleResult onWrenLoadModule(IntPtr vm, string name)
		{
			var list = _config.LoadModuleHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenitLoadModule load = list[i] as WrenitLoadModule;

				WrenitLoadModuleResult result = load.Invoke(this, name);
				if (result == null || result.Source == null) continue;

				var ptr = Marshal.StringToCoTaskMemAnsi(result.Source);
				return new LoadModuleResult()
				{
					source = ptr,
					userData = ptr,
					onComplete = Marshal.GetFunctionPointerForDelegate<WrenLoadModuleCompleteFn>(onWrenLoadComplete),
				};
			}

			return new LoadModuleResult();
		}

		private void onWrenLoadComplete(IntPtr vm, string name, LoadModuleResult result)
		{
			Marshal.FreeHGlobal(result.userData);
		}

		private WrenForeignClassMethods onWrenBindForeignClass(IntPtr vm, string module, string className)
		{
			var list = _config.BindForeignClass.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenitBindForeignClass foreign = list[i] as WrenitBindForeignClass;
				WrenitClass @class = foreign.Invoke(this, module, className);
				if (@class.Allocator == null) continue;

				return new WrenForeignClassMethods()
				{
					allocate = @class.Allocator.MethodPtr,
					finalize = @class.Finalizer.MethodPtr,
				};
			}
			return new WrenForeignClassMethods();
		}

		private IntPtr onWrenBindForeignMethod(IntPtr vm, string module, string className, bool isStatic, string signature)
		{
			var list = _config.BindForeignMethod.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenitBindForeignMethod foreign = list[i] as WrenitBindForeignMethod;
				WrenitFunction method = foreign.Invoke(this, module, className, isStatic, signature);
				if (method == null) continue;
				return method.MethodPtr;
			}
			return IntPtr.Zero;
		}

		#endregion

		#region

		public WrenitValueType GetSlotType(int slot)
		{
			return ProcessEnum(WrenImport.wrenGetSlotType(_vm, slot));
		}

		#endregion
	}
}
