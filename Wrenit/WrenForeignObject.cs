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

		public bool IsAlive => Id != IntPtr.Zero;

		/// <summary>
		/// the data in the foreign object
		/// </summary>
		public object Data { get; set; }

		// ReSharper disable once UnusedMember.Local
		private WrenForeignObject() { }

		internal WrenForeignObject(IntPtr id, object data)
		{
			Id = id;
			Data = data;
		}
	}
}
