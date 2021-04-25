using System;
using System.Runtime.InteropServices;

// all code related to and only accessible for pinvoke
namespace Wrenit.Interop
{
	internal static class WrenImport
	{
		// Helper Info:
		// | c			          | c#			 | remark
		// |------------------|----------|------------- ----  ---  --   -
		// | size_t           | UIntPtr  | 
		// | char*            | string   | [MarshalAs(UnmanagedType.LPStr)]
		// | bool             | bool     | [MarshalAs(UnmanagedType.I1)]
		// | <struct>*        | <class>  | 
		// | <struct>         | <class>  |
		// | return <struct>  | <struct> | when struct needs to be returned all internals need to be a bittable  
		// | <any>*           | IntPtr   |
		// |				          |					 |


		#region Imports

		
		/// <summary>
		/// Get wren version
		/// </summary>
		[DllImport(Wren.DllName)]
		internal static extern int wrenGetVersionNumber();
		
		/// <summary>
		/// Get default initialized WrenConfig data
		/// </summary>
		/// <param name="configuration">interop config filled with the default values</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenInitConfiguration([Out] InteropWrenConfiguration configuration);

		/// <summary>
		/// Creates a new Wren virtual machine using the given <paramref name="configuration"/>.
		/// If <paramref name="configuration"/> is `null`, uses a default configuration created by <see cref="wrenInitConfiguration"/>.
		/// </summary>
		/// <param name="configuration">config with bindings and settings</param>
		/// <returns>pointer to c vm</returns>
		[DllImport(Wren.DllName)]
		internal static extern IntPtr wrenNewVM(InteropWrenConfiguration configuration);

		/// <summary>
		/// Disposes of all resources is use by <paramref name="vm"/>,
		/// which was previously created by a call to <see cref="wrenNewVM"/>.
		/// </summary>
		/// <param name="vm">pointer of the vm to free</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenFreeVM(IntPtr vm);

		/// <summary>
		/// Immediately run the garbage collector to free unused memory.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenCollectGarbage(IntPtr vm);

		/// <summary>
		/// Runs <paramref name="source"/>, a string of Wren source code in a new fiber in <paramref name="vm"/> in the context of resolved <paramref name="module"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module name</param>
		/// <param name="source">module source</param>
		/// <returns>interpret result</returns>
		[DllImport(Wren.DllName)]
		internal static extern WrenInterpretResult wrenInterpret(IntPtr vm, string module, string source);

		/// <summary>
		/// Creates a handle that can be used to invoke a method with <paramref name="signature"/> on
		/// using a receiver and arguments that are set up on the stack.
		///
		/// <para>
		/// 	This handle can be used repeatedly to directly invoke that method from C code using <see cref="wrenCall(IntPtr,IntPtr)"/>.
		/// </para>
		///
		/// <para>
		///		When you are done with this handle, it must be released using <see cref="wrenReleaseHandle(IntPtr,IntPtr)"/>.
		/// </para>
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="signature">method signature</param>
		/// <returns>pointer to handle</returns>
		[DllImport(Wren.DllName)]
		internal static extern IntPtr wrenMakeCallHandle(IntPtr vm, [MarshalAs(UnmanagedType.LPStr)] string signature); 

		/// <summary>
		/// Calls <paramref name="method"/>, using the receiver and arguments previously set up on the stack.
		///
		/// <para>
		/// 	<paramref name="method"/> must have been created by a call to <see cref="wrenMakeCallHandle"/>. The
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
		/// <param name="vm">pointer to c vm</param>
		/// <param name="method">pointer to c handle</param>
		/// <returns>interpret result</returns>
		[DllImport(Wren.DllName)]
		internal static extern WrenInterpretResult wrenCall(IntPtr vm, IntPtr method); 
		
		/// <summary>
		/// Releases the reference stored in <paramref name="handle"/>. After calling this, <paramref name="handle"/> can
		/// no longer be used.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="handle"></param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenReleaseHandle(IntPtr vm, IntPtr handle);

		/// <summary>
		/// Returns the number of slots available to the current foreign method.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <returns>slot count</returns>		
		[DllImport(Wren.DllName)]
		internal static extern int wrenGetSlotCount(IntPtr vm);

		/// <summary>
		/// Ensures that the foreign method stack has at least <paramref name="slots"/> available for
		/// use, growing the stack if needed.
		///
		/// Does not shrink the stack if it has more than enough slots.
		///
		/// It is an error to call this from a finalizer.
		/// </summary>
		/// <param name="vm"></param>
		/// <param name="slots"></param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenEnsureSlots(IntPtr vm, int slots);

