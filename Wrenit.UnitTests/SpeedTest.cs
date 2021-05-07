using NUnit.Framework;
using Wrenit.Shared;
using Wrenit.Utilities;
using System.Diagnostics;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class SpeedTests : TestsBase
	{
		[Test]
		public void Speed()
		{
			string main = $@"
for (i in 0..100000) {{
	System.print(i)
}}
";
			var config = new WrenConfig();
			config.WriteHandler += (vm, msg) => Debug.Write(msg);
			WrenBuilder.GetModule<AssetsModule>().Bind(config);
			
			var vm = new WrenVm(config);
			var result = vm.Interpret("<main>", main);
			if (result != WrenInterpretResult.Success) Assert.Fail("Expected successful interpret");
		}
	}
}
