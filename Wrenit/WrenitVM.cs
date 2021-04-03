using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{
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

		private WrenReallocateFn _reallocateFn;

		public bool IsAlive => _vm != IntPtr.Zero;

		#region Events/Callbacks

		public delegate void WrenitWrite(WrenitVM vm, string text);
		public WrenitWrite WriteHandler;

		public delegate void WrenitError(WrenitVM vm, WrenitResult result, string module, int line, string message);
		public WrenitError ErrorHandler;

		public delegate string WrenitResolveModule(WrenitVM vm, string importer, string name);
		public WrenitResolveModule ResolveModuleHandler;

		public delegate WrenitLoadModuleResult WrenitLoadModule(WrenitVM vm, string name);
		public WrenitLoadModule LoadModuleHandler;

		#endregion

		public WrenitVM() : this(WrenitConfig.GetDefaults()) { }

		public WrenitVM(WrenitConfig config)
		{
			_reallocateFn = config.ReallocateFn;
			
			WrenConfig wrenConfig = new WrenConfig()
			{
				ReallocateFn = config.ReallocateFn,
				
				WrenWriteFn = OnWrenWrite,
				WrenErrorFn = OnWrenError,

				ResolveModuleFn = OnWrenResolveModuleName,
				LoadModuleFn = onWrenLoadModule,
				
				bindForeignClassFn = null,
				bindForeignMethodFn = null,

				initialHeapSize = new UIntPtr(config.InitialHeapSize),
				minHeapSize = new UIntPtr(config.MinHeapSize),
				heapGrowthPercent = config.HeapGrowthPercent,
				
				userData = IntPtr.Zero,
			};
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
			switch(error)
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

		private void OnWrenWrite(IntPtr vm, string text) => WriteHandler?.Invoke(this, text);
		private void OnWrenError(IntPtr vm, WrenErrorType type, string module, int line, string message) => ErrorHandler?.Invoke(this, ProcessEnum(type), module, line, message);
		
		private IntPtr OnWrenResolveModuleName(IntPtr vm, string importer, IntPtr namePtr)
		{
			if (ResolveModuleHandler == null) return namePtr;


			string name = Marshal.PtrToStringAnsi(namePtr);

			string resolved = ResolveModuleHandler?.Invoke(this, importer, name);
			if (resolved == name) return namePtr;

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
			if (LoadModuleHandler != null)
			{
				var ret = LoadModuleHandler?.Invoke(this, name);
				return new LoadModuleResult()
				{
					source = ret.Source
				};
			}
			return new LoadModuleResult();
		}
	}
}