		/// <summary>
		/// Gets the type of the object in <paramref name="slot"/>
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot index</param>
		/// <returns>resolved type</returns>
		[DllImport(Wren.DllName)]
		internal static extern WrenValueType wrenGetSlotType(IntPtr vm, int slot);

		/// <summary>
		/// Reads a boolean value from <paramref name="slot"/>.
		/// It is an error to call this if the slot does not contain a boolean value.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(Wren.DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenGetSlotBool(IntPtr vm, int slot);

		/// <summary>
		/// Stores the boolean <paramref name="value"/> in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store int</param>
		/// <param name="value">value to store</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotBool(IntPtr vm, int slot, bool value);

		/// <summary>
		/// Reads a byte array from <paramref name="slot"/>.
		///
		/// <para>
		/// 	The memory for the returned string is owned by Wren. You can inspect it
		/// 	while in your foreign method, but cannot keep a pointer to it after the
		/// 	function returns, since the garbage collector may reclaim it.
		/// </para>
		///
		/// <para>
		/// 	Returns a pointer to the first byte of the array and fill [length] with the
		/// 	number of bytes in the array.
		/// </para>
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		/// <param name="length">number of bytes</param>
		[DllImport(Wren.DllName)]
		internal static extern IntPtr wrenGetSlotBytes(IntPtr vm, int slot, [Out] out int length);
		
		/// <summary>
		/// Stores the array <paramref name="length"/> of <paramref name="bytes"/> in <paramref name="slot"/>.
		///
		/// The bytes are copied to a new string within Wren's heap, so you can free
		/// memory used by them after this is called.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		/// <param name="bytes">bytes to store</param>
		/// <param name="length">bytes length</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotBytes(IntPtr vm, int slot, IntPtr bytes, UIntPtr length);
		
		/// <summary>
		/// Reads a number from <paramref name="slot"/>.
		///
		/// It is an error to call this if the slot does not contain a number.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(Wren.DllName)]
		internal static extern double wrenGetSlotDouble(IntPtr vm, int slot);

		/// <summary>
		/// Stores the numeric <paramref name="value"/> in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		/// <param name="value">value to store</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotDouble(IntPtr vm, int slot, double value);
		
		/// <summary>
		/// Creates a new instance of the foreign class stored in <paramref name="classSlot"/> with <paramref name="size"/>
		/// bytes of raw storage and places the resulting object in <paramref name="slot"/>.
		///
		/// <para>
		/// 	This does not invoke the foreign class's constructor on the new instance. If
		/// 	you need that to happen, call the constructor from Wren, which will then
		/// 	call the allocator foreign method. In there, call this to create the object
		/// 	and then the constructor will be invoked when the allocator returns.
		/// </para>
		///
		/// Returns a pointer to the foreign object's data.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		/// <param name="classSlot">slot to store in</param>
		/// <param name="size">size of foreign data</param>
		[DllImport(Wren.DllName)]
		internal static extern IntPtr wrenSetSlotNewForeign(IntPtr vm, int slot, int classSlot, IntPtr size);

		
		/// <summary>
		/// Reads a foreign object from <paramref name="slot"/> and returns a pointer to the foreign data
		/// stored with it.
		///
		/// It is an error to call this if the slot does not contain an instance of a
		/// foreign class.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(Wren.DllName)]
		internal static extern IntPtr wrenGetSlotForeign(IntPtr vm, int slot);

		/// <summary>
		/// Stores the string <paramref name="text"/> in <paramref name="slot"/>
		///
		/// <para>
		/// 	The <paramref name="text"/> is copied to a new string within Wren's heap, so you can free
		/// 	memory used by it after this is called. The length is calculated using
		/// 	[strlen()]. If the string may contain any null bytes in the middle, then you
		/// 	should use <see cref="wrenSetSlotBytes"/> instead.
		/// </para>
		/// </summary>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotString(IntPtr vm, int slot, 
			[MarshalAs(UnmanagedType.LPStr)] string text);

		/// <summary>
		/// Reads a string from <paramref name="slot"/>.
		///
		/// The memory for the returned string is owned by Wren. You can inspect it
		/// while in your foreign method, but cannot keep a pointer to it after the
		/// function returns, since the garbage collector may reclaim it.
		///
		/// It is an error to call this if the slot does not contain a string.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(Wren.DllName)]
		internal static extern IntPtr wrenGetSlotString(IntPtr vm, int slot);

