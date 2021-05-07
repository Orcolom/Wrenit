using System;
using System.Diagnostics;
using Wrenit.Shared;
using Wrenit.Utilities;

namespace Wrenit.Consoles
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			string main = $@"
import ""Assets"" for Asset, AssetSystem

for (i in 0..10000000) {{
	var asset = AssetSystem.Load(""a/path"")
	System.print(i)
}}
";
			var config = new WrenConfig();
			config.WriteHandler += (vm, msg) => { };
			WrenBuilder.GetModule<AssetsModule>().Bind(config);
			
			var vmx = new WrenVm(config);
			Debug.Write("start");
			vmx.Interpret("<main>", main);
			Debug.Write("end");
		}
	}
}
