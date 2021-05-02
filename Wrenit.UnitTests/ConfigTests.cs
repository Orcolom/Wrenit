using System;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class ConfigTests : TestsBase
	{
		[Test]
		public void Modules()
		{
			var metaSource = @"
import ""meta"" for Meta

var source = """"""
	System.print(6)
""""""

Meta.eval(source)
";
			var randomSource = @"
import ""random"" for Random

var r = Random.new(12345)
";

			var config = new WrenConfig();
			Assert.AreEqual(WrenModules.All, config.OptionalModules);
			
			config.OptionalModules = WrenModules.None;
			var result = new WrenVm(config).Interpret("m", metaSource);
			if (result != WrenInterpretResult.RuntimeError) Assert.Fail("expected a runtime fail");
			result = new WrenVm(config).Interpret("m", randomSource);
			if (result != WrenInterpretResult.RuntimeError) Assert.Fail("expected a runtime fail");

			config.OptionalModules = WrenModules.ModuleMeta;
			result = new WrenVm(config).Interpret("m", metaSource);
			if (result != WrenInterpretResult.Success) Assert.Fail("expected a successfull run");
			result = new WrenVm(config).Interpret("m", randomSource);
			if (result != WrenInterpretResult.RuntimeError) Assert.Fail("expected a runtime fail");

			config.OptionalModules = Wrenit.WrenModules.ModuleRandom;
			result = new WrenVm(config).Interpret("m", metaSource);
			if (result != WrenInterpretResult.RuntimeError) Assert.Fail("expected a runtime fail");
			result = new WrenVm(config).Interpret("m", randomSource);
			if (result != WrenInterpretResult.Success) Assert.Fail("expected a successfull run");
		}

		[Test]
		public void Write()
		{
			string msg = "abc";
			bool called = false;

			WrenConfig config = new WrenConfig();

			config.WriteHandler += (_, text) =>
			{
				called = true;
				Assert.AreEqual(text, msg);
			};

			config.ErrorHandler += (vm, result, module, line, message) =>
			{
				Assert.IsTrue(false, "script execution failed");
			};

			new WrenVm(config).Interpret("m", $"System.write(\"{msg}\")");
			Assert.IsTrue(called);
		}


		[Test]
		public void WriteAfter()
		{
			string msg = "abc";
			bool called = false;

			var vm = new WrenVm();

			vm.Config.WriteHandler += (_, text) =>
			{
				called = true;
				Assert.AreEqual(text, msg);
			};

			vm.Config.ErrorHandler += (vm, result, module, line, message) =>
			{
				Assert.IsTrue(false, "script execution failed");
			};

			vm.Interpret("m", $"System.write(\"{msg}\")");
			Assert.IsTrue(called);
		}

		[Test]
		public void CompileError()
		{
			string msg = ";";
			bool called = false;

			WrenConfig config = new WrenConfig();

			config.WriteHandler += (_, text) => { Assert.IsTrue(false, "should not be hit"); };

			config.ErrorHandler = (vm, result, module, line, message) =>
			{
				called = true;
				Assert.AreEqual(WrenErrorType.CompileError, result);
				Assert.IsTrue(message.Contains(msg));
			};

			new WrenVm(config).Interpret("m", $"System.write(\"abc\"){msg}");
			Assert.IsTrue(called);
		}

		[Test]
		public void RuntimeError()
		{
			string msg = "writ";
			bool called = false;
			
			WrenConfig config = new WrenConfig();

			config.WriteHandler += (_, text) => { Assert.IsTrue(false, "should not be hit"); };

			bool first = true;
			config.ErrorHandler = (vm, result, module, line, message) =>
			{
				called = true;
				if (first)
				{
					Assert.AreEqual(WrenErrorType.RuntimeError, result);
					Assert.IsTrue(message.Contains(msg));
					Assert.AreEqual(-1, line);
					first = false;
				}
				else
				{
					Assert.AreEqual(WrenErrorType.StackTrace, result);
					Assert.AreEqual(1, line);
				}
			};

			new WrenVm(config).Interpret("m", $"System.{msg}(\"abc\")");
			Assert.IsTrue(called);
		}

		[Test]
		public void LoadModule()
		{
			string importerModule = "main";
			string inputModule = "hello";
			bool called = false;
			bool expectFail = false;

			WrenConfig config = new WrenConfig();

			config.LoadModuleHandler += (vm, name) =>
			{
				called = true;
				if (name != inputModule)
				{
					Assert.True(expectFail, $"did not expect module of name {name}");
					return null;
				}

				return $"var {name} = \"{name}\"";
			};
			config.ErrorHandler += (vm, result, module, line, message) => { };

			WrenInterpretResult res =
				new WrenVm(config).Interpret(importerModule, $"import \"{inputModule}\" for {inputModule}");
			if (res != WrenInterpretResult.Success) Assert.Fail("Failed interpretation");

			expectFail = true;
			res = new WrenVm(config).Interpret(importerModule, $"import \"willFail\" for willFail");
			if (res != WrenInterpretResult.RuntimeError) Assert.Fail("Expected runtime error");
			
			Assert.IsTrue(called);
		}

		[Test]
		public void ResolveModule()
		{
			string importerModule = "main";
			string inputModule = "hello";
			string inputModule2 = "foo";
			string resolveModule = "hello_world";

			bool expectFail = false;

			WrenConfig config = new WrenConfig();
			config.ResolveModuleHandler += (vm, importer, name) =>
			{
				Assert.AreEqual(importer, importerModule);

				if (name == inputModule)
				{
					return resolveModule;
				}

				if (name == inputModule2)
				{
					return inputModule2;
				}

				Assert.True(expectFail, $"did not expect module of name {name}");
				return name;
			};

			config.LoadModuleHandler += (vm, name) =>
			{
				if (name != resolveModule && name != inputModule2)
				{
					Assert.True(expectFail, $"did not expect module of name {name}");
					return null;
				}

				return $"var {name} = \"{name}\"";
			};
			config.ErrorHandler += (vm, result, module, line, message) => { };

			WrenInterpretResult res =
				new WrenVm(config).Interpret(importerModule, $"import \"{inputModule}\" for {resolveModule}");
			if (res != WrenInterpretResult.Success) Assert.Fail("Failed interpretation");

			res = new WrenVm(config).Interpret(importerModule, $"import \"{inputModule2}\" for {inputModule2}");
			if (res != WrenInterpretResult.Success) Assert.Fail("Failed interpretation");

			expectFail = true;
			res = new WrenVm(config).Interpret(importerModule, $"import \"willFail\" for willFail");
			if (res != WrenInterpretResult.RuntimeError) Assert.Fail("Expected runtime error");
		}

		[Test]
		public void BindMethod()
		{
			string classA = "AClass";
			WrenForeignMethod bindingA =new WrenForeignMethod(vm => { });

			WrenConfig config = new WrenConfig();
			config.WriteHandler += (vm, text) => { Assert.AreEqual(text, classA); };
			config.ErrorHandler += (vm, type, module, line, message) => { };

			config.BindForeignMethodHandler += (vm, module, name, isStatic, signature) =>
			{
				if (name == classA) return bindingA;

				return null;
			};

			WrenInterpretResult result = new WrenVm(config).Interpret("main", $@"
				class AClass {{
					foreign static One()
				}}
				System.write(AClass.One())
			");
			if (result != WrenInterpretResult.Success) Assert.Fail("Expected successful interpretation");

			result = new WrenVm(config).Interpret("main", @"
				Class ClassB {
					foreign static One()
				}
				ClassB.One()
			");
			if (result != WrenInterpretResult.CompileError) Assert.Fail("Expected compile error interpretation");
		}

		[Test]
		public void BindClass()
		{
			string className = "ClassA";

			void Alloc(WrenVm vm)
			{
				vm.EnsureSlots(1);
				vm.SetSlotNewForeign<float>(0, 0);
			}

			WrenForeignMethod InitA = new WrenForeignMethod(vm=> { });
			WrenForeignMethod InitB = new WrenForeignMethod(vm=> { });
			
			WrenConfig config = new WrenConfig();
			config.BindForeignMethodHandler += (vm, module, name, isStatic, signature) =>
			{
				if (name == className) return InitA;
				return InitB;
			};

			config.BindForeignClassHandler += (vm, module, name) =>
			{
				if (name == className)
				{
					return new WrenForeignClass(Alloc);
				}

				return null;
			};

			WrenInterpretResult result = new WrenVm(config).Interpret("main", $@"
				foreign class {className} {{
					foreign construct new()
				}}
				
				{className}.new()
			");
			if (result != WrenInterpretResult.Success) Assert.Fail("Expected successful interpretation");

			
			result = new WrenVm(config).Interpret("main", $@"
					foreign class {className}x {{
						foreign construct new()
					}}
					
					foreign class {className}y {{
						foreign construct new()
					}}
					
					{className}x.new()
					{className}y.new()
			");
			if (result != WrenInterpretResult.RuntimeError) Assert.Fail("Expected compile error");
			
		}
	}
}
