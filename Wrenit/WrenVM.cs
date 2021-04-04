using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Wrenit.Interlop;

namespace Wrenit
{
	public class WrenVm : IDisposable
	{
		/// <summary>
		/// list of all vm's
		/// </summary>
		private static readonly Dictionary<IntPtr, WeakReference<WrenVm>> VmList = new Dictionary<IntPtr, WeakReference<WrenVm>>();

		/// <summary>
		/// pointer to c vm
		/// </summary>
		private IntPtr _vm;

		/// <summary>
		/// the used config
		/// </summary>
		private WrenConfig _config;

		/// <summary>
		/// is the vm currently alive
		/// </summary>
		public bool IsAlive => _vm != IntPtr.Zero;

		#region Lifetime

		/// <summary>
		/// create a new vm with all default values 
		/// </summary>
		public WrenVm()
		{
			_vm = WrenImport.wrenNewVM(null);
			VmList.Add(_vm, new WeakReference<WrenVm>(this));
		}

		/// <summary>
		/// create a new vm with provided settings and bindings
		/// </summary>
		/// <param name="wrenConfig"></param>
		public WrenVm(WrenConfig wrenConfig)
		{
			_config = wrenConfig;

			InterlopWrenConfiguration interlopConfiguration = new InterlopWrenConfiguration
			{
				InitialHeapSize = new UIntPtr(wrenConfig.InitialHeapSize),
				MinHeapSize = new UIntPtr(wrenConfig.MinHeapSize),
				HeapGrowthPercent = wrenConfig.HeapGrowthPercent,
			};

			// make native "bindings" for wanted functions
			if (wrenConfig.WriteHandler != null) interlopConfiguration.WriteFn = OnWrenWrite;
			if (wrenConfig.ErrorHandler != null) interlopConfiguration.ErrorFn = OnWrenError;
			if (wrenConfig.ResolveModuleHandler != null) interlopConfiguration.ResolveModuleFn = OnWrenResolveModule;
			if (wrenConfig.LoadModuleHandler != null) interlopConfiguration.LoadModuleFn = OnWrenLoadModule;
			if (wrenConfig.BindForeignMethodHandler != null) interlopConfiguration.BindForeignMethodFn = OnWrenBindForeignMethod;
			if (wrenConfig.BindForeignClassHandler != null) interlopConfiguration.BindForeignClassFn= OnWrenBindForeignClass;

			_vm = WrenImport.wrenNewVM(interlopConfiguration);
			VmList.Add(_vm, new WeakReference<WrenVm>(this));
		}

		~WrenVm()
		{
			VmList.Remove(_vm);

			WrenImport.wrenFreeVM(_vm);
			_vm = IntPtr.Zero;
		}

		/// <summary>
		/// dispose the vm
		/// </summary>
		public void Dispose()
		{
			if (IsAlive == false) return;
			
			VmList.Remove(_vm);
			
			WrenImport.wrenFreeVM(_vm);
			_vm = IntPtr.Zero;

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Get a vm via its c pointer
		/// </summary>
		/// <param name="ptr"></param>
		/// <returns></returns>
		internal static WrenVm GetVm(IntPtr ptr)
		{
			if (VmList.TryGetValue(ptr, out WeakReference<WrenVm> weakRef) == false) return null;
			if (weakRef.TryGetTarget(out WrenVm vm) == false) return null;
			return vm;
		}

		#endregion

		/// <inheritdoc cref="WrenImport.wrenInterpret(IntPtr,string,string)"/>
		public WrenInterpretResult Interpret(string module, string source)
		{
			if (IsAlive == false)
				throw new ObjectDisposedException("Tried to Interpret module in a disposed VM");

			WrenInterpretResult error = WrenImport.wrenInterpret(_vm, module, source);
			return error;
		}

		#region Bindings

		/// <inheritdoc cref="WrenWriteFn"/>
		private void OnWrenWrite(IntPtr vm, string text)
		{
			Delegate[] list = _config.WriteHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenWrite write = list[i] as WrenWrite;
				write?.Invoke(this, text);
			}
		}

		/// <inheritdoc cref="WrenErrorFn"/>
		private void OnWrenError(IntPtr vm, WrenErrorType type, string module, int line, string message)
		{
			Delegate[] list = _config.ErrorHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenError error = list[i] as WrenError;
				error?.Invoke(this, type, module, line, message);
			}
		}

		/// <inheritdoc cref="WrenResolveModuleFn"/>
		private IntPtr OnWrenResolveModule(IntPtr vm, string importer, IntPtr namePtr)
		{
			string name = Marshal.PtrToStringAnsi(namePtr);
			string resolved = null;

			Delegate[] list = _config.ResolveModuleHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenResolveModule resolve = list[i] as WrenResolveModule;
				resolved = resolve?.Invoke(this, importer, name);
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

		/// <inheritdoc cref="WrenLoadModuleFn"/>
		private InterlopWrenLoadModuleResult OnWrenLoadModule(IntPtr vm, string name)
		{
			Delegate[] list = _config.LoadModuleHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenLoadModule load = list[i] as WrenLoadModule;

				string result = load?.Invoke(this, name);
				if (string.IsNullOrEmpty(result)) continue;

				IntPtr ptr = Marshal.StringToCoTaskMemAnsi(result);
				return new InterlopWrenLoadModuleResult()
				{
					Source = ptr,
					UserData = ptr,
					OnComplete = Marshal.GetFunctionPointerForDelegate<WrenLoadModuleCompleteFn>(OnWrenLoadComplete),
				};
			}

			return new InterlopWrenLoadModuleResult();
		}

		/// <inheritdoc cref="WrenLoadModuleCompleteFn"/>
		private void OnWrenLoadComplete(IntPtr vm, string name, InterlopWrenLoadModuleResult result)
		{
			Marshal.FreeHGlobal(result.UserData);
		}

		/// <inheritdoc cref="WrenBindForeignMethodFn"/>
		private InterlopWrenForeignClassMethods OnWrenBindForeignClass(IntPtr vm, string module, string className)
		{
			Delegate[] list = _config.BindForeignClassHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenBindForeignClass foreign = list[i] as WrenBindForeignClass;
				WrenForeignClass @class = foreign?.Invoke(this, module, className);
				if (@class?.Allocator == null) continue;

				return new InterlopWrenForeignClassMethods()
				{
					AllocateFn = @class.Allocator.MethodPtr,
					FinalizeFn = Marshal.GetFunctionPointerForDelegate(@class.Finalizer),
				};
			}
			return new InterlopWrenForeignClassMethods();
		}

		/// <inheritdoc cref="WrenBindForeignMethodFn"/>
		private IntPtr OnWrenBindForeignMethod(IntPtr vm, string module, string className, bool isStatic, string signature)
		{
			Delegate[] list = _config.BindForeignMethodHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenBindForeignMethod foreign = list[i] as WrenBindForeignMethod;
				WrenForeignMethodBinding methodBinding = foreign?.Invoke(this, module, className, isStatic, signature);
				if (methodBinding == null) continue;
				return methodBinding.MethodPtr;
			}
			return IntPtr.Zero;
		}

		#endregion

		#region

		/// <inheritdoc cref="WrenImport.wrenGetSlotType(IntPtr,int)"/>
		public WrenValueType GetSlotType(int slot)
		{
			return WrenImport.wrenGetSlotType(_vm, slot);
		}

		#endregion
	}
}
