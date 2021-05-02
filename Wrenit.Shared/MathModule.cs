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
	[WrenImport(typeof(ConstantsModule), "PI")]
	[WrenImport(typeof(ConstantsModule), typeof(ConstantsModule.Version))]
	[WrenImport("ConstantsModule", "Wrapper", "W")]
	public class MathModule
	{
		[WrenManualSource]
		private static string Raw1()
		{
			return "var PI2 = PI * 2";
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
				return @"toString { ""{%(x), %(y)}"" }";
			}

			[WrenMethod(WrenMethodType.Construct, "new")]
			public static void NewZero(WrenVm vm, IWrenSlot x, IWrenSlot y)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData = new Vector
				{
					x = x.GetDouble(),
					y = y.GetDouble(),
				};
			}

			[WrenMethod(WrenMethodType.Method)]
			public static void Add(WrenVm vm, IWrenSlot x, IWrenSlot y)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData.x += x.GetDouble();
				fo.TypedData.y += y.GetDouble();
			}

			[WrenMethod(WrenMethodType.Times)]
			public static void Multiply(WrenVm vm, IWrenSlot other)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData.x *= other.GetDouble();
				fo.TypedData.y *= other.GetDouble();
			}

			[WrenMethod(WrenMethodType.FieldGetter, "x")]
			public static void GetX(WrenVm vm)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				vm.SetSlotDouble(0, fo.TypedData.x);
			}

			[WrenMethod(WrenMethodType.FieldSetter, "x")]
			public static void SetX(WrenVm vm, IWrenSlot other)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				fo.TypedData.x = other.GetDouble();
			}

			[WrenMethod(WrenMethodType.SubScriptGetter, "x")] // names get ignored for signatures that dont need it
			public static void Subscript(WrenVm vm, IWrenSlot index)
			{
				var fo = vm.GetSlotForeign<Vector>(0);
				if (index.GetDouble() != 0) return;

				index.SetDouble(fo.TypedData.x);
				index.SetAsReturn();
			}
		}
	}
}
