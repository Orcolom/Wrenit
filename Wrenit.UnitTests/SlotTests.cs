using System.Text;
using NUnit.Framework;

namespace Wren.it.UnitTests
{
	[TestFixture]
	public class SlotTests
	{
		private WrenVm _vm;

		public SlotTests()
		{
			_vm = new WrenVm();
		}
		
		[Test]
		public void Count()
		{
			_vm.EnsureSlots(7);
			Assert.AreEqual(7, _vm.GetSlotCount());
		}

		[Test]
		public void Null()
		{
			_vm.EnsureSlots(1);
			_vm.SetSlotBool(0, true);
			_vm.SetSlotNull(0);
			
			Assert.AreEqual(WrenValueType.Null, _vm.GetSlotType(0));
		}
		
		[Test]
		public void Bool()
		{
			_vm.EnsureSlots(1);
			_vm.SetSlotNull(0);

			const bool wanted = true;
			_vm.SetSlotBool(0, true);
			bool value = _vm.GetSlotBool(0);

			Assert.AreEqual(wanted, value);
			Assert.AreEqual(WrenValueType.Bool, _vm.GetSlotType(0));
		}
		
		[Test]
		public void Double()
		{
			_vm.EnsureSlots(1);
			_vm.SetSlotNull(0);

			const double wanted = 10.10;
			_vm.SetSlotDouble(0, wanted);
			double value = _vm.GetSlotDouble(0);
			
			Assert.AreEqual(wanted, value);
			Assert.AreEqual(WrenValueType.Number, _vm.GetSlotType(0));
		}
		
		[Test]
		public void String()
		{
			_vm.EnsureSlots(1);
			_vm.SetSlotNull(0);

			const string wanted = "this is a string";
			_vm.SetSlotString(0, wanted);
			string value = _vm.GetSlotString(0);
			
			Assert.AreEqual(wanted, value);
			Assert.AreEqual(WrenValueType.String, _vm.GetSlotType(0));
		}
		
		[Test]
		public void Bytes()
		{
			_vm.EnsureSlots(1);
			_vm.SetSlotNull(0);

			const string wantedString = "this is a string";
			byte[] wanted = Encoding.ASCII.GetBytes(wantedString);  
			
			_vm.SetSlotBytes(0, wanted);
			byte[] valueBytes = _vm.GetSlotBytes(0);
			string valueString = _vm.GetSlotString(0);
			
			Assert.AreEqual(wanted, valueBytes);
			Assert.AreEqual(wantedString, valueString);
			Assert.AreEqual(WrenValueType.String, _vm.GetSlotType(0));
		}
		
		[Test]
		public void Handle()
		{
			_vm.EnsureSlots(1);
			_vm.SetSlotNull(0);

			const double wanted = 10.10;
			
			_vm.SetSlotDouble(0, wanted);
			
			WrenHandle handle = _vm.GetSlotHandle(0);
			_vm.SetSlotNull(0);
			_vm.SetSlotHandle(0, handle);
			
			double value = _vm.GetSlotDouble(0);
			
			Assert.AreEqual(wanted, value);
			Assert.AreEqual(WrenValueType.Number, _vm.GetSlotType(0));
		}
		
		//[Test]
		//public void Foreign()
		//{
		//	_vm.Interpret("main", @"
		//		foreign class ClassA {{
		//			foreign construct new()
		//		}}
		//		ClassA.new()
		//	");
		//	_vm.EnsureSlots(1);
		//	_vm.SetSlotNull(0);
		//
		//	_vm.SetSlotNewForeign<SlotTests>(0, 0);
		//	
		//	WrenForeignObject foreignObject = _vm.GetSlotForeign<SlotTests>(0);
		//	Assert.AreEqual(this, foreignObject.Value);
		//	Assert.AreEqual(WrenValueType.Foreign, _vm.GetSlotType(0));
		//}
		
		[Test]
		public void List()
		{
			_vm.EnsureSlots(2);
			_vm.SetSlotNull(0);

			const bool wanted = true; 
			_vm.SetSlotNewList(0);
			_vm.SetSlotBool(1, wanted);
			_vm.InsertInList(0, -1, 1);
			_vm.SetSlotNull(1);
			_vm.GetListElement(0, 0, 1);
			
			Assert.AreEqual(wanted , _vm.GetSlotBool(1));
			Assert.AreEqual(WrenValueType.List, _vm.GetSlotType(0));
			Assert.AreEqual(1, _vm.GetListCount(0));
		}
		
		[Test]
		public void Map()
		{
			_vm.EnsureSlots(3);
			_vm.SetSlotNull(0);
		
			const string key = "works?"; 
			const bool wanted = true;
			_vm.SetSlotNewMap(0);
			_vm.SetSlotString(1, key);
			_vm.SetSlotBool(2, wanted);
			_vm.SetMapValue(0, 1, 2);

			_vm.SetSlotNull(2);

			_vm.GetMapValue(0, 1, 2);

			Assert.AreEqual(wanted , _vm.GetSlotBool(2));
			Assert.AreEqual(WrenValueType.Map, _vm.GetSlotType(0));
			Assert.AreEqual(1, _vm.GetMapCount(0));
		}
	}
}
