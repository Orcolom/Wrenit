using System;
using System.Runtime.InteropServices;
using Wrenit.Interop;

namespace Wrenit
{
	public interface IWrenSlot
	{
		public int Index { get; }
		public WrenValueType Type { get; }
		public IWrenListSlot AsList();
		public IWrenMapSlot AsMap();

		public bool GetBool();
		public void SetBool(bool value);
		public byte[] GetBytes();
		public void SetBytes(byte[] bytes);
		public double GetDouble();
		public void SetDouble(double value);
		public string GetString();
		public void SetString(string value);
		public void SetNull();
		public WrenHandle GetHandle();
		public void SetSlotHandle(WrenHandle handle);
		public void SetAsReturn();
		
		public IWrenListSlot SetNewList();
		public IWrenMapSlot SetNewMap();
	}

	public interface IWrenListSlot
	{
		public int Index { get; }
		public WrenValueType Type { get; }
		
		public int Count { get; }

		public void SetElement(int index, IWrenSlot element);

		public void GetElement(int index, ref IWrenSlot element);

		public void Insert(int index, IWrenSlot element);
		
		public IWrenSlot AsSlot();
		public IWrenMapSlot AsMap();
		public void SetAsReturn();
	}
	
	public interface IWrenMapSlot
	{
		public int Index { get; }
		public WrenValueType Type { get; }
		public int Count { get; }

		public bool ContainsKey(int mapSlot, IWrenSlot key);
		public void GetValue(IWrenSlot key, ref IWrenSlot value);
		public void SetValue(IWrenSlot key, IWrenSlot value);
		public void RemoveValue(IWrenSlot key, ref IWrenSlot removedValue);
		
		public IWrenSlot AsSlot();
		public IWrenListSlot AsList();
		public void SetAsReturn();
	}

	internal struct WrenSlot : IWrenSlot, IWrenListSlot, IWrenMapSlot
	{
		public int Index { get; }
		internal WrenVm Vm;

		public WrenSlot(int slotIndex)
		{
			Index = slotIndex;
			Vm = null;
		}

		/// <inheritdoc cref="WrenVm.GetSlotType"/>
		public WrenValueType Type
		{
			get
			{
				this.AssertAlive();
				return Vm.GetSlotType(Index);
			}
		}

		/// <inheritdoc cref="WrenVm.GetSlotBool"/>
		public bool GetBool()
		{
			this.AssertAlive();
			return Vm.GetSlotBool(Index);
		}

		/// <inheritdoc cref="WrenVm.SetSlotBool"/>
		public void SetBool(bool value)
		{
			this.AssertAlive();
			Vm.SetSlotBool(Index, value);
		}

		/// <inheritdoc cref="WrenVm.GetSlotBytes"/>
		public byte[] GetBytes()
		{
			this.AssertAlive();
			return Vm.GetSlotBytes(Index);
		}

		/// <inheritdoc cref="WrenVm.SetSlotBytes"/>
		public void SetBytes(byte[] bytes)
		{
			this.AssertAlive();
			Vm.SetSlotBytes(Index, bytes);
		}

		/// <inheritdoc cref="WrenVm.GetSlotDouble"/>
		public double GetDouble()
		{
			this.AssertAlive();
			return Vm.GetSlotDouble(Index);
		}

		/// <inheritdoc cref="WrenVm.SetSlotDouble"/>
		public void SetDouble(double value)
		{
			this.AssertAlive();
			Vm.SetSlotDouble(Index, value);
		}

		/// <inheritdoc cref="WrenVm.GetSlotString"/>
		public string GetString()
		{
			this.AssertAlive();
			return Vm.GetSlotString(Index);
		}

		/// <inheritdoc cref="WrenVm.SetSlotString"/>
		public void SetString(string value)
		{
			this.AssertAlive();
			Vm.SetSlotString(Index, value);
		}

		/// <inheritdoc cref="WrenVm.SetSlotNull"/>
		public void SetNull()
		{
			this.AssertAlive();
			Vm.SetSlotNull(Index);
		}

		/// <inheritdoc cref="WrenVm.GetSlotHandle"/>
		public WrenHandle GetHandle()
		{
			this.AssertAlive();
			return Vm.GetSlotHandle(Index);
		}

		/// <inheritdoc cref="WrenVm.SetSlotHandle"/>
		public void SetSlotHandle(WrenHandle handle)
		{
			this.AssertAlive();
			Vm.SetSlotHandle(Index, handle);
		}

		/// <summary>
		/// set this value as the method return value. Moves the value to slot 0
		/// </summary>
		public void SetAsReturn()
		{
			this.AssertAlive();
			using var handle = Vm.GetSlotHandle(Index);
			Vm.SetSlotHandle(0, handle);
		}

		/// <inheritdoc cref="WrenVm.SetSlotHandle"/>
		public IWrenListSlot SetNewList()
		{
			this.AssertAlive();
			Vm.SetSlotNewList(Index);
			return this;
		}

		int  IWrenListSlot.Count
		{
			get
			{
				this.AssertSlotType(WrenValueType.List);
				return Vm.GetListCount(Index);
			}
		}

		public void SetElement(int index, IWrenSlot element)
		{
			this.AssertSlotType(WrenValueType.List);
			Vm.SetListElement(Index, index, element.Index);
		}

		public void GetElement(int index, ref IWrenSlot element)
		{
			this.AssertSlotType(WrenValueType.List);
			Vm.GetListElement(Index, index, element.Index);
		}

		public void Insert(int index, IWrenSlot element)
		{
			this.AssertSlotType(WrenValueType.List);
			Vm.InsertInList(Index, index, element.Index);
		}
		
		public IWrenMapSlot SetNewMap()
		{
			this.AssertAlive();
			Vm.SetSlotNewMap(Index);
			return this;
		}

		int  IWrenMapSlot.Count
		{
			get
			{
				this.AssertSlotType(WrenValueType.Map);
				return Vm.GetListCount(Index);
			}
		}

		public bool ContainsKey(int mapSlot, IWrenSlot key)
		{
			this.AssertAlive();
			return Vm.GetMapContainsKey(Index, key.Index);
		}

		public void GetValue(IWrenSlot key, ref IWrenSlot value)
		{
			this.AssertAlive();
			Vm.GetMapValue(Index, key.Index, value.Index);
		}

		public void SetValue(IWrenSlot key, IWrenSlot value)
		{
			this.AssertAlive();
			Vm.SetMapValue(Index, key.Index, value.Index);
		}

		public void RemoveValue(IWrenSlot key, ref IWrenSlot removedValue)
		{
			this.AssertAlive();
			Vm.RemoveMapValue(Index, key.Index, removedValue.Index);
		}
		
		public WrenForeignObject<T> SetNewForeign<T>(WrenSlot @class, T data = default)
		{
			this.AssertAlive();
			return Vm.SetSlotNewForeign(Index, @class.Index, data);
		}
		
		public IWrenListSlot AsList() => this;
		public IWrenSlot AsSlot() => this;
		public IWrenMapSlot AsMap() => this;
	}
}
