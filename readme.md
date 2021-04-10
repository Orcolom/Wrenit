# Wrenit

Wrenit is a .net binding for [wren](wren_site) written in c#

**Early stages! _not_ production ready at all!**

## Features / current stat

- supports latest wren (0.4.0)
- module and source builder using attributes

#### TODOs
- [ ] needs full documentation
- [ ] full unit testing
- [ ] move some code around to pretty the source code, its a mess in some files
- [ ] ensure it's fully memory safe
- [ ] write a special errors and messages handler so the user only receives one callback with all the info 
- [ ] ...

## Usage

Wrenit mostly follows wren's own c implementation. but with some wrapping for memory safety and c# flavoring.

For instance instead `wrenInterpret(vm, module, source)` turns into `vm.Interpret(module, source)`

### Hello world example
```cs
// configuration will hold all your callbacks and settings 
var config = WrenConfig.GetDefaults();

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
```


## Builder

Wrenit has a c# to wren source code builder via attributes

The more extensive Vector example with implementations can be found [here](wrenit_vector)
```cs
[WrenModule("Math")]
public class Math
{
	[WrenClass] // names get based on provided name or fallback on class/method name 
	public class Vector
	{
		[WrenAllocator]
		public static void Init(WrenVm vm) { }

		[WrenMethod(MethodType.Construct, "new", 2)]
		public static void NewZero(WrenVm vm) { }

		[WrenMethod(MethodType.Times, 1)]
		public static void Multiply(WrenVm vm) { }

		[WrenMethod(MethodType.FieldGetter, "x")]
		public static void GetX(WrenVm vm) { }
	}


	[WrenManualSource] // sometimes you just do it better yourself
	private static string Raw()
	{
		return "var PI = 3.1415";
	}
}

...

public void BindModules(ref WrenConfig config)
{
	// the build module will get cached
	WrenModule module = WrenBuilder.Build<Math>();
	
	// bind it to the needed callbacks of a config
	module.Bind(ref config);
} 
```

would result into the module `Math` with source code:
```js
foreign class Vector {
	foreign construct new(a,b)
	foreign Multiply(a)
	foreign x
}

var PI = 3.1415
```

[wren_site]: https://wren.io
[wrenit_vector]: https://github.com/Orcolom/Wrenit/blob/main/Wrenit.Shared/Vector.cs

## 