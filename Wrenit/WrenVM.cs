using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using Wrenit.Interop;

namespace Wrenit
{
	/// <summary>
	/// A VM to interpret, call and run wren syntax files   
	/// </summary>
	public sealed class WrenVm : IDisposable
	{
		/// <summary>
		/// list of all active vm's
		/// </summary>
		private static readonly Dictionary<IntPtr, WeakReference<WrenVm>> VmList =
			new Dictionary<IntPtr, WeakReference<WrenVm>>();

		/// <summary>
		/// list of foreign objects created by the vm. They dont get garbage collected without disposing them 
		/// </summary>
		private static readonly Dictionary<IntPtr, WrenForeignObject>
			ForeignObjects = new Dictionary<IntPtr, WrenForeignObject>();
		
		/// <summary>
		/// last used foreign id if no unused foreign ids are available
		/// </summary>
		private static int _lastForeignId = 0;
		
		/// <summary>
		/// list of handles created by the vm. They dont get garbage collected without disposing them 
		/// </summary>
		private readonly Dictionary<IntPtr, WrenHandle> _handles = new Dictionary<IntPtr, WrenHandle>();

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
			Wren.Initialize();
			Ptr = WrenImport.wrenNewVM(null);
			VmList.Add(Ptr, new WeakReference<WrenVm>(this));
		}

