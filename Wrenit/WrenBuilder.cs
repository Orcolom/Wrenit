using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wrenit.Utilities
{
	public enum MethodType
	{
		Method, // foo()
		MethodStatic, // static foo()
		MethodConstruct, // init foo()
		FieldGetter, // foo
		FieldSetter, // foo=()
		SubScriptGetter, // []=()
		SubScriptSetter, // []=()

		OperatorPrefixNot, // !
		OperatorPrefixMinus, // -
		OperatorPrefixTilda, // ~
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WrenModuleAttribute : Attribute
	{
		public readonly string Name;
		public readonly string Source;

		public WrenModuleAttribute(string source)
		{
			Source = source;
		}

		public WrenModuleAttribute(string name, string source)
		{
			Name = name;
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
			if (string.IsNullOrEmpty(moduleAttribute.Source))  return null;

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