		/// <summary>
		/// Stores null in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotNull(IntPtr vm, int slot);
		
		/// <summary>
		/// Creates a handle for the value stored in <paramref name="slot"/>.
		///
		/// This will prevent the object that is referred to from being garbage collected
		/// until the handle is released by calling <see cref="wrenReleaseHandle(IntPtr,IntPtr)"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to get</param>
		[DllImport(Wren.DllName)]
		internal static extern IntPtr wrenGetSlotHandle(IntPtr vm, int slot);


		/// <summary>
		/// Stores the value captured in <paramref name="handle"/> in <paramref name="slot"/>.
		///
		/// This does not release the handle for the value.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		/// <param name="handle">pointer of handle to store</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotHandle(IntPtr vm, int slot, IntPtr handle);

		/// <summary>
		/// Stores a new empty list in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store in</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotNewList(IntPtr vm, int slot);
		
		/// <summary>
		/// Returns the number of elements in the list stored in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot get from</param>
		/// <returns>count of list elements</returns>
		[DllImport(Wren.DllName)]
		internal static extern int wrenGetListCount(IntPtr vm, int slot);

		/// <summary>
		/// Sets the value stored at <paramref name="index"/> in the list at <paramref name="listSlot"/>, 
		/// to the value from <paramref name="elementSlot"/>. 
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index in the list</param>
		/// <param name="elementSlot">slot of value to store in list</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetListElement(IntPtr vm, int listSlot, int index, int elementSlot);

		/// <summary>
		/// Reads element <paramref name="index"/> from the list in <paramref name="listSlot"/> and stores it in <paramref name="elementSlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index in the list</param>
		/// <param name="elementSlot">slot to store the value in</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenGetListElement(IntPtr vm, int listSlot, int index, int elementSlot);

		/// <summary>
		/// Takes the value stored at <paramref name="elementSlot"/> and inserts it into the list stored
		/// at <paramref name="listSlot"/> at <paramref name="index"/>.
		///
		/// <para>
		/// 	As in Wren, negative indexes can be used to insert from the end. To append an element, use `-1` for the index.
		/// </para>
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="listSlot">slot where the list is</param>
		/// <param name="index">index to store element</param>
		/// <param name="elementSlot">slot of value to store in list</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenInsertInList(IntPtr vm, int listSlot, int index, int elementSlot);

		/// <summary>
		/// Stores a new empty map in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to store</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetSlotNewMap(IntPtr vm, int slot);
		
		/// <summary>
		/// Returns the number of entries in the map stored in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot to look at</param>
		/// <returns>count of entries</returns>
		[DllImport(Wren.DllName)]
		internal static extern int wrenGetMapCount(IntPtr vm, int slot);
		
