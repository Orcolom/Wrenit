using System;
using System.Collections.Generic;
using Wrenit.Utilities;

namespace Wrenit.Shared
{
	public class Vector
	{
		public double x;
		public double y;
	}

	[WrenModule("Math")]
	public class WrenMath
	{
		[WrenManualSource]
		private static string Raw1()
		{
			return "var PI = 3.1415";
		}

		[WrenClass(nameof(Vector))]
		public class VectorClass
		{
			[WrenAllocator]
			public static void Init(WrenVm vm)
			{
				vm.SetSlotNewForeign<Vector>(0, 0);
			}

			[WrenFinalizer]
			public static void Fin(WrenForeignObject data)
			{
				// nothing to destroy. the vector will get claimed by gc all on it son
			}

			[WrenManualSource]
			private static string Raw1()
			{
				return @"static toString { ""{%(x), %(y)}"" }";
			}

			[WrenMethod(WrenMethodType.Construct, "new", 2)]
			public static void NewZero(WrenVm vm)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData = new Vector
				{
					x = vm.GetSlotDouble(1),
					y = vm.GetSlotDouble(2)
				};
			}

			[WrenMethod(WrenMethodType.Method, 2)]
			public static void Add(WrenVm vm)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData.x += vm.GetSlotDouble(1);
				fo.TypedData.y += vm.GetSlotDouble(2);
			}

			[WrenMethod(WrenMethodType.Times, 1)]
			public static void Multiply(WrenVm vm)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData.x *= vm.GetSlotDouble(1);
				fo.TypedData.y *= vm.GetSlotDouble(1);
			}

			[WrenMethod(WrenMethodType.FieldGetter)]
			public static void GetX(WrenVm vm)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				vm.SetSlotDouble(0, fo.TypedData.x);
			}

			[WrenMethod(WrenMethodType.FieldSetter)]
			public static void SetX(WrenVm vm)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData.x = vm.GetSlotDouble(0);
			}

			[WrenMethod(WrenMethodType.SubScriptGetter, "x")] // names get ignored for signatures that dont need it
			public static void Subscript(WrenVm vm)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				int index = (int) vm.GetSlotDouble(1);
				if (index == 0) vm.SetSlotDouble(0, fo.TypedData.x);
			}
		}
	}
}
