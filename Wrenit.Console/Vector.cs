using System;
using Wrenit.Utilities;

namespace Wrenit.Consoles
{
	public class Vector
	{
		public double x;
		public double y;
	}

	[WrenModule(WrenSource, Name = "Math")]
	public class WrenMath
	{
		private const string WrenSource =
		@"
			foreign class Vector {
				foreign add(a,b,c)
			}
		";
			
		[WrenClass]
		public class Vector
		{
			[WrenAllocator]
			public static void Init(WrenVm vm) { }

			[WrenFinalizer]
			public static void Fin(WrenForeignObject data) { }

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
		}
	}
}
