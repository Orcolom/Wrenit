using System;
using Wrenit.Interop;

namespace Wrenit
{
	public sealed class WrenSignatureHandle : WrenHandle
	{
		internal WrenSignatureHandle(WrenVm vm, IntPtr ptr) : base(vm, ptr) {}
	}

	/// <summary>
	/// A handle to a Wren object.
	///
	/// This lets code outside of the VM hold a persistent reference to an object.
	/// After a handle is acquired, and until it is released, this ensures the
	/// garbage collector will not reclaim the object it references.
	/// </summary>
	public class WrenHandle : IDisposable
	{
		public bool IsAlive => Ptr != IntPtr.Zero;

		internal IntPtr Ptr;
		private readonly WeakReference<WrenVm> _vm;

		// ReSharper disable once UnusedMember.Local
		private WrenHandle() { }

		internal WrenHandle(WrenVm vm, IntPtr handle)
		{
			_vm = new WeakReference<WrenVm>(vm);
			Ptr = handle;
		}
		
		~WrenHandle()
		{
			Free();
		}

		public void Dispose()
		{
			Free();
			GC.SuppressFinalize(this);
		}

		public void Free(WrenVm vm)
		{
			if (IsAlive == false) return;
			vm.RemoveHandle(Ptr);
			WrenImport.wrenReleaseHandle(vm.Ptr, Ptr);
			Ptr = IntPtr.Zero;
		}

		private void Free()
		{
			if (IsAlive == false) return;

			if (_vm.TryGetTarget(out WrenVm vm))
			{
				Free(vm);
			}
		}
	}
}
