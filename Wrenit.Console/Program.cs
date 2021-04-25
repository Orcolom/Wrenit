using System;
using Wrenit.Shared;
using Wrenit.Utilities;

namespace Wrenit.Consoles
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			string path = "./a/path/to/file.ext";
			string main = $@"
import ""Assets"" for Asset, AssetSystem

var asset = AssetSystem.Load(""{path}"")

System.write(asset.path)
";
			var config = new WrenConfig();
			config.WriteHandler += (wrenVm, text) =>
			{
				Console.WriteLine("e");
			};
				
			var vm = new WrenVm(config);
			vm.Config.BindModule<AssetsModule>();
			
			var result = vm.Interpret("<main>", main);
			Console.WriteLine("x");
		}
	}
}
