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

		internal WrenForeignObject(IntPtr id) : base(id, default(T)) { }
		internal WrenForeignObject(IntPtr id, T data) : base(id, data) { }
	}

	/// <summary>
	/// A foreign object bound to a wren vm
	/// </summary>
	public class WrenForeignObject
	{
		/// <summary>
		/// id of the foreign object
		/// </summary>
		internal IntPtr Id;

		private object _data;
		
		public bool IsAlive => Id != IntPtr.Zero;

		/// <summary>
		/// the data in the foreign object
		/// </summary>
		public object Data
		{
			get
			{
				this.AssertAlive();
				return _data;
			}
			set
			{
				this.AssertAlive();
				_data = value;
			}
		}

		// ReSharper disable once UnusedMember.Local
		private WrenForeignObject() { }

		internal WrenForeignObject(IntPtr id, object data)
		{
			Id = id;
			Data = data;
		}

		public WrenForeignObject<T> As<T>()
		{
			return this as WrenForeignObject<T>;
		}
	}
}
