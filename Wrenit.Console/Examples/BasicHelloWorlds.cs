using System;

namespace Wrenit.Consoles.Examples
{
	public class BasicHelloWorlds
	{
		public static void OneLineHelloWorld()
		{
			new WrenVm().Interpret("main", "System.write\"Hello World\"");
		}

		public static void HelloWorld()
		{
			// configuration will hold all your callbacks and settings 
			var config = new WrenConfig();

			// listen to write callbacks
			config.WriteHandler += (vm, text) =>
			{
				Console.Write(text); // Hello World
			};

			// create a new vm
			var wrenVm = new WrenVm(config);
			
			// interpret some code
			WrenInterpretResult result = wrenVm.Interpret("main", "System.write(\"Hello World\")");

			if (result == WrenInterpretResult.Success)
			{
				Console.Write(@"\o/ ran some wren code");
			}
		}
	}
}
