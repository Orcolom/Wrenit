using System;
using System.Runtime.InteropServices;
using Wrenit;

namespace Wrenit.Consoles
{
	internal class Program
	{
		private static string _script = @"
System.write(""test"")
System.print(""test"")
		";
		public static void Main(string[] args)
		{
			WrenitVM vm = new WrenitVM();
			vm.ErrorEvent += Vm_ErrorEvent;
			vm.WriteEvent += Vm_WriteEvent;
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