		/// <summary>
		/// create a new vm with provided settings and bindings
		/// </summary>
		/// <param name="wrenConfig"></param>
		public WrenVm(WrenConfig wrenConfig)
		{
			Wren.Initialize();
			_config = wrenConfig;

			InteropWrenConfiguration interopConfiguration = new InteropWrenConfiguration
			{
				InitialHeapSize = new UIntPtr(wrenConfig.InitialHeapSize),
				MinHeapSize = new UIntPtr(wrenConfig.MinHeapSize),
				HeapGrowthPercent = wrenConfig.HeapGrowthPercent,
			};

			// make native "bindings" for wanted functions
			if (wrenConfig.WriteHandler != null) interopConfiguration.WriteFn = OnWrenWrite;
			if (wrenConfig.ErrorHandler != null) interopConfiguration.ErrorFn = OnWrenError;
			if (wrenConfig.ResolveModuleHandler != null) interopConfiguration.ResolveModuleFn = OnWrenResolveModule;
			if (wrenConfig.LoadModuleHandler != null) interopConfiguration.LoadModuleFn = OnWrenLoadModule;
			if (wrenConfig.BindForeignMethodHandler != null)
				interopConfiguration.BindForeignMethodFn = OnWrenBindForeignMethod;
			if (wrenConfig.BindForeignClassHandler != null) interopConfiguration.BindForeignClassFn = OnWrenBindForeignClass;

			Ptr = WrenImport.wrenNewVM(interopConfiguration);
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

		/// <summary>
		/// actual function that frees the vm and all its dependents
		/// </summary>
		private void Free()
		{
			if (IsAlive == false) return;

			Dictionary<IntPtr, WrenHandle> handles = new Dictionary<IntPtr, WrenHandle>(_handles);
			foreach (var pair in handles)
			{
				pair.Value.Free(this);
			}

			handles.Clear();

			WrenImport.wrenFreeVM(Ptr);
			VmList.Remove(Ptr);

			Ptr = IntPtr.Zero;
		}

		#endregion

		#region Internal Access

		/// <summary>
		/// Get a vm via its c pointer
		/// </summary>
		/// <param name="ptr"></param>
		/// <returns></returns>
		internal static WrenVm GetVm(IntPtr ptr)
		{
			if (VmList.TryGetValue(ptr, out WeakReference<WrenVm> weakRef) == false) return null;
			if (weakRef.TryGetTarget(out WrenVm vm) == false) return null;
			if (vm.IsAlive == false) return null;

			return vm;
		}

		/// <summary>
		/// remove a foreign object from this vm's list.
		/// Note this does not free the object
		/// </summary>
		/// <param name="id">pointer of the object</param>
		internal static void RemoveForeignObject(IntPtr id)
		{
			if (ForeignObjects.ContainsKey(id) == false) return;

			ForeignObjects.Remove(id);
		}

		/// <summary>
		/// remove a handle from the vm's list.
		/// Note this does not free the object
		/// </summary>
		/// <param name="handlePtr">pointer of the handle</param>
		internal void RemoveHandle(IntPtr handlePtr)
		{
			_handles.Remove(handlePtr);
		}

		/// <summary>
		/// get foreign object by its id
		/// </summary>
		/// <param name="id">id of the foreign object</param>
		/// <returns>returns object if found</returns>
		internal static WrenForeignObject GetForeignById(IntPtr id)
		{
			if (ForeignObjects.ContainsKey(id) == false) return null;

			return ForeignObjects[id];
		}

		#endregion

		#region Run
		
		/// <summary>
		/// Runs <paramref name="source"/>, a string of Wren source code in a new fiber in the context of <paramref name="module"/>.
		/// </summary>
		/// <param name="module">module name</param>
		/// <param name="source">module source</param>
		/// <returns>interpret result</returns>
		public WrenInterpretResult Interpret(string module, string source)
		{
			this.AssertAlive();
			WrenInterpretResult error = WrenImport.wrenInterpret(Ptr, module, source);
			return error;
		}

		/// <summary>
		/// Creates a handle that can be used to invoke a method with <paramref name="signature"/>
		///
		/// <para>
		/// 	This handle can be used repeatedly to directly invoke that method using <see cref="Call"/>.
		/// </para>
		///
		/// <para>
		///		When you are done with this handle, it must be released using its <see cref="WrenHandle.Dispose()"/> or Vm's <see cref="ReleaseHandle"/>.
		/// </para>
		/// </summary>
		/// <param name="signature">method signature</param>
		/// <returns>pointer to handle</returns>
		public WrenSignatureHandle MakeCallHandle(string signature)
		{
			this.AssertAlive();
			IntPtr handlePtr = WrenImport.wrenMakeCallHandle(Ptr, signature);
			WrenSignatureHandle handle = new WrenSignatureHandle(this, handlePtr);
			_handles.Add(handlePtr, handle);
			return handle;
		}

		/// <summary>
		/// Calls <paramref name="handle"/>, using the receiver and arguments previously set up on the stack.
		///
		/// <para>
		/// 	<paramref name="handle"/> must have been created by a call to <see cref="MakeCallHandle"/>. The
		/// 	arguments to the method must be already on the stack. The receiver should be
		/// 	in slot 0 with the remaining arguments following it, in order. It is an
		/// 	error if the number of arguments provided does not match the method's
		/// 	signature.
		/// </para>
		///
		/// <para>
		///		After this returns, you can access the return value from slot 0 on the stack.
		/// </para>
		/// </summary>
		/// <returns>interpret result</returns>
		public WrenInterpretResult Call(WrenSignatureHandle handle)
		{
			this.AssertAlive();
			handle.AssertAlive();
			return WrenImport.wrenCall(Ptr, handle.Ptr);
		}

		/// <summary>
		/// Releases the reference stored in <paramref name="handle"/>. After calling this, <paramref name="handle"/> can
		/// no longer be used.
		/// </summary>
		/// <param name="handle"></param>
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
			UIntPtr size = new UIntPtr((uint) (resolved.Length + 1) * (uint) IntPtr.Size);

			// 2. create pointer using same allocator that wren uses 
			IntPtr ptr = WrenConfig.DefaultReallocateFn.Invoke(IntPtr.Zero, size, IntPtr.Zero);

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
		private InteropWrenLoadModuleResult OnWrenLoadModule(IntPtr vm, string name)
		{
			Delegate[] list = _config.LoadModuleHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenLoadModule load = list[i] as WrenLoadModule;

				string result = load?.Invoke(this, name);
				if (string.IsNullOrEmpty(result)) continue;

				IntPtr ptr = Marshal.StringToCoTaskMemAnsi(result);
				return new InteropWrenLoadModuleResult()
				{
					Source = ptr,
					UserData = ptr,
					OnComplete = Marshal.GetFunctionPointerForDelegate<WrenLoadModuleCompleteFn>(OnWrenLoadComplete),
				};
			}

			return new InteropWrenLoadModuleResult();
		}

		/// <inheritdoc cref="WrenLoadModuleCompleteFn"/>
		private void OnWrenLoadComplete(IntPtr vm, string name, InteropWrenLoadModuleResult result)
		{
			Marshal.FreeHGlobal(result.UserData);
		}

