using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wrenit.Utilities
{
	struct Signature
	{
		public readonly int Arguments;
		public readonly string Format;
		public readonly string ForcedName;
		public readonly Func<bool, string> CustomValue;

		// 0 = custom word
		// 1 = name
		// 2 = arguments list
		// 3 = extra arguments list
		private const string FormatCustomNameArg = "{0} {1}({2})";
		private const string FormatNameArg = "{1}({2})";
		private const string FormatName = "{1}";

		public static readonly Dictionary<MethodType, Signature> Signatures = new Dictionary<MethodType, Signature>()
		{
			{MethodType.Method, new Signature(FormatNameArg, -1)},
			{
				MethodType.StaticMethod,
				new Signature(FormatCustomNameArg, -1, customValue: implement => implement ? "static" : "")
			},
			{
				MethodType.Construct,
				new Signature(FormatCustomNameArg, -1, customValue: implement => implement ? "construct" : "init")
			},
			{MethodType.RangeInclusive, new Signature(FormatNameArg, 1, "..")},
			{MethodType.RangeExclusive, new Signature(FormatNameArg, 1, "...")},
			{MethodType.Times, new Signature(FormatNameArg, 1, "*")},
			{MethodType.Divide, new Signature(FormatNameArg, 1, "/")},
			{MethodType.Modulo, new Signature(FormatNameArg, 1, "%")},
			{MethodType.Plus, new Signature(FormatNameArg, 1, "+")},
			{MethodType.Minus, new Signature(FormatNameArg, 1, "-")},
			{MethodType.BitwiseLeftShift, new Signature(FormatNameArg, 1, "<<")},
			{MethodType.BitwiseRightShift, new Signature(FormatNameArg, 1, ">>")},
			{MethodType.BitwiseXor, new Signature(FormatNameArg, 1, "^")},
			{MethodType.BitwiseOr, new Signature(FormatNameArg, 1, "|")},
			{MethodType.BitwiseAnd, new Signature(FormatNameArg, 1, "&")},
			{MethodType.SmallerThen, new Signature(FormatNameArg, 1, "<")},
			{MethodType.SmallerEqualThen, new Signature(FormatNameArg, 1, "<=")},
			{MethodType.BiggerThen, new Signature(FormatNameArg, 1, ">")},
			{MethodType.BiggerEqualThen, new Signature(FormatNameArg, 1, ">=")},
			{MethodType.Equal, new Signature(FormatNameArg, 1, "==")},
			{MethodType.NotEqual, new Signature(FormatNameArg, 1, "!=")},
			{MethodType.Is, new Signature(FormatNameArg, 1, "is")},
			{MethodType.FieldGetter, new Signature(FormatName, 0)},
			{MethodType.Inverse, new Signature(FormatName, 0, "-")},
			{MethodType.Not, new Signature(FormatName, 0, "!")},
			{MethodType.Tilda, new Signature(FormatName, 0, "~")},
			{MethodType.FieldSetter, new Signature("{1}=({2})", 1)},
			{
				MethodType.SubScriptSetter,
				new Signature("[{2}]=({0})", -1, customValue: implement => implement ? "value" : "_")
			},
			{MethodType.SubScriptGetter, new Signature("[{2}]", -1)},
		};

		private Signature(string format, int arguments, string customName = null, Func<bool, string> customValue = null)
		{
			ForcedName = customName;
			Arguments = arguments;
			Format = format;
			CustomValue = customValue;
		}
	}

	public enum SignatureStyle
	{
		Signature,
		Implementation,
		ForeignImplementation,
	}
	
	public enum MethodType
	{
		Method, // foo()
		StaticMethod, // static foo()
		Construct, // init foo()

		FieldGetter, // foo
		FieldSetter, // foo=()
		SubScriptGetter, // []
		SubScriptSetter, // []=()

		Not, // !
		Inverse, // -
		Tilda, // ~

		RangeInclusive, // ..		?RangeExlusive
		RangeExclusive, // ...	?RangeInclusive (check docs for dot count)

		Times, // * 
		Divide, // / 
		Modulo, // % 
		Plus, // + 
		Minus, // - 

		BitwiseLeftShift, // <<		BitwiseLeft
		BitwiseRightShift, // >>		BitwiseRight
		BitwiseXor, // ^		BitwiseXor
		BitwiseOr, // |		BitwiseOr
		BitwiseAnd, // &		BitwiseAnd

		SmallerThen,
		SmallerEqualThen,
		BiggerThen,
		BiggerEqualThen,
		Equal,
		NotEqual,

		Is,
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WrenModuleAttribute : Attribute
	{
		public string Name;
		public readonly string Source;

		public WrenModuleAttribute(string source)
		{
			Source = source;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WrenClassAttribute : Attribute
	{
		public readonly string Name;

		public WrenClassAttribute() { }

		public WrenClassAttribute(string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenAllocatorAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenFinalizerAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenRawSourceAttribute : Attribute { }

	public class WrenMethodAttribute : Attribute
	{
		public readonly string Name;
		public readonly int ArgumentCount;
		public readonly MethodType Type;

		public WrenMethodAttribute(MethodType type, string name = null, int argumentCount = 0)
		{
			Name = name;
			ArgumentCount = Wren.CorrectArgumentCount(type, argumentCount);
			Type = type;
		}
	}

	public static class WrenBuilder
	{
		private static T GetAttribute<T>(this Type type)
			where T : Attribute
		{
			return type.GetCustomAttribute(typeof(T)) as T;
		}

		public static WrenModule Build<T>()
		{
			Type moduleType = typeof(T);
			WrenModuleAttribute moduleAttribute = moduleType.GetAttribute<WrenModuleAttribute>();
			if (moduleAttribute == null) return null;
			if (string.IsNullOrEmpty(moduleAttribute.Source)) return null;

			List<WrenClass> _classes = new List<WrenClass>();
			Type[] nestedTypes = moduleType.GetNestedTypes();

			for (int i = 0; i < nestedTypes.Length; i++)
			{
				Type classType = nestedTypes[i];
				WrenClassAttribute classAttribute = classType.GetAttribute<WrenClassAttribute>();
				if (classAttribute == null) continue;

				WrenClass wrenClass = BuildClass(classType, classAttribute);
				if (wrenClass != null) _classes.Add(wrenClass);
			}

			return new WrenModule(moduleAttribute.Name ?? moduleType.Name, moduleAttribute.Source, _classes);
		}

		private static WrenClass BuildClass(Type classType, WrenClassAttribute classAttribute)
		{
			MethodInfo[] methodInfos =
				classType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			WrenForeignMethod allocator = null;
			WrenFinalizer finalizer = null;
			List<WrenMethod> methods = new List<WrenMethod>();
			List<WrenMethodAttribute> methodAttributes = new List<WrenMethodAttribute>();

			for (int i = 0; i < methodInfos.Length; i++)
			{
				var methodInfo = methodInfos[i];

				if (allocator == null)
				{
					WrenAllocatorAttribute newAlloc = methodInfo.GetCustomAttribute<WrenAllocatorAttribute>();
					if (newAlloc != null)
					{
						if (IsValidCSharpSignature(methodInfo))
						{
							allocator = Delegate.CreateDelegate(typeof(WrenForeignMethod), methodInfo) as WrenForeignMethod;
						}

						continue;
					}
				}

				if (finalizer == null)
				{
					WrenFinalizerAttribute newFin = methodInfo.GetCustomAttribute<WrenFinalizerAttribute>();
					if (newFin != null)
					{
						if (IsValidCSharpSignature(methodInfo, true))
						{
							finalizer = Delegate.CreateDelegate(typeof(WrenFinalizer), methodInfo) as WrenFinalizer;
						}

						continue;
					}
				}

				WrenMethodAttribute methodAttribute = methodInfo.GetCustomAttribute<WrenMethodAttribute>();
				if (IsValidCSharpSignature(methodInfo) == false) continue;

				string name = methodAttribute.Name ?? methodInfo.Name;
				if (IsOriginalAttribute(methodAttributes, methodAttribute, name) == false) continue;

				methodAttributes.Add(methodAttribute);

				WrenMethod wrenMethod = new WrenMethod(methodAttribute.Type, name, methodAttribute.ArgumentCount,
					Delegate.CreateDelegate(typeof(WrenForeignMethod), methodInfo) as WrenForeignMethod);

				methods.Add(wrenMethod);
			}

			return new WrenClass(classAttribute.Name ?? classType.Name, allocator, finalizer, methods);
		}

		private static bool IsOriginalAttribute(List<WrenMethodAttribute> methods, WrenMethodAttribute attribute,
			string name)
		{
			for (int i = 0; i < methods.Count; i++)
			{
				WrenMethodAttribute method = methods[i];
				if (method.Type != attribute.Type) continue;
				if (method.ArgumentCount != attribute.ArgumentCount) continue;
				if (method.Name != name) continue;

				return false;
			}

			return true;
		}

		private static bool IsValidCSharpSignature(MethodInfo methodInfo, bool isFinalizer = false)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length != 1) return false;

			if (isFinalizer) return parameterInfos[0].ParameterType == typeof(IntPtr);

			return parameterInfos[0].ParameterType == typeof(WrenVm);
		}
	}
}
