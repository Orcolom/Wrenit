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
		internal IntPtr Ptr;

		/// <summary>
		/// the used config
		/// </summary>
		private readonly WrenConfig _config;

		/// <summary>
		/// is the vm currently alive
		/// </summary>
		public bool IsAlive => Ptr != IntPtr.Zero;

		#region Lifetime

		/// <summary>
		/// create a new vm with all default values 
		/// </summary>
		public WrenVm()
		{
			Ptr = WrenImport.wrenNewVM(null);
			VmList.Add(Ptr, new WeakReference<WrenVm>(this));
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

			Ptr = WrenImport.wrenNewVM(interlopConfiguration);
			VmList.Add(Ptr, new WeakReference<WrenVm>(this));
		}

		~WrenVm()
		{
			Free();
		}

		/// <summary>
		/// dispose the vm
		/// </summary>
		public void Dispose()
		{
			Free();
			GC.SuppressFinalize(this);
		}

		private void Free()
		{
			if (IsAlive == false) return;
			
			VmList.Remove(Ptr);
			
			WrenImport.wrenFreeVM(Ptr);
			Ptr = IntPtr.Zero;
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

		#region Run

		/// <inheritdoc cref="WrenImport.wrenInterpret(IntPtr,string,string)"/>
		public WrenInterpretResult Interpret(string module, string source)
		{
			if (IsAlive == false)
				throw new ObjectDisposedException("Tried to Interpret module in a disposed VM");

			WrenInterpretResult error = WrenImport.wrenInterpret(Ptr, module, source);
			return error;
		}
		
		/// <inheritdoc cref="WrenImport.wrenMakeCallHandle(IntPtr,string)"/>
		public WrenSignatureHandle MakeCallHandle(string signature)
		{
			IntPtr handlePtr = WrenImport.wrenMakeCallHandle(Ptr, signature);
			WrenSignatureHandle handle =	new WrenSignatureHandle(this, handlePtr);
			return handle;
		}

		/// <inheritdoc cref="WrenImport.wrenCall(IntPtr,IntPtr)"/>
		public WrenInterpretResult Call(WrenSignatureHandle handle)
		{
			return WrenImport.wrenCall(Ptr, handle.Ptr);
		}

		/// <inheritdoc cref="WrenImport.wrenReleaseHandle(IntPtr,IntPtr)"/>
		public void ReleaseHandle(WrenHandle handle)
		{
			handle?.Dispose();
		}

		#endregion
		
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
			IntPtr ptr = WrenImport.wrenReallocate(Ptr, IntPtr.Zero, UIntPtr.Zero, size);

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
		
		#region Slots

		/// <inheritdoc cref="WrenImport.wrenGetSlotCount(IntPtr)"/>
		public int GetSlotCount() => WrenImport.wrenGetSlotCount(Ptr);

		/// <inheritdoc cref="WrenImport.wrenEnsureSlots"/>
		public void EnsureSlots(int slots) => WrenImport.wrenEnsureSlots(Ptr, slots);

		/// <inheritdoc cref="WrenImport.wrenGetSlotType"/>
		public WrenValueType GetSlotType(int slot) => WrenImport.wrenGetSlotType(Ptr, slot);

		/// <inheritdoc cref="WrenImport.wrenGetSlotBool"/>
		public bool GetSlotBool(int slot) => WrenImport.wrenGetSlotBool(Ptr, slot);

		/// <inheritdoc cref="WrenImport.wrenSetSlotBool"/>
		public void SetSlotBool(int slot, bool value) => WrenImport.wrenSetSlotBool(Ptr, slot, value);

		/// <inheritdoc cref="WrenImport.wrenGetSlotBytes"/>
		public byte[] GetSlotBytes(int slot)
		{
			IntPtr arrayPtr = WrenImport.wrenGetSlotBytes(Ptr, slot, out int length);
			byte[] managedArray = new byte[length];
			Marshal.Copy(arrayPtr, managedArray, 0, length);
			return managedArray;
		}

		/// <inheritdoc cref="WrenImport.wrenSetSlotBytes"/>
		public void SetSlotBytes(int slot, byte[] bytes)
		{
			IntPtr arrayPtr = Marshal.AllocHGlobal(bytes.Length);
			Marshal.Copy(bytes, 0, arrayPtr, bytes.Length);
			WrenImport.wrenSetSlotBytes(Ptr, slot, arrayPtr, new UIntPtr((uint)bytes.Length));
			Marshal.FreeHGlobal(arrayPtr);
		}

		/// <inheritdoc cref="WrenImport.wrenGetSlotDouble"/>
		public double GetSlotDouble(int slot) => WrenImport.wrenGetSlotDouble(Ptr, slot);

		/// <inheritdoc cref="WrenImport.wrenSetSlotDouble"/>
		public void SetSlotDouble(int slot, double value) => WrenImport.wrenSetSlotDouble(Ptr, slot, value);

		/// <inheritdoc cref="WrenImport.wrenGetSlotString"/>
		public string GetSlotString(int slot) => WrenImport.wrenGetSlotString(Ptr, slot);
		
		/// <inheritdoc cref="WrenImport.wrenGetSlotString"/>
		public void SetSlotString(int slot, string value) => WrenImport.wrenSetSlotString(Ptr, slot, value);

		/// <inheritdoc cref="WrenImport.wrenSetSlotNull"/>
		public void SetSlotNull(int slot) => WrenImport.wrenSetSlotNull(Ptr, slot);

		/// <inheritdoc cref="WrenImport.wrenGetSlotHandle"/>
		public WrenHandle GetSlotHandle(int slot)
		{
			IntPtr handlePtr = WrenImport.wrenGetSlotHandle(Ptr, slot);
			return new WrenHandle(this, handlePtr);
		}

		/// <inheritdoc cref="WrenImport.wrenSetSlotHandle"/>
		public void SetSlotHandle(int slot, WrenHandle handle) => WrenImport.wrenSetSlotHandle(Ptr, slot, handle.Ptr);

		/// <inheritdoc cref="WrenImport.wrenSetSlotNewList"/>
		public void SetSlotNewList(int slot) => WrenImport.wrenSetSlotNewList(Ptr, slot);

		/// <inheritdoc cref="WrenImport.wrenGetListCount"/>
		public int GetListCount(int slot) => WrenImport.wrenGetListCount(Ptr, slot);

		/// <inheritdoc cref="WrenImport.wrenSetListElement"/>
		public void SetListElement(int listSlot, int index, int elementSlot) =>
			WrenImport.wrenSetListElement(Ptr, listSlot, index, elementSlot);

		/// <inheritdoc cref="WrenImport.wrenGetListElement"/>
		public void GetListElement(int listSlot, int index, int elementSlot) => WrenImport.wrenGetListElement(Ptr, listSlot, index, elementSlot);

		/// <inheritdoc cref="WrenImport.wrenInsertInList"/>
		public void InsertInList(int listSlot, int index, int elementSlot) => WrenImport.wrenInsertInList(Ptr, listSlot, index, elementSlot);

		/// <inheritdoc cref="WrenImport.wrenSetSlotNewMap"/>
		public void SetSlotNewMap(int slot) => WrenImport.wrenSetSlotNewMap(Ptr, slot);
		
		/// <inheritdoc cref="WrenImport.wrenGetMapCount"/>
		public int GetMapCount(int slot) => WrenImport.wrenGetMapCount(Ptr, slot);
		
		/// <inheritdoc cref="WrenImport.wrenGetMapContainsKey"/>
		public bool GetMapContainsKey(int mapSlot, int keySlot) => WrenImport.wrenGetMapContainsKey(Ptr, mapSlot, keySlot);

		/// <inheritdoc cref="WrenImport.wrenGetMapValue"/>
		public void GetMapValue(int mapSlot, int keySlot, int valueSlot) => WrenImport.wrenGetMapValue(Ptr, mapSlot, keySlot, valueSlot);

		/// <inheritdoc cref="WrenImport.wrenSetMapValue"/>
		public void SetMapValue(int mapSlot, int keySlot, int valueSlot) => WrenImport.wrenSetMapValue(Ptr, mapSlot, keySlot, valueSlot);
		
		/// <inheritdoc cref="WrenImport.wrenRemoveMapValue"/>
		public void RemoveMapValue(int mapSlot, int keySlot, int removedValueSlot) => WrenImport.wrenRemoveMapValue(Ptr, mapSlot, keySlot, removedValueSlot);

		/// <inheritdoc cref="WrenImport.wrenRemoveMapValue"/>
		public void GetVariable(string module, string name, int slot) => WrenImport.wrenGetVariable(Ptr, module, name, slot);

		/// <inheritdoc cref="WrenImport.wrenHasVariable"/>
		public bool HasVariable(IntPtr vm, string module, string name) => WrenImport.wrenHasVariable(Ptr, module, name);
		
		/// <inheritdoc cref="WrenImport.wrenHasModule"/>
		public bool HasModule(IntPtr vm, string module) => WrenImport.wrenHasModule(Ptr, module);

		/// <inheritdoc cref="WrenImport.wrenAbortFiber"/>
		public void AbortFiber(int slot) => WrenImport.wrenAbortFiber(Ptr, slot);
		#endregion
	}
}