		/// <inheritdoc cref="WrenBindForeignMethodFn"/>
		private InteropWrenForeignClassMethods OnWrenBindForeignClass(IntPtr vm, string module, string className)
		{
			Delegate[] list = _config.BindForeignClassHandler.GetInvocationList();
			for (int i = 0; i < list.Length; i++)
			{
				WrenBindForeignClass foreign = list[i] as WrenBindForeignClass;
				WrenForeignClassBinding classBinding = foreign?.Invoke(this, module, className);
				if (classBinding?.Allocator == null) continue;

				return new InteropWrenForeignClassMethods()
				{
					AllocateFn = classBinding.Allocator.MethodPtr,
					FinalizeFn = classBinding.Finalizer.MethodPtr,
				};
			}

			// wren defaults to aborting when no allocator is defined
			// to avoid sudden aborts we pass a dummy allocator, the following construct **will** fail
			// resulting in a safe WrenInterpretError.RuntimeError
			_config.ErrorHandler?.Invoke(this, WrenErrorType.WrenitRuntimeError, module, -1,
				$"Allocator for foreign {className} not defined in bindings");
			return new InteropWrenForeignClassMethods()
			{
				AllocateFn = _notImplementedBinding.MethodPtr,
			};
		}

		// catch all to avoid aborts
		private readonly WrenForeignMethodBinding _notImplementedBinding =
			new WrenForeignMethodBinding(NotImplementedForeign);

		private static void NotImplementedForeign(WrenVm vm) { }

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

		/// <summary>
		/// Returns the number of slots available to the current foreign method.
		/// </summary>
		/// <returns>slot count</returns>		
		public int GetSlotCount() => WrenImport.wrenGetSlotCount(Ptr);

		/// <summary>
		/// Ensures that the foreign method stack has at least <paramref name="slots"/> available for
		/// use, growing the stack if needed.
		///
		/// Does not shrink the stack if it has more than enough slots.
		///
		/// It is an error to call this from a finalizer.
		/// </summary>
		/// <param name="slots">count of slots to ensure</param>
		public void EnsureSlots(int slots) => WrenImport.wrenEnsureSlots(Ptr, slots);

		/// <summary>
		/// Gets the type of the object in <paramref name="slot"/>
		/// </summary>
		/// <param name="slot">slot index</param>
		/// <returns>resolved type</returns>
		public WrenValueType GetSlotType(int slot)
		{
			this.AssertSlotCount(slot);
			return WrenImport.wrenGetSlotType(Ptr, slot);
		}

		/// <summary>
		/// Reads a boolean value from <paramref name="slot"/>.
		/// It is an error to call this if the slot does not contain a boolean value.
		/// </summary>
		/// <param name="slot">slot to get</param>
		public bool GetSlotBool(int slot)
		{
			this.AssertSlot(slot, WrenValueType.Bool);
			return WrenImport.wrenGetSlotBool(Ptr, slot);
		}

		/// <summary>
		/// Stores the boolean <paramref name="value"/> in <paramref name="slot"/>.
		/// </summary>
		/// <param name="slot">slot to store int</param>
		/// <param name="value">value to store</param>
		public void SetSlotBool(int slot, bool value)
		{
			this.AssertSlot(slot);
			WrenImport.wrenSetSlotBool(Ptr, slot, value);
		}

		/// <summary>
		/// Reads a byte array from <paramref name="slot"/>.
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		/// <param name="slot">slot to get</param>
		public byte[] GetSlotBytes(int slot)
		{
			this.AssertSlot(slot, WrenValueType.String);
			IntPtr arrayPtr = WrenImport.wrenGetSlotBytes(Ptr, slot, out int length);
			byte[] managedArray = new byte[length];
			Marshal.Copy(arrayPtr, managedArray, 0, length);
			return managedArray;
		}

		/// <summary>
		/// Stores the array of <paramref name="bytes"/> in <paramref name="slot"/>.
		///
		/// </summary>
		/// <param name="slot">slot to store in</param>
		/// <param name="bytes">bytes to store</param>
		public void SetSlotBytes(int slot, byte[] bytes)
		{
			this.AssertSlot(slot);
			IntPtr arrayPtr = Marshal.AllocHGlobal(bytes.Length);
			Marshal.Copy(bytes, 0, arrayPtr, bytes.Length);
			WrenImport.wrenSetSlotBytes(Ptr, slot, arrayPtr, new UIntPtr((uint) bytes.Length));
			Marshal.FreeHGlobal(arrayPtr);
		}

