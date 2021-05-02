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
		internal readonly WrenCache Cache;

		/// <summary>
		/// pointer to c vm
		/// </summary>
		internal IntPtr Ptr;

		/// <summary>
		/// the used config
		/// </summary>
		public WrenConfig Config { get; }

		/// <summary>
		/// is the vm currently alive
		/// </summary>
		public bool IsAlive => Ptr != IntPtr.Zero;

		#region Lifetime

		/// <summary>
		/// create a new vm with all default values 
		/// </summary>
		public WrenVm() : this(null) { }

		/// <summary>
		/// create a new vm with provided settings and bindings. 
		///
		/// </summary>
		/// <param name="wrenConfig"></param>
		public WrenVm(WrenConfig wrenConfig)
		{
			Wren.Initialize();

			Config = wrenConfig ?? new WrenConfig();
			Cache = new WrenCache();

			var interopConfiguration = new InteropWrenConfiguration
			{
				InitialHeapSize = new UIntPtr(Config.InitialHeapSize),
				MinHeapSize = new UIntPtr(Config.MinHeapSize),
				HeapGrowthPercent = Config.HeapGrowthPercent,
				WriteFn = Wren.OnWrenWrite,
				ErrorFn = Wren.OnWrenError,
				ResolveModuleFn = Wren.OnWrenResolveModule,
				LoadModuleFn = Wren.OnWrenLoadModule,
				BindForeignMethodFn = Wren.OnWrenBindForeignMethod,
				BindForeignClassFn = Wren.OnWrenBindForeignClass,
			};

			Ptr = WrenImport.wrenNewVM(interopConfiguration);
			WrenCache.AddVm(Ptr, this);
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

			var handles = new Dictionary<IntPtr, WrenHandle>(Cache.Handles);
			foreach (var pair in handles)
			{
				pair.Value.Free(this);
			}

			Cache.Handles.Clear();

			WrenImport.wrenFreeVM(Ptr);
			WrenCache.RemoveVm(Ptr);

			Ptr = IntPtr.Zero;
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
			var handle = new WrenSignatureHandle(this, handlePtr);
			Cache.Handles.Add(handlePtr, handle);
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
			IntPtr intPtr = WrenImport.wrenGetSlotString(Ptr, slot);
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

		public IWrenSlot GetSlot(int slot)
		{
			return new WrenSlot(slot) {Vm = this};
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
			var handle = new WrenHandle(this, handlePtr);
			Cache.Handles.Add(handlePtr, handle);
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
		public WrenForeignObject<T> SetSlotNewForeign<T>(int slot, int classSlot, T data = default)
		{
			this.AssertSlot(slot);
			this.AssertSlot(classSlot);

			IntPtr ptr = WrenImport.wrenSetSlotNewForeign(Ptr, slot, classSlot, new IntPtr(IntPtr.Size));

			var wrenForeignObject = new WrenForeignObject<T>(ptr, data);
			Cache.AddForeignObject(ptr, wrenForeignObject);
			Marshal.WriteIntPtr(ptr, Ptr);

			return wrenForeignObject;
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
			return Cache.GetForeignById(ptr);
		}

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
