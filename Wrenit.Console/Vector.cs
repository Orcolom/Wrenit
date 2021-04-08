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
		private const string WrenSource =
		@"
			foreign class Vector {
				foreign add(a,b,c)
			}
		";

		[WrenRawSource]
		private static string Raw1()
		{
			return "var hello = \"hello\"";
		}

		[WrenClass]
		public class Vector
		{
			[WrenAllocator]
			public static void Init(WrenVm vm) { }

			[WrenFinalizer]
			public static void Fin(WrenForeignObject data) { }

			
			[WrenRawSource]
			private static string Raw1()
			{
				return " static Two(a,b) { 2 } ";
			}

			[WrenMethod(MethodType.Construct, 1)]
			public static void Con(WrenVm vm) { }
			
			[WrenMethod(MethodType.Method, "Add", 1)]
			public static void AddNumber(WrenVm vm) { }

			[WrenMethod(MethodType.Method, argumentCount: 3)]
			public static void Modulo(WrenVm vm) { }

			[WrenMethod(MethodType.StaticMethod)]
			public static void Zero(WrenVm vm) { }

			[WrenMethod(MethodType.FieldGetter, "x")]
			public static void GetX(WrenVm vm) { }

			[WrenMethod(MethodType.FieldSetter, "x")]
			public static void SetX(WrenVm vm) { }
			
			[WrenMethod(MethodType.Inverse)]
			public static void Inv(WrenVm vm) { }
			
			[WrenMethod(MethodType.SubScriptGetter, argumentCount: 2)]
			public static void Subscript(WrenVm vm) { }
			
			[WrenMethod(MethodType.SubScriptSetter, argumentCount: 2)]
			public static void SubscriptX(WrenVm vm) { }
			
			[WrenMethod(MethodType.Plus)]
			public static void Plus(WrenVm vm) { }
		}
	}
}