		/// <summary>
		/// Reads a number from <paramref name="slot"/>.
		///
		/// It is an error to call this if the slot does not contain a number.
		/// </summary>
		/// <param name="slot">slot to get</param>
		public double GetSlotDouble(int slot)
		{
			this.AssertSlot(slot, WrenValueType.Number);
			return WrenImport.wrenGetSlotDouble(Ptr, slot);
		}

		/// <summary>
		/// Stores the numeric <paramref name="value"/> in <paramref name="slot"/>.
		/// </summary>
		/// <param name="slot">slot to store in</param>
		/// <param name="value">value to store</param>
		public void SetSlotDouble(int slot, double value)
		{
			this.AssertSlot(slot);
			WrenImport.wrenSetSlotDouble(Ptr, slot, value);
		}

		/// <summary>
		/// Reads a string from <paramref name="slot"/>.
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		/// <param name="slot">slot to get</param>
		public string GetSlotString(int slot)
		{
			this.AssertSlot(slot, WrenValueType.String);
			IntPtr intPtr =  WrenImport.wrenGetSlotString(Ptr, slot);
			return Marshal.PtrToStringAnsi(intPtr);
		}

		/// <summary>
		/// Stores the string <paramref name="value"/> in <paramref name="slot"/>
		///
		/// <para>
		/// 	If the string may contain any null bytes in the middle, then you
		/// 	should use <see cref="SetSlotBytes"/> instead.
		/// </para>
		/// </summary>
		public void SetSlotString(int slot, string value)
		{
			this.AssertSlot(slot);
			WrenImport.wrenSetSlotString(Ptr, slot, value);
		}

		/// <summary>
		/// Stores null in <paramref name="slot"/>.
		/// </summary>
		/// <param name="slot">slot to store in</param>
		public void SetSlotNull(int slot)
		{
			this.AssertSlot(slot);
			WrenImport.wrenSetSlotNull(Ptr, slot);
		}

		/// <summary>
		/// Creates a handle for the value stored in <paramref name="slot"/>.
		///
		/// This will prevent the object that is referred to from being garbage collected
		/// until the handle is released by calling <see cref="ReleaseHandle"/>.
		/// </summary>
		/// <param name="slot">slot to get</param>
		public WrenHandle GetSlotHandle(int slot)
		{
			this.AssertSlot(slot);
			IntPtr handlePtr = WrenImport.wrenGetSlotHandle(Ptr, slot);
			WrenHandle handle = new WrenHandle(this, handlePtr);
			_handles.Add(handlePtr, handle);
			return handle;
		}

		/// <summary>
		/// Stores the value captured in <paramref name="handle"/> in <paramref name="slot"/>.
		///
		/// This does not release the handle for the value.
		/// </summary>
		/// <param name="slot">slot to store in</param>
		/// <param name="handle">pointer of handle to store</param>
		public void SetSlotHandle(int slot, WrenHandle handle)
		{
			this.AssertSlot(slot);
			WrenImport.wrenSetSlotHandle(Ptr, slot, handle.Ptr);
		}

		#region Lists

		/// <summary>
		/// Stores a new empty list in <paramref name="slot"/>.
		/// </summary>
		/// <param name="slot">slot to store in</param>
		public void SetSlotNewList(int slot)
		{
			this.AssertSlot(slot);
			WrenImport.wrenSetSlotNewList(Ptr, slot);
		}

		/// <summary>
		/// Returns the number of elements in the list stored in <paramref name="slot"/>.
		/// </summary>
		/// <param name="slot">slot get from</param>
		/// <returns>count of list elements</returns>
		public int GetListCount(int slot)
		{
			this.AssertSlot(slot, WrenValueType.List);
			return WrenImport.wrenGetListCount(Ptr, slot);
		}

		/// <summary>
		/// Sets the value stored at <paramref name="index"/> in the list at <paramref name="listSlot"/>, 
		/// to the value from <paramref name="elementSlot"/>. 
		/// </summary>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index in the list</param>
		/// <param name="elementSlot">slot of value to store in list</param>
		public void SetListElement(int listSlot, int index, int elementSlot)
		{
			this.AssertSlot(listSlot, WrenValueType.List);
			this.AssertSlot(elementSlot);
			WrenImport.wrenSetListElement(Ptr, listSlot, index, elementSlot);
		}

