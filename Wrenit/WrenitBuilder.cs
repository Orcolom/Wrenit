using System.Collections.Generic;
using System.Text;

namespace Wren.it.Builder
{
	public class WrenitModule
	{
		public readonly string Name;
		public readonly string Source;
		private readonly Dictionary<string, WrenitClass> _classes = new Dictionary<string, WrenitClass>();

		public WrenitModule(string name, string source, params WrenitClass[] classes)
		{
			Name = name;
			Source = source;
			for (int i = 0; i < classes.Length; i++)
			{
				_classes.Add(classes[i].Name, classes[i]);
			}
		}

		public WrenitClass FindClass(string name)
		{
			if (_classes.TryGetValue(name, out WrenitClass cls) == false) return null;

			return cls;
		}
	}

	public class WrenitClass
	{
		public readonly string Name;
		
		private readonly WrenForeignMethodBinding _allocator;
		private readonly WrenFinalizerMethodBinding _finalizer;
		private readonly List<WrenitMethod> _methods = new List<WrenitMethod>();

		public WrenitClass(string name, WrenForeignMethod allocator, WrenFinalizer finalizer, params WrenitMethod[] methods)
		{
			Name = name;
			if (allocator != null)
			{
				_allocator = new WrenForeignMethodBinding(allocator);
				_finalizer = new WrenFinalizerMethodBinding(finalizer);
			}
			_methods.AddRange(methods);
		}

		public WrenitMethod FindMethod(string signature, bool isStatic)
		{
			for (int i = 0; i < _methods.Count; i++)
			{
				WrenitMethod m = _methods[i];
				if (m.Signature == signature && m.IsStatic == isStatic) return m;
			}

			return null;
		}

		public WrenForeignClass AsForeign()
		{
			return new WrenForeignClass(_allocator, _finalizer);
		}
	}

	public class WrenitMethod
	{
		public readonly string Signature;
		public readonly bool IsStatic;
		public readonly WrenForeignMethodBinding MethodBinding;

		public WrenitMethod(string signature, bool isStatic, WrenForeignMethod method)
		{
			MethodBinding = new WrenForeignMethodBinding(method);
			Signature = signature;
			IsStatic = isStatic;
		}
	}

	public static class WrenitSignature
	{
		public static string Method(string name, int param = 0)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(name);
			sb.Append("(");
			for (int i = 0; i < param; i++)
			{
				sb.Append("_");
				if (i + 1 < param)
				{
					sb.Append(",");
				}
			}

			sb.Append(")");
			return sb.ToString();
		}
	}
}
