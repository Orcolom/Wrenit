using System;
using NUnit.Framework;
using Wrenit.Shared;
using Wrenit.Utilities;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class ForeignTests : TestsBase
	{

		[Test]
		public void Asset()
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
				Assert.AreEqual(path, text);
			};
				
			WrenBuilder.GetModule<AssetsModule>().Bind(ref config);
			
			var vm = new WrenVm(config);
			var result = vm.Interpret("<main>", main);
			if (result != WrenInterpretResult.Success) Assert.Fail("Expected successful interpret");
		}
		
		[Test]
		public void ForeignClassInherit()
		{
			WrenBuilder.GetModule<AssetsModule>();
			Assert.Throws<InvalidOperationException>(() => WrenBuilder.GetModule<AssetsModule2>(), "Should not be able to inherit");
		}
		
		[Test]
		public void ForeignClassInherit2()
		{
			Assert.Throws<NullReferenceException>(() => WrenBuilder.GetModule<AssetsModule2>(), "Should not be able to inherit a class that doesnt exist");
			WrenBuilder.GetModule<AssetsModule>();
		}
	}
}
