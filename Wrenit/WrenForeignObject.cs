using System;

namespace Wrenit
{
	/// <summary>
	/// A foreign object bound to a wren vm
	/// </summary>
	public sealed class WrenForeignObject<T> : WrenForeignObject
	{
		/// <summary>
		/// get the data in the foreign object
		/// </summary>
		public T TypedData
		{
			get => (T)Data;
			set => Data = value;
		}

		internal WrenForeignObject(WrenVm vm, IntPtr id) : base(vm, id, default(T)) { }
	}

	/// <summary>
	/// A foreign object bound to a wren vm
	/// </summary>
	public class WrenForeignObject : IDisposable
	{
		/// <summary>
		/// a weak reference to the bound vm
		/// </summary>
		private readonly WeakReference<WrenVm> _vm;
		
		/// <summary>
		/// id of the foreign object
		/// </summary>
		private readonly IntPtr _id;

		public bool IsAlive => _id != IntPtr.Zero;

		/// <summary>
		/// the data in the foreign object
		/// </summary>
		public object Data { get; set; }

		// ReSharper disable once UnusedMember.Local
		private WrenForeignObject() { }

		internal WrenForeignObject(WrenVm vm, IntPtr id, object data)
		{
			_vm = new WeakReference<WrenVm>(vm);
			_id = id;
			Data = data;
		}

		~WrenForeignObject()
		{
			Free();
		}

		/// <summary>
		/// dispose and free a foreign object.
		/// Note this doesn't instantly dispose of the Data. if there are no other references it will be picked up by the GC 
		/// </summary>
		public void Dispose()
		{
			Free();
		}

		/// <summary>
		/// function that does the actual freeing
		/// </summary>
		private void Free()
		{
			if (IsAlive == false) return;

			if (_vm.TryGetTarget(out WrenVm vm))
			{
				Free(vm);
			}
		}
		
		/// <summary>
		/// function that does the actual freeing
		/// </summary>
		internal void Free(WrenVm vm)
		{
			vm.RemoveForeignObject(_id);
		}
	}
}
