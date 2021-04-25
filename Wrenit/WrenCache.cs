using System;
using System.Collections.Generic;
using System.Data;
using Wrenit.Interop;

namespace Wrenit
{
	public class WrenCache
	{
		#region VM

		/// <summary>
		/// list of all active vm's
		/// </summary>
		private static readonly Dictionary<IntPtr, WeakReference<WrenVm>> VmList =
			new Dictionary<IntPtr, WeakReference<WrenVm>>();

		internal static WeakReference<WrenVm> AddVm(IntPtr ptr, WrenVm vm)
		{
			var @ref = new WeakReference<WrenVm>(vm);
			VmList.Add(ptr, @ref);
			return @ref;
		}
		
		internal static void RemoveVm(IntPtr ptr)
		{
			VmList.Remove(ptr);
		}
		
		internal static WrenVm GetVm(IntPtr ptr)
		{
			if (VmList.TryGetValue(ptr, out var weakRef) == false) return null;
			return weakRef.TryGetTarget(out WrenVm vm) ? vm : null;
		}

		#endregion
		
		#region Foreign Methods

		/// <summary>
		/// foreign bound methods
		/// </summary>
		private readonly Dictionary<IntPtr, WrenForeignMethod>
			_wrenForeignMethods = new Dictionary<IntPtr, WrenForeignMethod>();

		internal IntPtr GetNewForeignMethodId(WrenForeignMethod wrenForeignMethod)
		{
			var count = new IntPtr(_wrenForeignMethods.Count);
			_wrenForeignMethods.Add(count, wrenForeignMethod);
			return count;
		}

		internal WrenForeignMethod GetForeignMethodById(IntPtr id)
		{
			return _wrenForeignMethods.TryGetValue(id, out var value) ? value : null;
		}
		
		#endregion

			
		#region Foreign Class

		/// <summary>
		/// foreign bound methods
		/// </summary>
		private readonly Dictionary<IntPtr, WrenForeignClass>
			_wrenForeignClasses = new Dictionary<IntPtr, WrenForeignClass>();

		internal IntPtr GetNewForeignClassId(WrenForeignClass wrenForeignClass)
		{
			var count = new IntPtr(_wrenForeignMethods.Count + 1);
			_wrenForeignClasses.Add(count, wrenForeignClass);
			return count;
		}

		internal WrenForeignClass GetForeignClassById(IntPtr id)
		{
			return _wrenForeignClasses.TryGetValue(id, out var value) ? value : null;
		}
		
		#endregion


		#region Handles

		/// <summary>
		/// list of handles created by the vm. They dont get garbage collected without disposing them 
		/// </summary>
		internal readonly Dictionary<IntPtr, WrenHandle> Handles = new Dictionary<IntPtr, WrenHandle>();


		#endregion

		public WrenSignatureHandle AddHandle(WrenVm wrenVm, IntPtr handlePtr)
		{
			throw new NotImplementedException();
		}


		#region Foreign Objects

		/// <summary>
		/// list of foreign objects created by the vm. They dont get garbage collected without disposing them 
		/// </summary>
		private readonly Dictionary<IntPtr, WrenForeignObject>
			_foreignObjects = new Dictionary<IntPtr, WrenForeignObject>();

		/// <summary>
		/// remove a foreign object from this vm's list.
		/// Note this does not free the object
		/// </summary>
		/// <param name="id">pointer of the object</param>
		internal void AddForeignObject(IntPtr id, WrenForeignObject fo)
		{
			_foreignObjects.Add(id, fo);
		}
		
		/// <summary>
		/// remove a foreign object from this vm's list.
		/// Note this does not free the object
		/// </summary>
		/// <param name="id">pointer of the object</param>
		internal void RemoveForeignObject(IntPtr id)
		{
			if (_foreignObjects.ContainsKey(id) == false) return;

			_foreignObjects.Remove(id);
		}

		/// <summary>
		/// get foreign object by its id
		/// </summary>
		/// <param name="id">id of the foreign object</param>
		/// <returns>returns object if found</returns>
		internal WrenForeignObject GetForeignById(IntPtr id)
		{
			return _foreignObjects.ContainsKey(id) == false ? null : _foreignObjects[id];
		}

		#endregion
	}
}
