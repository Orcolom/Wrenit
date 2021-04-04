using System;

namespace Wrenit
{
	public sealed class WrenForeignObject<T> : WrenForeignObject
	{
		// ReSharper disable once InconsistentNaming
		private new T Obj;
		public new T Value
		{
			get => Obj;
			set => Obj = value;
		}

		internal WrenForeignObject(WrenVm vm, IntPtr id) : base(vm, id, default(T)) { }
	}

	public class WrenForeignObject : IDisposable
	{
		private readonly WeakReference<WrenVm> _vm;
		private readonly IntPtr _id;
		// ReSharper disable once MemberCanBePrivate.Global
		protected object Obj;

		public object Value
		{
			get => Obj;
			set => Obj = value;
		}

		// ReSharper disable once UnusedMember.Local
		private WrenForeignObject() { }

		internal WrenForeignObject(WrenVm vm, IntPtr id, object obj)
		{
			_vm = new WeakReference<WrenVm>(vm);
			_id = id;
			Obj = obj;
		}

		~WrenForeignObject()
		{
			Free();
		}

		public void Dispose()
		{
			Free();
			GC.SuppressFinalize(this);
		}

		private void Free()
		{
			if (_vm.TryGetTarget(out WrenVm vm))
			{
				vm.FreeForeignObject(_id);
			}
		}
	}
}
