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
			var config = WrenConfig.GetDefaults();
			config.WriteHandler += (wrenVm, text) =>
			{
				Console.WriteLine("e");
			};
				
			WrenBuilder.Build<AssetsModule>().Bind(ref config);
			
			var vm = new WrenVm(config);
			var result = vm.Interpret("<main>", main);
			Console.WriteLine("x");
		}
	}
}
