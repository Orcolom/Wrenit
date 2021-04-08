using System;
using Wrenit.Utilities;

namespace Wrenit.Consoles
{
	public class Vector
	{
		public double x;
		public double y;
	}

	[WrenModule("Math")]
	public class WrenMath
	{
		[WrenRawSource]
		private static string Raw1()
		{
			return "var PI = 3.1415";
		}

		[WrenClass]
		public class Vector
		{
			[WrenAllocator]
			public static void Init(WrenVm vm)
			{
				vm.SetSlotNewForeign<Consoles.Vector>(0, 0);
			}

			[WrenFinalizer]
			public static void Fin(WrenForeignObject data) { }

			
			[WrenRawSource]
			private static string Raw1()
			{
				return @"static toString() { ""{%(x), %(y)}"" }";
			}

			[WrenMethod(MethodType.Construct, "new", 2)]
			public static void NewZero(WrenVm vm)
			{
				WrenForeignObject<Consoles.Vector> fo = vm.GetSlotForeign<Consoles.Vector>(0);
				fo.TypedData = new Consoles.Vector();
				fo.TypedData.x = vm.GetSlotDouble(1);
				fo.TypedData.y = vm.GetSlotDouble(2);
			}

			[WrenMethod(MethodType.Method, 1)]
			public static void Add(WrenVm vm)
			{
				WrenForeignObject<Consoles.Vector> fo = vm.GetSlotForeign<Consoles.Vector>(0);
				fo.TypedData.x += vm.GetSlotDouble(1);
				fo.TypedData.y += vm.GetSlotDouble(1);
			}

			[WrenMethod(MethodType.FieldGetter)]
			public static void GetX(WrenVm vm)
			{
				WrenForeignObject<Consoles.Vector> fo = vm.GetSlotForeign<Consoles.Vector>(0);
				vm.SetSlotDouble(0, fo.TypedData.x);
			}

			[WrenMethod(MethodType.FieldSetter)]
			public static void SetX(WrenVm vm)
			{
				WrenForeignObject<Consoles.Vector> fo = vm.GetSlotForeign<Consoles.Vector>(0);
				fo.TypedData.x = vm.GetSlotDouble(0);
			}

			[WrenMethod(MethodType.SubScriptGetter, "x")]
			public static void Subscript(WrenVm vm)
			{
				WrenForeignObject<Consoles.Vector> fo = vm.GetSlotForeign<Consoles.Vector>(0);
				int index = (int)vm.GetSlotDouble(1);
				if (index == 0) vm.SetSlotDouble(0, fo.TypedData.x);
			}
		}
	}
}