		/// <summary>
		/// Returns true if the key in <paramref name="keySlot"/> is found in the map placed in <paramref name="mapSlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of value to check exists</param>
		[DllImport(Wren.DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenGetMapContainsKey(IntPtr vm, int mapSlot, int keySlot);

		/// <summary>
		/// Retrieves a value with the key in <paramref name="keySlot"/> from the map in <paramref name="mapSlot"/> and
		/// stores it in <paramref name="valueSlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of the key</param>
		/// <param name="valueSlot">slot to store the value in</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenGetMapValue(IntPtr vm, int mapSlot, int keySlot, int valueSlot);
		
		/// <summary>
		/// Takes the value stored at <paramref name="valueSlot"/> and inserts it into the map stored
		/// at <paramref name="mapSlot"/> with key <paramref name="keySlot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where map is</param>
		/// <param name="keySlot">slot of the key to store in</param>
		/// <param name="valueSlot">slot of the value to store</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenSetMapValue(IntPtr vm, int mapSlot, int keySlot, int valueSlot);
		
		/// <summary>
		/// Removes a value from the map in <paramref name="mapSlot"/>, with the key from <paramref name="keySlot"/>,
		/// and place it in <paramref name="removedValueSlot"/>. If not found, <paramref name="removedValueSlot"/> is
		/// set to null, the same behaviour as the Wren Map API.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="mapSlot">slot where the map is</param>
		/// <param name="keySlot">slot of the key to remove</param>
		/// <param name="removedValueSlot">slot to store value that was removed</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenRemoveMapValue(IntPtr vm, int mapSlot, int keySlot, int removedValueSlot);

		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/> and stores
		/// it in <paramref name="slot"/>.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module to check in</param>
		/// <param name="name">name to get</param>
		/// <param name="slot">slot to store in</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenGetVariable(IntPtr vm, 
			[MarshalAs(UnmanagedType.LPStr)]string module, 
			[MarshalAs(UnmanagedType.LPStr)]string name, int slot);
		
		/// <summary>
		/// Looks up the top level variable with <paramref name="name"/> in resolved <paramref name="module"/>, 
		/// returns false if not found. The module must be imported at the time, 
		/// use <see cref="wrenHasModule(IntPtr,string)"/>  to ensure that before calling.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="module">module to check in</param>
		/// <param name="name">name to check for</param>
		[DllImport(Wren.DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenHasVariable(IntPtr vm, 
			[MarshalAs(UnmanagedType.LPStr)]string module, 
			[MarshalAs(UnmanagedType.LPStr)]string name);
		
		/// <summary>
		/// Returns true if <paramref name="module"/> has been imported/resolved before, false if not.
		/// </summary>
		/// <param name="vm">pointer to vm</param>
		/// <param name="module">module to check</param>
		[DllImport(Wren.DllName)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool wrenHasModule(IntPtr vm, 
			[MarshalAs(UnmanagedType.LPStr)]string module);

		/// <summary>
		/// Sets the current fiber to be aborted, and uses the value in <paramref name="slot"/> as the runtime error object.
		/// </summary>
		/// <param name="vm">pointer to c vm</param>
		/// <param name="slot">slot for the runtime error</param>
		[DllImport(Wren.DllName)]
		internal static extern void wrenAbortFiber(IntPtr vm, int slot);

		#endregion
	}

	#region Types/Delegates

	// A generic allocation function that handles all explicit memory management
	// used by Wren. It's used like so:
	//
	// - To allocate new memory, [memory] is NULL and [newSize] is the desired
	//   size. It should return the allocated memory or NULL on failure.
	//
	// - To attempt to grow an existing allocation, [memory] is the memory, and
	//   [newSize] is the desired size. It should return [memory] if it was able to
	//   grow it in place, or a new pointer if it had to move it.
	//
	// - To shrink memory, [memory] and [newSize] are the same as above but it will
	//   always return [memory].
	//
	// - To free memory, [memory] will be the memory to free and [newSize] will be
	//   zero. It should return NULL.
	internal delegate IntPtr InteropWrenReallocateFn(IntPtr memory, UIntPtr newSize, IntPtr userData);
	
	/// <summary>
	/// display text to the user
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="text">text to display</param>
	internal delegate void InteropWrenWrite(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string text);

	/// <summary>
	/// Reports an error to the user.
	///
	/// <para>
	/// 	An error detected during compile time is reported by calling this once with
	/// 	<paramref name="type"/> <see cref="F:WrenErrorType.CompileError"/>,
	///		the resolved name of the <paramref name="module"/> and <paramref name="line"/>
	/// 	where the error occurs, and the compiler's error <paramref name="message"/>.
	/// </para>
	///
	/// <para>
	/// 	A runtime error is reported by calling this once with <paramref name="type"/> <see cref="F:WrenErrorType.RuntimeError"/>,
	/// 	no <paramref name="module"/> or <paramref name="line"/>, and the runtime error's <paramref name="message"/>.
	/// 	After that, a series of <paramref name="type"/> <see cref="F:WrenErrorType.StackTrace"/> calls are
	/// 	made for each line in the stack trace. Each of those has the resolved
	/// 	<paramref name="module"/> and <paramref name="line"/> where the method or function is defined
	///		and <paramref name="message"/> is the name of the method or function.
	/// </para>
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="type">error type</param>
	/// <param name="module">module name</param>
	/// <param name="line">line position</param>
	/// <param name="message">the message</param>
	internal delegate void InteropWrenError(IntPtr vm,
		WrenErrorType type,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		int line,
		[MarshalAs(UnmanagedType.LPStr)] string message);

	/// <summary>
	///	<para>
	///		The callback Wren uses to resolve a module name.
	/// </para>
	///
	/// <para>
	/// 	Some host applications may wish to support "relative" imports, where the
	/// 	meaning of an import string depends on the module that contains it. To
	/// 	support that without baking any policy into Wren itself, the VM gives the
	/// 	host a chance to resolve an import string.
	/// </para>
	///
	/// <para>
	/// 	Before an import is loaded, it calls this, passing in the name of the
	/// 	module that contains the import and the import string. The host app can
	/// 	look at both of those and produce a new "canonical" string that uniquely
	/// 	identifies the module. This string is then used as the name of the module
	/// 	going forward. It is what is passed to <see cref="F:InterlopWrenConfiguration.LoadModuleFn"/>, how duplicate
	/// 	imports of the same module are detected, and how the module is reported in
	/// 	stack traces.
	/// </para>
	///
	/// <para>
	/// 	If you leave this function null, then the original import string is
	/// 	treated as the resolved string.
	/// </para>
	///
	/// <para>
	/// 	If an import cannot be resolved by the embedder, it should return null and
	/// 	Wren will report that as a runtime error.
	/// </para>
	///
	/// <para>
	/// 	Wren will take ownership of the string you return and free it for you, so
	/// 	it should be allocated using the same allocation function you provide above,
	///		or accessible via <see cref="WrenImport.wrenReallocate"/>
	/// </para>
	/// 
	/// </summary>
	internal delegate IntPtr InteropWrenResolveModule(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string importer,
		IntPtr name);

	/// <summary>
	/// Loads and returns the source code for the module <param name="name"/>
	/// </summary>
	/// <param name="vm">pointer to the c vm</param>
	/// <param name="name">name of the module</param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate InteropWrenLoadModuleResult InteropWrenLoadModule(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string name);

	/// <summary>
	/// Called after <see cref="F:InterlopWrenConfiguration.LoadModuleFn"/> is called for module <param name="name"/>.
	/// The original returned result is handed back to you in this callback, so that you can free memory if appropriate.
	/// </summary>
	/// <param name="vm">pinter to the c vm</param>
	/// <param name="name">name of the module</param>
	/// <param name="result">result created by <see cref="F:InterlopWrenConfiguration.LoadModuleFn"/></param>
	internal delegate void InteropWrenLoadModuleComplete(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string name,
		InteropWrenLoadModuleResult result);

	/// <summary>
	/// Returns a pointer to a foreign method on <paramref name="className"/> in <paramref name="module"/> with <paramref name="signature"/>.
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="module">module name</param>
	/// <param name="className">class name</param>
	/// <param name="isStatic">is function static</param>
	/// <param name="signature">function signature</param>
	internal delegate InteropBindForeignMethodResult InteropWrenBindForeignMethod(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		[MarshalAs(UnmanagedType.LPStr)] string className,
		[MarshalAs(UnmanagedType.I1)] bool isStatic,
		[MarshalAs(UnmanagedType.LPStr)] string signature);

	/// <summary>
	/// Returns a pair of pointers to the foreign methods used to allocate and
	/// finalize the data for instances of <paramref name="className"/> in resolved <paramref name="module"/>.
	/// </summary>
	/// <param name="vm">pointer to c vm</param>
	/// <param name="module">module name</param>
	/// <param name="className">class name</param>
	internal delegate InteropWrenForeignClassMethods InteropWrenBindForeignClass(IntPtr vm,
		[MarshalAs(UnmanagedType.LPStr)] string module,
		[MarshalAs(UnmanagedType.LPStr)] string className);

	/// <summary>
	/// A function callable from Wren code, but implemented in C#.
	/// </summary>
	/// <param name="vm"></param>
	internal delegate void InteropWrenForeignMethod(IntPtr vm, IntPtr userData);

	/// <summary>
	/// A function callable from Wren code, but implemented in C#.
	/// </summary>
	/// <param name="vm"></param>
	internal delegate void InteropWrenForeignFinalizer(IntPtr data, IntPtr userData);

	internal struct InteropWrenForeignClassMethods
	{
		/// <summary>
		/// The callback invoked when the foreign object is created.
		///
		/// This must be provided. Inside the body of this,
		/// it must call <see cref="WrenImport.wrenSetSlotNewForeign"/> exactly once.
		/// </summary>
		public IntPtr AllocateFn;


		/// <summary>
		/// user data bound to allocatFn
		/// </summary>
		public IntPtr UserData;

		/// <summary>
		/// The callback invoked when the garbage collector is about to collect a foreign object's memory.
		/// This may be `null` if the foreign class does not need to finalize.
		/// </summary>
		public IntPtr FinalizeFn;
	}

	/// <summary>
	/// result from <see cref="F:InterlopWrenConfiguration.LoadModuleFn"/> call
	/// </summary>
	internal struct InteropWrenLoadModuleResult
	{
		/// <summary>
		/// Source code of the module
		/// </summary>
		public IntPtr Source;

		/// <summary>
		/// an optional callback that will be called once Wren is done with the result.
		/// </summary>
		public IntPtr OnComplete;

		public IntPtr UserData;
	}

	/// <summary>
	/// interop struct for WrenConfiguration
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct InteropBindForeignMethodResult
	{
		public IntPtr ExecuteFn;
		public IntPtr UserData;
	}
	

	/// <summary>
	/// interop struct for WrenConfiguration
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal class InteropWrenConfiguration
	{
		/// <summary>
		///		The callback Wren will use to allocate, reallocate, and deallocate memory.
		///		If `null`, defaults to a built-in function that uses `realloc` and `free`.
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InteropWrenReallocateFn ReallocateFn;

		/// <inheritdoc cref="InteropWrenResolveModule"/>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InteropWrenResolveModule ResolveModuleFn;

		/// <summary>
		/// The callback Wren uses to load a module.
		///
		/// <para>
		/// 	Since Wren does not talk directly to the file system, it relies on the
		/// 	embedder to physically locate and read the source code for a module. The
		/// 	first time an import appears, Wren will call this and pass in the name of
		/// 	the module being imported. The VM should return the source code for that
		/// 	module. Memory for the source should be allocated using <see cref="ReallocateFn"/> and
		/// 	Wren will take ownership over it.
		/// </para>
		///
		/// <para>
		/// 	This will only be called once for any given module name. Wren caches the
		/// 	result internally so subsequent imports of the same module will use the
		/// 	previous source and not call this.
		/// </para>
		///
		/// <para>
		/// 	If a module with the given name could not be found by the embedder, it
		/// 	should return NULL and Wren will report that as a runtime error.
		/// </para>
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InteropWrenLoadModule LoadModuleFn;

		/// <summary>
		///	The callback Wren uses to find a foreign method and bind it to a class.
		///
		/// <para>
		/// 	When a foreign method is declared in a class, this will be called with the
		/// 	foreign method's module, class, and signature when the class body is
		/// 	executed. It should return a pointer to the foreign function that will be
		/// 	bound to that method.
		/// </para>
		///
		/// <para>
		/// 	If the foreign function could not be found, this should return null and
		/// 	Wren will report it as runtime error.
		/// </para>
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InteropWrenBindForeignMethod BindForeignMethodFn;

		/// <summary>
		/// The callback Wren uses to find a foreign class and get its foreign methods.
		///
		/// <para>
		/// 	When a foreign class is declared, this will be called with the class's
		/// 	module and name when the class body is executed. It should return the
		/// 	foreign functions uses to allocate and (optionally) finalize the bytes
		/// 	stored in the foreign object when an instance is created.
		/// </para>
		/// </summary>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InteropWrenBindForeignClass BindForeignClassFn;

		/// <inheritdoc cref="InteropWrenWrite"/>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InteropWrenWrite WriteFn;

		/// <inheritdoc cref="InteropWrenError"/>
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InteropWrenError ErrorFn;

		/// <summary>
		///	The number of bytes Wren will allocate before triggering the first garbage collection.
		///
		/// If zero, defaults to 10MB.
		/// </summary>
		public UIntPtr InitialHeapSize;

		/// <summary>
		/// After a collection occurs, the threshold for the next collection is
		/// determined based on the number of bytes remaining in use. This allows Wren
		/// to shrink its memory usage automatically after reclaiming a large amount
		/// of memory.
		///
		/// This can be used to ensure that the heap does not get too small, which can
		/// in turn lead to a large number of collections afterwards as the heap grows
		/// back to a usable size.
		///
		/// If zero, defaults to 1MB.
		/// </summary>
		public UIntPtr MinHeapSize;

		/// <summary>
		/// Wren will resize the heap automatically as the number of bytes
		/// remaining in use after a collection changes. This number determines the
		/// amount of additional memory Wren will use after a collection, as a
		/// percentage of the current heap size.
		///
		/// For example, say that this is 50. After a garbage collection, when there
		/// are 400 bytes of memory still in use, the next collection will be triggered
		/// after a total of 600 bytes are allocated (including the 400 already in
		/// use.)
		///
		/// Setting this to a smaller number wastes less memory, but triggers more
		/// frequent garbage collections.
		///
		/// If zero, defaults to 50.
		/// </summary>
		public int HeapGrowthPercent;

		/// <summary>
		/// User-defined data associated with the VM.
		/// <remarks>
		///		Wrenit doesnt give the option to provide this. here for struct layout
		/// </remarks>
		/// </summary>
		#pragma warning disable 649
		public IntPtr UserData;
		#pragma warning restore 649
	}

	#endregion
}