		/// <summary>
		/// Reads element <paramref name="index"/> from the list in <paramref name="listSlot"/> and stores it in <paramref name="elementSlot"/>.
		/// </summary>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index in the list</param>
		/// <param name="elementSlot">slot to store the value in</param>
		public void GetListElement(int listSlot, int index, int elementSlot)
		{
			this.AssertSlot(listSlot, WrenValueType.List);
			this.AssertSlot(elementSlot);
			WrenImport.wrenGetListElement(Ptr, listSlot, index, elementSlot);
		}

		/// <summary>
		/// Takes the value stored at <paramref name="elementSlot"/> and inserts it into the list stored
		/// at <paramref name="listSlot"/> at <paramref name="index"/>.
		///
		/// <para>
		/// 	As in Wren, negative indexes can be used to insert from the end. To append an element, use `-1` for the index.
		/// </para>
		/// </summary>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index to store element</param>
		/// <param name="elementSlot">slot of value to store in list</param>
		public void InsertInList(int listSlot, int index, int elementSlot)
		{
			this.AssertSlot(listSlot, WrenValueType.List);
			this.AssertSlot(elementSlot);
			WrenImport.wrenInsertInList(Ptr, listSlot, index, elementSlot);
		}

		#endregion

		#region Maps

		/// <summary>
		/// Stores a new empty map in <paramref name="slot"/>.
		/// </summary>
		/// <param name="slot">slot to store</param>
		public void SetSlotNewMap(int slot)
		{
			this.AssertSlot(slot);
			WrenImport.wrenSetSlotNewMap(Ptr, slot);
		}

		/// <summary>
		/// Returns the number of entries in the map stored in <paramref name="slot"/>.
		/// </summary>
		/// <param name="slot">slot to look at</param>
		/// <returns>count of entries</returns>
		public int GetMapCount(int slot)
		{
			this.AssertSlot(slot, WrenValueType.Map);
			return WrenImport.wrenGetMapCount(Ptr, slot);
		}

		/// <summary>
		/// Returns true if the key in <paramref name="keySlot"/> is found in the map placed in <paramref name="mapSlot"/>.
		/// </summary>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of value to check exists</param>
		public bool GetMapContainsKey(int mapSlot, int keySlot)
		{
			this.AssertSlot(mapSlot, WrenValueType.Map);
			this.AssertSlot(keySlot);
			return WrenImport.wrenGetMapContainsKey(Ptr, mapSlot, keySlot);
		}

		/// <summary>
		/// Retrieves a value with the key in <paramref name="keySlot"/> from the map in <paramref name="mapSlot"/> and
		/// stores it in <paramref name="valueSlot"/>.
		/// </summary>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of the key</param>
		/// <param name="valueSlot">slot to store the value in</param>
		public void GetMapValue(int mapSlot, int keySlot, int valueSlot)
		{
			this.AssertSlot(mapSlot, WrenValueType.Map);
			this.AssertSlot(keySlot);
			this.AssertSlot(valueSlot);
			WrenImport.wrenGetMapValue(Ptr, mapSlot, keySlot, valueSlot);
		}

		/// <summary>
		/// Takes the value stored at <paramref name="valueSlot"/> and inserts it into the map stored
		/// at <paramref name="mapSlot"/> with key <paramref name="keySlot"/>.
		/// </summary>
		/// <param name="mapSlot">slot where map is</param>
		/// <param name="keySlot">slot of the key to store in</param>
		/// <param name="valueSlot">slot of the value to store</param>
		public void SetMapValue(int mapSlot, int keySlot, int valueSlot)
		{
			this.AssertSlot(mapSlot, WrenValueType.Map);
			this.AssertSlot(keySlot);
			this.AssertSlot(valueSlot);
			WrenImport.wrenSetMapValue(Ptr, mapSlot, keySlot, valueSlot);
		}

