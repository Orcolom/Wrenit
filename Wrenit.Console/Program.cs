using System;
using System.Runtime.InteropServices;
using Wrenit;

namespace Wrenit.Consoles
{
	internal class Program
	{
		private static string _script = @"
		import ""c"" for ClassA, ClassB
System.write(""test"")
System.print(ClassA.Two())
System.print(ClassB.One(1,2,3))
		";

		public static void One(WrenitVM vm)
		{
			Console.WriteLine("One");
		}

		public static void Two(WrenitVM vm)
		{
			Console.WriteLine("Two");
		}

		public static void Main(string[] args)
		{
			WrenitModule wrenitModule = new WrenitModule("const",
				@"
					class ClassA {
						foreign static Two()
					}

					foreign class ClassB {
						foreign One(a,b,c)
					}
				",
				new WrenitClass("ClassA", null, null,
					new WrenitMethod(WrenitSignature.Method("Two"), true, Two)
				),
				new WrenitClass("ClassB", null, null,
					new WrenitMethod(WrenitSignature.Method("One", 3), true, One)
				)
			);
			
			WrenitConfig config = WrenitConfig.GetDefaults();
			config.ErrorHandler += Vm_ErrorEvent;
			config.WriteHandler += Vm_WriteEvent;

			config.ResolveModuleHandler += (WrenitVM _, string importer, string name) =>
			{
				if (name == "c") return "const";
				return name;
			};

			config.LoadModuleHandler += (WrenitVM __, string name) =>
			{
				if (name == "const")
				{
					return new WrenitLoadModuleResult() { Source = wrenitModule.Source };
				}
				return new WrenitLoadModuleResult();
			};

			config.BindForeignMethod += (WrenitVM _, string module, string className, bool isStatic, string signature) =>
			{
				if (wrenitModule.Name == module) 
				{
					WrenitClass @class = wrenitModule.FindClass(className);
					if (@class != null)
					{
						WrenitMethod method = @class.FindMethod(signature, isStatic);
						if (method != null) return method.Method;
					}
				}
				return null;
			};

			config.BindForeignClass += (WrenitVM _, string module, string className) =>
			{
				if (wrenitModule.Name == module)
				{
					WrenitClass @class = wrenitModule.FindClass(className);
					if (@class != null)
					{
						return @class;
					}
				}
				return null;
			};

			WrenitVM vm = new WrenitVM(config);
			vm.Interpret("main", _script);
		}

		private static void Vm_WriteEvent(WrenitVM vm, string text)
		{
			Console.Write(text);
		}

		private static void Vm_ErrorEvent(WrenitVM vm, WrenitResult result, string module, int line, string message)
		{
			switch(result)
			{
				case WrenitResult.CompileError:
					Console.Write($"[COMPLILE ERROR in {module} line [{line}] {message}");
					break;
				case WrenitResult.RuntimeError:
					Console.Write($"[RUNTIME ERROR] {message}");
					break;
				case WrenitResult.StackTrace:
					Console.Write($"[{module} line [{line}] in {message}");
					break;
			}
		}
	}
}
