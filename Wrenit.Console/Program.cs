using System;
using System.Runtime.InteropServices;
using Wrenit;

namespace Wrenit.Consoles
{
	internal class Program
	{
		private static string _script = @"
		import ""c"" for const_x
System.write(""test"")
System.print(const_x)
		";

		private static string _const = @"
var const_x = ""Hello""
		";

		public static void Main(string[] args)
		{
			WrenitVM vm = new WrenitVM();
			vm.ErrorHandler = Vm_ErrorEvent;
			vm.WriteHandler = Vm_WriteEvent;

			vm.ResolveModuleHandler = (WrenitVM _, string importer, string name) =>
			{
				//if (name == "c") return "const";
				return name;
			};
			vm.LoadModuleHandler = (WrenitVM __, string name) =>
			{
				if (name == "const")
				{
					return new WrenitLoadModuleResult() { Source = _const };
				}
				return new WrenitLoadModuleResult();
			};

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
