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

	public enum WrenitResult
	{
		Unknown,
		Success,
		CompileError,
		RuntimeError,
		StackTrace,
		Disposed,
	}

	public class WrenitLoadModuleResult
	{
		public string Source;
	}

	public class WrenitVM : IDisposable
	{
		private IntPtr _vm;

		private WrenitConfig _config;

		public bool IsAlive => _vm != IntPtr.Zero;

		public WrenitVM()
		{
			_vm = WrenImport.xWrenNewVM(null);
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

			_vm = WrenImport.xWrenNewVM(wrenConfig);
		}

		~WrenitVM()
		{
			WrenImport.xWrenFreeVM(_vm);
			_vm = IntPtr.Zero;
		}

		public void Dispose()
		{
			if (IsAlive == false) return;

			WrenImport.xWrenFreeVM(_vm);
			_vm = IntPtr.Zero;

			GC.SuppressFinalize(this);
		}

		public WrenitResult Interpret(string module, string source)
		{
			if (IsAlive == false)
				throw new ObjectDisposedException("Tried to Interpret module in disposed VM");

			WrenInterpretResult error = WrenImport.xWrenInterpret(_vm, module, source);
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
			IntPtr ptr = WrenImport.xWrenReallocate(_vm, IntPtr.Zero, UIntPtr.Zero, size);

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
				
				return new LoadModuleResult()
				{
					source = result.Source
				};
			}

			return new LoadModuleResult();
		}
	}
}