		/// <summary>
		/// Removes a value from the map in <paramref name="mapSlot"/>, with the key from <paramref name="keySlot"/>,
		/// and place it in <paramref name="removedValueSlot"/>. If not found, <paramref name="removedValueSlot"/> is
		/// set to null, the same behaviour as the Wren Map API.
		/// </summary>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of the key to remove</param>
		/// <param name="removedValueSlot">slot to store value that was removed</param>
		public void RemoveMapValue(int mapSlot, int keySlot, int removedValueSlot)
		{
			this.AssertSlot(mapSlot, WrenValueType.Map);
			this.AssertSlot(keySlot);
			this.AssertSlot(removedValueSlot);
			WrenImport.wrenRemoveMapValue(Ptr, mapSlot, keySlot, removedValueSlot);
		}

		#endregion

		/// <summary>
		/// Creates a new instance of the foreign class stored in <paramref name="classSlot"/>
		/// and places the resulting object in <paramref name="slot"/>.
		///
		/// <para>
		/// 	This does not invoke the foreign class's constructor on the new instance. If
		/// 	you need that to happen, call the constructor from Wren, which will then
		/// 	call the allocator foreign method. In there, call this to create the object
		/// 	and then the constructor will be invoked when the allocator returns.
		/// </para>
		///
		/// </summary>
		/// <param name="slot">slot to get</param>
		/// <param name="classSlot">slot to store in</param>
		public void SetSlotNewForeign<T>(int slot, int classSlot)
		{
			this.AssertSlot(slot);
			this.AssertSlot(classSlot);
			
			_lastForeignId++;
			if (_lastForeignId == 0) _lastForeignId++;
			IntPtr id =  new IntPtr(_lastForeignId);

			WrenForeignObject wrenForeignObject = new WrenForeignObject<T>(id);
			ForeignObjects.Add(id, wrenForeignObject);
			IntPtr ptr = WrenImport.wrenSetSlotNewForeign(Ptr, slot, classSlot, new IntPtr(IntPtr.Size));
			Marshal.WriteIntPtr(ptr, id);
		}

		/// <summary>
		/// Reads a foreign object from <paramref name="slot"/>
		///
		/// It is an error to call this if the slot does not contain an instance of a
		/// foreign class.
		/// </summary>
		/// <param name="slot">slot to get</param>
		/// <returns>the foreign object</returns>
		public WrenForeignObject GetSlotForeign(int slot)
		{
			this.AssertSlot(slot, WrenValueType.Foreign);
			IntPtr ptr = WrenImport.wrenGetSlotForeign(Ptr, slot);
			IntPtr id = Marshal.ReadIntPtr(ptr);
			if (ForeignObjects.TryGetValue(id, out WrenForeignObject obj) == false) return null;

			return obj;
		}
		
		/// <summary>
		/// Reads a foreign object from <paramref name="slot"/>
		///
		/// It is an error to call this if the slot does not contain an instance of a
		/// foreign class.
		/// </summary>
		/// <param name="slot">slot to get</param>
		/// <returns>the foreign object</returns>
		public WrenForeignObject<T> GetSlotForeign<T>(int slot) => GetSlotForeign(slot) as WrenForeignObject<T>;

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/> and stores
		/// it in <paramref name="slot"/>.
		/// </summary>
		/// <param name="module">module to check in</param>
		/// <param name="name">name to get</param>
		/// <param name="slot">slot to store in</param>
		public void GetVariable(string module, string name, int slot)
		{
			this.AssertVariable(module, name);
			this.AssertSlot(slot);
			WrenImport.wrenGetVariable(Ptr, module, name, slot);
		}

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/>, 
		/// returns false if not found. The module must be imported at the time, 
		/// use <see cref="HasModule"/>  to ensure that before calling.
		/// </summary>
		/// <param name="module">module to check in</param>
		/// <param name="name">name to check for</param>
		public bool HasVariable(string module, string name)
		{
			this.AssertModule(module);
			return WrenImport.wrenHasVariable(Ptr, module, name);
		}

		/// <summary>
		/// Returns true if <paramref name="module"/> has been imported/resolved before, false if not.
		/// </summary>
		/// <param name="module">module to check</param>
		public bool HasModule(string module) => WrenImport.wrenHasModule(Ptr, module);

		/// <summary>
		/// Sets the current fiber to be aborted, and uses the value in <paramref name="slot"/> as the runtime error object.
		/// </summary>
		/// <param name="slot">slot for the runtime error</param>
		public void AbortFiber(int slot)
		{
			this.AssertSlot(slot);
			WrenImport.wrenAbortFiber(Ptr, slot);
		}

		#endregion
	}
}
