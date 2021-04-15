using System;

namespace Wrenit.Utilities
{
	public abstract class AWrenCodeAttribute : Attribute { }

	public abstract class AWrenMetaAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class WrenAttributeAttribute : AWrenMetaAttribute
	{
		public readonly string Group;
		public readonly string Key;
		public readonly object Value;
		public bool RuntimeAccess;

		public WrenAttributeAttribute(string key) : this(null, key, (object) null) { }
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

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class WrenImportAttribute : AWrenMetaAttribute
	{
		public readonly string Module;
		public readonly Type ModuleType;
		public readonly string For;
		public readonly Type ForType;
		public readonly string As;

		public WrenImportAttribute(Type module, string @for = null, string @as = null)
		{
			ModuleType = module;
			For = @for;
			As = @as;
		}
		
		public WrenImportAttribute(Type module, Type @for = null, string @as = null)
		{
			ModuleType = module;
			ForType = @for;
			As = @as;
		}
		
		public WrenImportAttribute(string module, string @for = null, string @as = null)
		{
			Module = module;
			For = @for;
			As = @as;
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
		public readonly string Inherit;
		public readonly Type InheritType;

		public WrenClassAttribute() { }

		public WrenClassAttribute(string name)
		{
			Name = name;
		}
		
		public WrenClassAttribute(string name, string inherit)
		{
			Name = name;
			Inherit = inherit;
		}
		
		public WrenClassAttribute(string name, Type inherit)
		{
			Name = name;
			InheritType = inherit;
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
		public readonly WrenMethodType Type;

		public WrenMethodAttribute(WrenMethodType type)
		{
			ArgumentCount = 0;
			Type = type;
		}

		public WrenMethodAttribute(WrenMethodType type, int argumentCount = 0)
		{
			ArgumentCount = WrenSignature.CorrectArgumentCount(type, argumentCount);
			Type = type;
		}

		public WrenMethodAttribute(WrenMethodType type, string name = null, int argumentCount = 0)
		{
			Name = name;
			ArgumentCount = WrenSignature.CorrectArgumentCount(type, argumentCount);
			Type = type;
		}
	}
}
