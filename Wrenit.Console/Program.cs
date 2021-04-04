using System;
using Wren.it.Builder;

namespace Wren.it.Consoles
{
	internal class Program
	{
		private static string _script = @"
		import ""c"" for ClassA, ClassB
System.write(""test"")
System.print(ClassA.Two())
System.print(ClassB.One(1,2,3))
		";

		public static void One(WrenVm vm)
		{
			Console.WriteLine("One");
		}

		public static void Two(WrenVm vm)
		{
			Console.WriteLine("Two");
		}

		public static void Main(string[] args)
		{
			Wrenit.Initialize();
			
			WrenitModule wrenitModule = new WrenitModule("const",
				@"
					class ClassA {
						foreign static Two()
					}

					class ClassB {
						foreign static One(a,b,c)
					}
				",
				new WrenitClass("ClassA", null, null,
					new WrenitMethod(WrenitSignature.Method("Two"), true, Two)
				),
				new WrenitClass("ClassB", null, null,
					new WrenitMethod(WrenitSignature.Method("One", 3), true, One)
				)
			);
			
			WrenConfig config = WrenConfig.GetDefaults();
			config.ErrorHandler += Vm_ErrorEvent;
			config.WriteHandler += Vm_WriteEvent;

			config.ResolveModuleHandler += (WrenVm _, string importer, string name) =>
			{
				if (name == "c") return "const";
				return name;
			};

			config.LoadModuleHandler += (WrenVm __, string name) =>
			{
				if (name == "const")
				{
					return wrenitModule.Source;
				};
				return null;
			};

			config.BindForeignMethodHandler += (WrenVm _, string module, string className, bool isStatic, string signature) =>
			{
				if (wrenitModule.Name != module) return null;
				WrenitClass @class = wrenitModule.FindClass(className);
				WrenitMethod method = @class?.FindMethod(signature, isStatic);
				return method?.MethodBinding;
			};

			config.BindForeignClassHandler += (WrenVm _, string module, string className) =>
			{
				if (wrenitModule.Name == module)
				{
					WrenitClass @class = wrenitModule.FindClass(className);
					if (@class != null)
					{
						return @class.AsForeign();
					}
				}
				return null;
			};

			WrenVm vm = new WrenVm(config);
			vm.Interpret("main", _script);
		}

		private static void Vm_WriteEvent(WrenVm vm, string text)
		{
			Console.Write(text);
		}

		private static void Vm_ErrorEvent(WrenVm vm, WrenErrorType result, string module, int line, string message)
		{
			switch(result)
			{
				case WrenErrorType.CompileError:
					Console.Write($"[COMPLILE ERROR in {module} line [{line}] {message}");
					break;
				case WrenErrorType.RuntimeError:
					Console.Write($"[RUNTIME ERROR] {message}");
					break;
				case WrenErrorType.StackTrace:
					Console.Write($"[{module} line [{line}] in {message}");
					break;
			}
		}
	}
}
