using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wrenit.Utilities
{
	public static class WrenBuilder
	{
		/// <summary>
		/// dictionary of all the build modules
		/// </summary>
		private static readonly Dictionary<Type, WrenModule> Modules = new Dictionary<Type, WrenModule>();
		
		/// <summary>
		/// dictionary of all the build objects and their resolved names
		/// </summary>
		private static readonly Dictionary<Type, string> Names = new Dictionary<Type, string>();
		
		/// <summary>
		/// list of all the pure foreign classes. This is used to check possible inheritances
		/// </summary>
		private static readonly List<Type> ForeignClasses = new List<Type>();


		/// <summary>
		/// get the resolved name of type
		/// </summary>
		/// <returns>string name or null</returns>
		public static string NameOf<T>() => NameOf(typeof(T));

		/// <summary>
		/// get the resolved name of type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="assumeWillExist"></param>
		/// <returns>string name or null</returns>
		public static string NameOf(Type type, bool assumeWillExist = true)
		{
			if (type == null) throw new NullReferenceException("argument 'type' is null");

			if (Names.TryGetValue(type, out string name)) return name;
			if (assumeWillExist == false) return null;
			
			return type.GetAttribute<AWrenCodeAttribute>() switch
			{
				WrenModuleAttribute mod => AddName(type, mod),
				WrenClassAttribute cls => AddName(type, cls),
				WrenClassAssumedAttribute cls => AddName(type, cls),
				_ => throw new ArgumentException($"{type} is not marked as a module or class")
			};
		}
		
		/// <summary>
		/// Will get cached module of type or will build a new module from type
		/// </summary>
		/// <typeparam name="T">Module type that implements wren attributes</typeparam>
		public static WrenModule GetModule<T>()
		{
			Type moduleType = typeof(T);
			return Modules.ContainsKey(moduleType) ? Modules[moduleType] : BuildNew<T>();
		}

		/// <summary>
		/// clear all cached classes.
		/// </summary>
		internal static void ClearCache()
		{
			Modules.Clear();
			Names.Clear();
			ForeignClasses.Clear();
		}
		
		#region Build Process
		
		private static WrenModule BuildNew<T>()
		{
			Type moduleType = typeof(T);

			WrenModuleAttribute moduleAttribute = moduleType.GetAttribute<WrenModuleAttribute>();
			if (moduleAttribute == null) return null;

			WrenSourceBuilder sb = new WrenSourceBuilder();
			sb.AppendImports(moduleType.GetAttributes<WrenImportAttribute>());

			List<WrenClass> classes = new List<WrenClass>();

			List<(MemberInfo, AWrenCodeAttribute)> members = moduleType.GetAttributedMembers();

			for (int i = 0; i < members.Count; i++)
			{
				switch (members[i].Item2)
				{
					case WrenClassAttribute classAttribute:
						classes.Add(BuildClass(members[i].Item1 as Type, classAttribute, sb));
						break;

					case WrenManualSourceAttribute _:
						AppendManualSource(members[i].Item1 as MethodInfo, sb);
						break;
										
					case WrenClassAssumedAttribute classAssumedAttribute:
						AddName(members[i].Item1 as Type, classAssumedAttribute);
						break;
				}
			}

			string source = sb.ToString();
			string name = AddName(moduleType, moduleAttribute);
			WrenModule module = new WrenModule(name, source, classes);
			Modules.Add(moduleType, module);
			return module;
		}

		private static WrenClass BuildClass(Type classType, WrenClassAttribute classAttribute, WrenSourceBuilder sb)
		{
			List<(MemberInfo, AWrenCodeAttribute)> attributedMembers = classType.GetAttributedMembers();

			(MemberInfo, AWrenCodeAttribute) validAlloc = attributedMembers.Find(tuple =>
				tuple.Item2 is WrenAllocatorAttribute && IsValidAllocateSignature(tuple.Item1 as MethodInfo));

			(MemberInfo, AWrenCodeAttribute) validFin = attributedMembers.Find(tuple =>
				tuple.Item2 is WrenFinalizerAttribute && IsValidFinalizerSignature(tuple.Item1 as MethodInfo));

			string className = AddName(classType, classAttribute);

			string inherit = classAttribute.Inherit;
			if (string.IsNullOrEmpty(inherit) && classAttribute.InheritType != null)
			{
				inherit = NameOf(classAttribute.InheritType, false);
				if (string.IsNullOrEmpty(inherit))
					throw new NullReferenceException($"Could not find inherited class of type {classAttribute.InheritType}. inherited classes should be build beforehand");
				if (ForeignClasses.Contains(classAttribute.InheritType))
					throw new InvalidOperationException("Cant inherit from a foreign class");
			}
			
			sb.AppendAttributes(classType.GetAttributes<WrenAttributeAttribute>());
			sb.OpenClass(validAlloc.Item1 != null, className, inherit);

			List<WrenMethod> methods = new List<WrenMethod>();
			List<WrenMethodAttribute> methodAttributes = new List<WrenMethodAttribute>();

			WrenForeignMethod0 allocator = null;
			WrenForeignFinalizer finalizer = null;
			if (validAlloc.Item1 != null)
			{
				ForeignClasses.Add(classType);
				allocator =
					Delegate.CreateDelegate(typeof(WrenForeignMethod0), validAlloc.Item1 as MethodInfo) as WrenForeignMethod0;
			}

			if (validFin.Item1 != null)
			{
				finalizer =
					Delegate.CreateDelegate(typeof(WrenForeignFinalizer), validFin.Item1 as MethodInfo) as WrenForeignFinalizer;
			}

			for (int i = 0; i < attributedMembers.Count; i++)
			{
				(MemberInfo, AWrenCodeAttribute) attributeMember = attributedMembers[i];
				if (attributeMember == validAlloc) continue;
				if (attributeMember == validFin) continue;

				switch (attributeMember.Item2)
				{
					case WrenManualSourceAttribute rawSourceAttribute:
						AppendManualSource(attributeMember.Item1 as MethodInfo, sb);
						break;

					case WrenMethodAttribute methodAttribute:
						MethodInfo method = attributeMember.Item1 as MethodInfo;

						if (IsValidMethodSignature(method, methodAttribute, out var slots) == false) continue;
						
						string name = methodAttribute.Name ?? method.Name;
						
						if (IsOriginalAttribute(methodAttributes, methodAttribute, name) == false) continue;

						methodAttributes.Add(methodAttribute);

						WrenMethod wrenMethod = new WrenMethod(methodAttribute.Type, name, slots.Count,
							new WrenForeignMethod(method, slots.Count));

						sb.AppendAttributes(method.GetAttributes<WrenAttributeAttribute>());
						sb.AddMethod(methodAttribute.Type, name, slots);

						methods.Add(wrenMethod);
						break;
				}
			}

			sb.CloseClass();

			var wrenClass = new WrenClass(className, allocator, finalizer, methods);
			return wrenClass;
		}

		private static bool IsOriginalAttribute(IReadOnlyList<WrenMethodAttribute> methods, WrenMethodAttribute attribute,
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

		private static void AppendManualSource(MethodInfo methodInfo, WrenSourceBuilder sb)
		{
			if (IsValidRawSignature(methodInfo) == false) return;
			sb.AddRaw((string)methodInfo.Invoke(null,null));
		}

		
		private static string AddName(Type type, WrenModuleAttribute attribute)
		{
			if (Names.ContainsKey(type)) return Names[type];
			
			string name = attribute.Name ?? type.Name;
			Names.Add(type, name);
			return name;
		}
		
		private static string AddName(Type type, WrenClassAttribute attribute)
		{
			if (Names.ContainsKey(type)) return Names[type];
			
			string name = attribute.Name ?? type.Name;
			Names.Add(type, name);
			return name;
		}
		
		private static string AddName(Type type, WrenClassAssumedAttribute attribute)
		{
			if (Names.ContainsKey(type)) return Names[type];
			
			string name = attribute.Name ?? type.Name;
			Names.Add(type, name);
			return name;
		}
		
		private static bool IsValidFinalizerSignature(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length != 1) return false;

			return parameterInfos[0].ParameterType == typeof(WrenForeignObject);
		}

		private static bool IsValidAllocateSignature(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length != 1) return false;

			return parameterInfos[0].ParameterType == typeof(WrenVm);
		}
		
		private static bool IsValidMethodSignature(MethodInfo methodInfo, WrenMethodAttribute attribute, out List<WrenSlotAttribute> wrenSlots)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			wrenSlots = new List<WrenSlotAttribute>();
			if (parameterInfos.Length == 0) return false;
			if (parameterInfos[0].ParameterType != typeof(WrenVm)) return false;

			for (int i = 1; i < parameterInfos.Length; i++)
			{
				var name = parameterInfos[i].Name;
				var type = parameterInfos[i].ParameterType;
				if (type != typeof(IWrenSlot))
				{
					wrenSlots.Clear();
					return false;
				}

				WrenSlotAttribute slotAttribute = parameterInfos[i].GetCustomAttribute<WrenSlotAttribute>();
				if (slotAttribute == null) slotAttribute = new WrenSlotAttribute(name);
				else if (slotAttribute.Name == null) slotAttribute.Name = name;
				wrenSlots.Add(slotAttribute);
			}

			int expectedCount = WrenSignature.CorrectArgumentCount(attribute.Type, wrenSlots.Count);
			if (expectedCount != wrenSlots.Count)
				throw new TargetParameterCountException(
					$"{attribute.Type} expected {expectedCount} {nameof(IWrenSlot)} but {methodInfo.Name} has {wrenSlots.Count}");

			attribute.ArgumentCount = wrenSlots.Count;
			
			return true;
		}

		private static bool IsValidRawSignature(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length != 0) return false;

			return methodInfo.ReturnType == typeof(string);
		}
		
		private static T GetAttribute<T>(this MemberInfo info)
			where T : Attribute
		{
			return info.GetCustomAttribute(typeof(T)) as T;
		}

		private static List<T> GetAttributes<T>(this MemberInfo info)
			where T : Attribute
		{
			return info.GetCustomAttributes<T>().ToList();
		}

		private static List<(MemberInfo, AWrenCodeAttribute)> GetAttributedMembers(this Type type)
		{
			MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			List<(MemberInfo, AWrenCodeAttribute)> found = new List<(MemberInfo, AWrenCodeAttribute)>();

			for (int i = 0; i < members.Length; i++)
			{
				List<AWrenCodeAttribute> attributes = members[i].GetAttributes<AWrenCodeAttribute>();
				if (attributes != null && attributes.Count != 0) found.Add((members[i], attributes[0]));
			}

			return found;
		}

		#endregion
	}
}
