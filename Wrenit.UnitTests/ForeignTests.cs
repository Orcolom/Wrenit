using System;
using System.Globalization;
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
var asset2 = AssetSystem.Load(""{path}"",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0)

System.write(asset.path)
";
			var config = new WrenConfig();
			config.WriteHandler += (wrenVm, text) =>
			{
				Assert.AreEqual(path, text);
			};

			WrenBuilder.GetModule<AssetsModule>().Bind(config);

			var vm = new WrenVm(config);
			var result = vm.Interpret("<main>", main);
			if (result != WrenInterpretResult.Success) Assert.Fail("Expected successful interpret");
		}

		[Test]
		public void Asset10000()
		{
			string main = $@"
import ""Assets"" for Asset, AssetSystem

for (i in 0...10000) {{
	var asset = AssetSystem.Load(""a/path"")
}}
";
			var config = new WrenConfig();
			WrenBuilder.GetModule<AssetsModule>().Bind(config);

			var vm = new WrenVm(config);
			var result = vm.Interpret("<main>", main);
			if (result != WrenInterpretResult.Success) Assert.Fail("Expected successful interpret");
			vm.CollectGarbage();
			Assert.AreEqual(0, vm.Cache.ForeignObjects.Count);
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

		[Test]
		public void Math()
		{
			double val = 2.3;
			string main = $@"
import ""Math"" for Vector

var v = Vector.new({val.ToWrenSafeString()}, 1)

System.write(v.x)
";
			var config = new WrenConfig();
			config.WriteHandler += (wrenVm, text) =>
			{
				Assert.AreEqual(val.ToString(CultureInfo.InvariantCulture), text);
			};
			
			config.ErrorHandler += (wrenVm, type, module, line, message) =>
			{
				Assert.False(false, message);
			};
				
			WrenBuilder.GetModule<ConstantsModule>().Bind(config);
			WrenBuilder.GetModule<MathModule>().Bind(config);
			
			var vm = new WrenVm(config);
			var result = vm.Interpret("<main>", main);
			if (result != WrenInterpretResult.Success) Assert.Fail("Expected successful interpret");
		}


		[Test]
		public void InvalidInheritance()
		{
			
			Assert.Throws<NullReferenceException>(() => WrenBuilder.GetModule<NullImportModule>(), "Should not be able to import a null(string) class");
			Assert.Throws<NullReferenceException>(() => WrenBuilder.GetModule<NullImport2Module>(), "Should not be able to import a null(type) class");
			Assert.Throws<NullReferenceException>(() => WrenBuilder.GetModule<MissingImportModule>(), "Should not be able to import a class that doesnt exist");
		}
	}
}
