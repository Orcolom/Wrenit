using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wrenit
{
	public delegate void ForeignMethod(WrenitVM vm);
	
	public delegate void WrenFinalizer(IntPtr data);

	public class WrenitModule 
	{
		public readonly string Name;
		public readonly string Source;
		public readonly Dictionary<string, WrenitClass> Classes = new Dictionary<string, WrenitClass>();

		public WrenitModule(string name, string source, params WrenitClass[] classes)
		{
			Name = name;
			Source = source;
			for (int i = 0; i < classes.Length; i++)
			{
				Classes.Add(classes[i].Name, classes[i]);
			}
		}

		public WrenitClass FindClass(string name)
		{
			if (Classes.TryGetValue(name, out WrenitClass cls))
			{
				return cls;
			}
			return null;
		}
	}

	public class WrenitClass 
	{
		public readonly string Name;
		public readonly List<WrenitMethod> Methods = new List<WrenitMethod>();
		public readonly WrenitFunction Allocator;
		public readonly WrenitFinalizerFunction Finalizer;

		public WrenitClass(string name, ForeignMethod allocator, WrenFinalizer finalizer, params WrenitMethod[] methods)
		{
			Name = name;
			Allocator = allocator != null ? new WrenitFunction(allocator):null;
			Finalizer = finalizer != null ? new WrenitFinalizerFunction(finalizer): null;
			Methods.AddRange(methods);
		}

		public WrenitMethod FindMethod(string signature, bool isStatic)
		{
			for (int i = 0; i < Methods.Count; i++)
			{
				WrenitMethod m = Methods[i];
				if (m.Signature == signature && m.IsStatic == isStatic) return m;
			}
			return null;
		}
	}
	
	public class WrenitMethod
	{
		public readonly string Signature;
		public readonly bool IsStatic;
		public readonly WrenitFunction Method;

		public WrenitMethod(string signature, bool isStatic, ForeignMethod method)
		{
			Method = new WrenitFunction(method);
			Signature = signature;
			IsStatic = isStatic;
		}
	}

	public class WrenitFunction
	{
		public readonly IntPtr MethodPtr;
		private ForeignMethod _method;

		public WrenitFunction(ForeignMethod method)
		{
			_method = method;
			MethodPtr = Marshal.GetFunctionPointerForDelegate<WrenForeignMethodFn>(OnWrenCall);
		}

		private void OnWrenCall(IntPtr ptr)
		{
			WrenitVM vm = WrenitVM.GetVM(ptr);
			if (vm != null)
			{
				_method?.Invoke(vm);
			}
		}
	}

	public class WrenitFinalizerFunction
	{
		public readonly IntPtr MethodPtr;
		private WrenFinalizer _method;

		public WrenitFinalizerFunction(WrenFinalizer method)
		{
			_method = method;
			MethodPtr = Marshal.GetFunctionPointerForDelegate(method);
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
				if (i+1 < param)
				{
					sb.Append(",");
				}
			}
			sb.Append(")");
			return sb.ToString();
		}
	}
}
