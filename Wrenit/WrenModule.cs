using System.Collections.Generic;
using System.Text;

namespace Wrenit.Utilities
{
	public class WrenModule
	{
		public readonly string Name;
		public readonly string Source;
		private readonly Dictionary<string, WrenClass> _classes = new Dictionary<string, WrenClass>();

		public WrenModule(string name) { }

		public WrenModule(string name, string source, params WrenClass[] classes)
		{
			Name = name;
			Source = source;
			for (int i = 0; i < classes.Length; i++)
			{
				_classes.Add(classes[i].Name, classes[i]);
			}
		}

		public WrenModule(string name, string source, List<WrenClass> classes)
		{
			Name = name;
			Source = source;
			for (int i = 0; i < classes.Count; i++)
			{
				_classes.Add(classes[i].Name, classes[i]);
			}
		}

		public WrenClass FindClass(string name)
		{
			if (_classes.TryGetValue(name, out WrenClass cls) == false) return null;

			return cls;
		}
	}

	public class WrenClass
	{
		public readonly string Name;

		private readonly WrenForeignMethodBinding _allocator;
		private readonly WrenFinalizerMethodBinding _finalizer;
		private readonly List<WrenMethod> _methods = new List<WrenMethod>();

		internal bool HasAllocator => _allocator != null;
		internal List<WrenMethod> Methods => _methods;

		public WrenClass(string name, WrenForeignMethod allocator, WrenFinalizer finalizer, params WrenMethod[] methods)
		{
			Name = name;
			if (allocator != null)
			{
				_allocator = new WrenForeignMethodBinding(allocator);
				_finalizer = new WrenFinalizerMethodBinding(finalizer);
			}

			_methods.AddRange(methods);
		}

		public WrenClass(string name, WrenForeignMethod allocator, WrenFinalizer finalizer, List<WrenMethod> methods)
		{
			Name = name;
			if (allocator != null)
			{
				_allocator = new WrenForeignMethodBinding(allocator);
				_finalizer = new WrenFinalizerMethodBinding(finalizer);
			}

			_methods.AddRange(methods);
		}

		public WrenMethod FindMethod(string signature, bool isStatic)
		{
			for (int i = 0; i < _methods.Count; i++)
			{
				WrenMethod m = _methods[i];
				if (m.Signature == signature && m.IsStatic == isStatic) return m;
			}

			return null;
		}

		public WrenForeignClass AsForeign()
		{
			return new WrenForeignClass(_allocator, _finalizer);
		}

	}

	public class WrenMethod
	{
		public readonly string Signature;
		public readonly MethodType Type;
		public readonly bool IsStatic;
		public readonly WrenForeignMethodBinding MethodBinding;

		public WrenMethod(MethodType type, string name, int argumentCount, WrenForeignMethod method)
		{
			Signature = Wren.CreateSignature(type, name, argumentCount);
			IsStatic = type == MethodType.MethodStatic;
			MethodBinding = new WrenForeignMethodBinding(method);
			Type = type;
		}
	}
}
