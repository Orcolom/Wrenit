using System;
using Wrenit.Shared;
using Wrenit.Utilities;

namespace Wrenit.Consoles
{
	internal class Program
	{
		private static string _script = @"
import ""Math"" for PI2

System.print(PI2)
		";

		public static void Main(string[] args)
		{
			WrenConfig config = WrenConfig.GetDefaults();
			
			config.ErrorHandler += Vm_ErrorEvent;
			config.WriteHandler += Vm_WriteEvent;
			
			WrenBuilder.Build<Constants>().Bind(ref config);
			WrenBuilder.Build<WrenMath>().Bind(ref config);
			

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
