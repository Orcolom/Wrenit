using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Wrenit.Utilities
{
	struct Signature
	{
		public readonly (int, int) Arguments;
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
			{MethodType.Method, new Signature(FormatNameArg, (0, -1))},
			{
				MethodType.StaticMethod,
				new Signature(FormatCustomNameArg, (0, -1), customValue: implement => implement ? "static" : "")
			},
			{
				MethodType.Construct,
				new Signature(FormatCustomNameArg, (0, -1), customValue: implement => implement ? "construct" : "init")
			},
			{MethodType.RangeInclusive, new Signature(FormatNameArg, (1, 1), "..")},
			{MethodType.RangeExclusive, new Signature(FormatNameArg, (1, 1), "...")},
			{MethodType.Times, new Signature(FormatNameArg, (1, 1), "*")},
			{MethodType.Divide, new Signature(FormatNameArg, (1, 1), "/")},
			{MethodType.Modulo, new Signature(FormatNameArg, (1, 1), "%")},
			{MethodType.Plus, new Signature(FormatNameArg, (1, 1), "+")},
			{MethodType.Minus, new Signature(FormatNameArg, (1, 1), "-")},
			{MethodType.BitwiseLeftShift, new Signature(FormatNameArg, (1, 1), "<<")},
			{MethodType.BitwiseRightShift, new Signature(FormatNameArg, (1, 1), ">>")},
			{MethodType.BitwiseXor, new Signature(FormatNameArg, (1, 1), "^")},
			{MethodType.BitwiseOr, new Signature(FormatNameArg, (1, 1), "|")},
			{MethodType.BitwiseAnd, new Signature(FormatNameArg, (1, 1), "&")},
			{MethodType.SmallerThen, new Signature(FormatNameArg, (1, 1), "<")},
			{MethodType.SmallerEqualThen, new Signature(FormatNameArg, (1, 1), "<=")},
			{MethodType.BiggerThen, new Signature(FormatNameArg, (1, 1), ">")},
			{MethodType.BiggerEqualThen, new Signature(FormatNameArg, (1, 1), ">=")},
			{MethodType.Equal, new Signature(FormatNameArg, (1, 1), "==")},
			{MethodType.NotEqual, new Signature(FormatNameArg, (1, 1), "!=")},
			{MethodType.Is, new Signature(FormatNameArg, (1, 1), "is")},
			{MethodType.FieldGetter, new Signature(FormatName, (0, 0))},
			{MethodType.Inverse, new Signature(FormatName, (0, 0), "-")},
			{MethodType.Not, new Signature(FormatName, (0, 0), "!")},
			{MethodType.Tilda, new Signature(FormatName, (0, 0), "~")},
			{MethodType.FieldSetter, new Signature("{1}=({2})", (1, 1))},
			{
				MethodType.SubScriptSetter,
				new Signature("[{2}]=({0})", (1, -1), customValue: implement => implement ? "value" : "_")
			},
			{MethodType.SubScriptGetter, new Signature("[{2}]", (1, -1))},
		};

		private Signature(string format, (int, int) arguments, string customName = null,
			Func<bool, string> customValue = null)
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

	public abstract class AWrenCodeAttribute : Attribute { }

	public abstract class AWrenMetaAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class WrenAttributeAttribute : Attribute
	{
		public readonly string Group;
		public readonly string Key;
		public readonly object Value;
		public bool RuntimeAccess;

		public WrenAttributeAttribute(string group, string key) : this(group, key, (object) null) { }
		public WrenAttributeAttribute(string group, string key, bool value) : this(group, key, (object) value) { }
		public WrenAttributeAttribute(string group, string key, string value) : this(group, key, (object) value) { }
		public WrenAttributeAttribute(string group, string key, double value) : this(group, key, (object) value) { }

		private WrenAttributeAttribute(string group, string key, object value)
		{
			Group = group;
			Value = value;
			Key = key;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WrenModuleAttribute : AWrenCodeAttribute
	{
		public string Name;

		public WrenModuleAttribute() { }

		public WrenModuleAttribute(string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WrenClassAttribute : AWrenCodeAttribute
	{
		public readonly string Name;

		public WrenClassAttribute() { }

		public WrenClassAttribute(string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenAllocatorAttribute : AWrenCodeAttribute { }

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenFinalizerAttribute : AWrenCodeAttribute { }

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenManualSourceAttribute : AWrenCodeAttribute { }

	[AttributeUsage(AttributeTargets.Method)]
	public class WrenMethodAttribute : AWrenCodeAttribute
	{
		public readonly string Name;
		public readonly int ArgumentCount;
		public readonly MethodType Type;

		public WrenMethodAttribute(MethodType type)
		{
			ArgumentCount = 0;
			Type = type;
		}

		public WrenMethodAttribute(MethodType type, int argumentCount = 0)
		{
			ArgumentCount = Wren.CorrectArgumentCount(type, argumentCount);
			Type = type;
		}

		public WrenMethodAttribute(MethodType type, string name = null, int argumentCount = 0)
		{
			Name = name;
			ArgumentCount = Wren.CorrectArgumentCount(type, argumentCount);
			Type = type;
		}
	}

	public static class WrenBuilder
	{
		private static Dictionary<Type, WrenModule> _modules = new Dictionary<Type, WrenModule>();

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

		public static WrenModule Build<T>()
		{
			Type moduleType = typeof(T);
			if (_modules.ContainsKey(moduleType))
			{
				return _modules[moduleType];
			}

			WrenModuleAttribute moduleAttribute = moduleType.GetAttribute<WrenModuleAttribute>();
			if (moduleAttribute == null) return null;

			StringBuilder sb = new StringBuilder();
			sb.Append("// auto generated by wrenit \n\n");

			List<WrenClass> classes = new List<WrenClass>();

			List<(MemberInfo, AWrenCodeAttribute)> members = moduleType.GetAttributedMembers();

			for (int i = 0; i < members.Count; i++)
			{
				switch (members[i].Item2)
				{
					case WrenClassAttribute classAttribute:
						classes.Add(BuildClass(members[i].Item1 as Type, classAttribute, sb));
						break;

					case WrenManualSourceAttribute rawSourceAttribute:
						AppendManualSource(members[i].Item1 as MethodInfo, sb);
						break;
				}
			}

			string source = sb.ToString();
			WrenModule module = new WrenModule(moduleAttribute.Name ?? moduleType.Name, source, classes);
			_modules.Add(moduleType, module);
			return module;
		}

		private static WrenClass BuildClass(Type classType, WrenClassAttribute classAttribute, StringBuilder sb)
		{
			List<(MemberInfo, AWrenCodeAttribute)> attributedMembers = classType.GetAttributedMembers();

			(MemberInfo, AWrenCodeAttribute) validAlloc = attributedMembers.Find(tuple =>
				tuple.Item2 is WrenAllocatorAttribute && IsValidMethodSignature(tuple.Item1 as MethodInfo));

			(MemberInfo, AWrenCodeAttribute) validFin = attributedMembers.Find(tuple =>
				tuple.Item2 is WrenFinalizerAttribute && IsValidFInalizerSignature(tuple.Item1 as MethodInfo));

			List<WrenAttributeAttribute> usedAttributes = new List<WrenAttributeAttribute>();

			string className = classAttribute.Name ?? classType.Name;

			AppendWrenAttributes(classType.GetAttributes<WrenAttributeAttribute>(), sb, 0);
			if (validAlloc.Item1 != null)
			{
				sb.Append("foreign ");
			}

			sb.Append($"class {className} {{\n");

			List<WrenMethod> methods = new List<WrenMethod>();
			List<WrenMethodAttribute> methodAttributes = new List<WrenMethodAttribute>();

			WrenForeignMethod allocator = null;
			WrenFinalizer finalizer = null;
			if (validAlloc.Item1 != null)
			{
				allocator =
					Delegate.CreateDelegate(typeof(WrenForeignMethod), validAlloc.Item1 as MethodInfo) as WrenForeignMethod;
			}

			if (validFin.Item1 != null)
			{
				finalizer =
					Delegate.CreateDelegate(typeof(WrenFinalizer), validFin.Item1 as MethodInfo) as WrenFinalizer;
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

						if (IsValidMethodSignature(method) == false) continue;

						string name = methodAttribute.Name ?? method.Name;
						if (IsOriginalAttribute(methodAttributes, methodAttribute, name) == false) continue;

						methodAttributes.Add(methodAttribute);

						WrenMethod wrenMethod = new WrenMethod(methodAttribute.Type, name, methodAttribute.ArgumentCount,
							Delegate.CreateDelegate(typeof(WrenForeignMethod), method) as WrenForeignMethod);

						AppendWrenAttributes(method.GetAttributes<WrenAttributeAttribute>(), sb, 1);

						sb.Append("\t");
						sb.Append(Wren.CreateSignature(methodAttribute.Type, name, methodAttribute.ArgumentCount,
							SignatureStyle.ForeignImplementation));
						sb.Append("\n");

						methods.Add(wrenMethod);
						break;
				}
			}

			sb.Append("}");

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

		private static void AppendManualSource(MethodInfo methodInfo, StringBuilder sb)
		{
			if (IsValidRawSignature(methodInfo) == false) return;

			sb.Append($"\n// begin manual source: {methodInfo.Name}\n");
			sb.Append((string) methodInfo.Invoke(null, null));
			sb.Append("\n// end manual source\n\n");
		}

		private static bool IsValidFInalizerSignature(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length != 1) return false;

			return parameterInfos[0].ParameterType == typeof(WrenForeignObject);
		}

		private static bool IsValidMethodSignature(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length != 1) return false;

			return parameterInfos[0].ParameterType == typeof(WrenVm);
		}

		private static bool IsValidRawSignature(MethodInfo methodInfo)
		{
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			if (parameterInfos.Length != 0) return false;

			return methodInfo.ReturnType == typeof(string);
		}

		private static void AppendIndents(int indents, StringBuilder sb)
		{
			for (int i = 0; i < indents; i++)
			{
				sb.Append("\t");
			}
		}

		private static void AppendWrenAttributes(List<WrenAttributeAttribute> attributes, StringBuilder sb, int indents)
		{
			if (attributes.Count == 0) return;
			List<WrenAttributeAttribute> usedAttributes = new List<WrenAttributeAttribute>();

			sb.Append("\n");
			for (int i = 0; i < attributes.Count; i++)
			{
				WrenAttributeAttribute itAttribute = attributes[i];
				if (usedAttributes.Contains(itAttribute)) continue;

				AppendIndents(indents, sb);
				sb.Append("#");
				if (itAttribute.RuntimeAccess) sb.Append("!");
				if (string.IsNullOrEmpty(itAttribute.Group) == false)
				{
					sb.Append(itAttribute.Group);
					sb.Append("(\n");

					List<WrenAttributeAttribute> attributesOfGroup = attributes.FindAll(searchAttribute =>
					{
						if (usedAttributes.Contains(searchAttribute)) return false;
						if (string.IsNullOrEmpty(searchAttribute.Group)) return false;
						if (searchAttribute.RuntimeAccess != itAttribute.RuntimeAccess) return false;

						return searchAttribute.Group == itAttribute.Group;
					});

					usedAttributes.AddRange(attributesOfGroup);

					for (int j = 0; j < attributesOfGroup.Count; j++)
					{
						AppendIndents(indents + 1, sb);
						AppendWrenAttribute(attributesOfGroup[j], sb);
						if (j + 1 < attributesOfGroup.Count) sb.Append(",");
						sb.Append("\n");
					}

					AppendIndents(indents, sb);
					sb.Append(")\n");
				}
				else
				{
					AppendWrenAttribute(itAttribute, sb);
					sb.Append("\n");
				}
			}
		}

		private static void AppendWrenAttribute(WrenAttributeAttribute attribute, StringBuilder sb)
		{
			sb.Append(attribute.Key);
			if (attribute.Value == null) return;

			sb.Append(" = ");
			switch (attribute.Value)
			{
				case double d:
					sb.Append(d.ToString(CultureInfo.InvariantCulture));
					return;

				case bool b:
					sb.Append(b ? "true" : "false");
					return;

				case string s:
					sb.Append($"\"{s}\"");
					return;
			}
		}
	}
}
