using System;
using System.Collections.Generic;
using System.Reflection;

namespace Wrenit.Utilities
{
	/// <summary>
	/// defines a wren module in c#
	/// </summary>
	public class WrenModule : IWrenUtility
	{
		/// <summary>
		/// name of the module
		/// </summary>
		public readonly string Name;
		
		/// <summary>
		/// the module source code
		/// </summary>
		public readonly string Source;

		internal readonly MemberInfo ReflectedFrom;
		
		/// <summary>
		/// list of classes in the module
		/// </summary>
		internal readonly Dictionary<string, WrenClass> Classes = new Dictionary<string, WrenClass>();

		// ReSharper disable once UnusedMember.Local
		private WrenModule() { }

		/// <summary>
		/// create a new module
		/// </summary>
		/// <param name="name">name of the module</param>
		/// <param name="source">source for the module</param>
		/// <param name="classes">list of classes</param>
		public WrenModule(string name, string source, params WrenClass[] classes)
		{
			Name = name;
			Source = source;
			for (int i = 0; i < classes.Length; i++)
			{
				Classes.Add(classes[i].Name, classes[i]);
			}
		}

		/// <summary>
		/// create a new module
		/// </summary>
		/// <param name="name">name of the module</param>
		/// <param name="source">source for the module</param>
		/// <param name="classes">list of classes</param>
		/// <param name="reflectedFrom">class this was created from</param>
		internal WrenModule(string name, string source, IReadOnlyList<WrenClass> classes, MemberInfo reflectedFrom)
		{
			Name = name;
			Source = source;
			ReflectedFrom = reflectedFrom;
			for (int i = 0; i < classes.Count; i++)
			{
				Classes.Add(classes[i].Name, classes[i]);
			}
		}

		/// <summary>
		/// find a class with <paramref name="name"/>
		/// </summary>
		/// <param name="name">name of the class to find</param>
		/// <returns></returns>
		public WrenClass FindClass(string name)
		{
			if (Classes.TryGetValue(name, out WrenClass cls) == false) return null;

			return cls;
		}

		/// <summary>
		/// bind the module to the config's <see cref="WrenConfig.BindForeignMethodHandler"/> and <see cref="WrenConfig.BindForeignClassHandler"/>
		/// </summary>
		/// <param name="config"></param>
		public void Bind(ref WrenConfig config)
		{
			config.AddToCache(this);
			config.LoadModuleHandler += LoadModuleHandler;
			config.BindForeignMethodHandler += BindForeignMethodHandler;
			config.BindForeignClassHandler += BindForeignClassHandler;
		}

		/// <summary>
		/// unbind the module from the config's <see cref="WrenConfig.BindForeignMethodHandler"/> and <see cref="WrenConfig.BindForeignClassHandler"/>
		/// </summary>
		/// <param name="config"></param>
		public void Unbind(ref WrenConfig config)
		{
			config.RemoveFromCache(this);
			config.LoadModuleHandler -= LoadModuleHandler;
			config.BindForeignMethodHandler -= BindForeignMethodHandler;
			config.BindForeignClassHandler -= BindForeignClassHandler;
		}

		private string LoadModuleHandler(WrenVm vm, string name)
		{
			return Name == name ? Source : null;
		}

		/// <inheritdoc cref="WrenConfig.BindForeignClassHandler"/>
		private WrenForeignClassBinding BindForeignClassHandler(WrenVm vm, string module, string className)
		{
			return FindClass(className)?.AsForeign();
		}

		/// <inheritdoc cref="WrenConfig.BindForeignMethodHandler"/>
		private WrenForeignMethodBinding BindForeignMethodHandler(WrenVm vm, string module, string className, bool isStatic, string signature)
		{
				return FindClass(className)?.FindMethod(signature, isStatic)?.MethodBinding;
		}
	}

	/// <summary>
	/// defines a wren class in c#
	/// </summary>
	public class WrenClass
	{
		/// <summary>
		/// name of the class
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// the allocator binding if present
		/// </summary>
		private readonly WrenForeignMethodBinding _allocator;
		
		/// <summary>
		/// the finalizer binding if present
		/// </summary>
		private readonly WrenFinalizerMethodBinding _finalizer;

		internal readonly MemberInfo ReflectedFrom;
		
		/// <summary>
		/// list of methods in the class
		/// </summary>
		private readonly List<WrenMethod> _methods = new List<WrenMethod>();

		/// <summary>
		/// does the class have an allocator function
		/// </summary>
		internal bool HasAllocator => _allocator != null;
		
		/// <summary>
		/// list of the methods
		/// </summary>
		internal List<WrenMethod> Methods => _methods;

		// ReSharper disable once UnusedMember.Local
		private WrenClass(){}
		
		/// <summary>
		/// create a new class
		/// </summary>
		/// <param name="name">name of the class</param>
		/// <param name="allocator">allocator if wanted</param>
		/// <param name="finalizer">finalizer if wanted</param>
		/// <param name="methods">list of methods to add</param>
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

		/// <summary>
		/// create a new class
		/// </summary>
		/// <param name="name">name of the class</param>
		/// <param name="allocator">allocator if wanted</param>
		/// <param name="finalizer">finalizer if wanted</param>
		/// <param name="methods">list of methods to add</param>
		/// <param name="reflectedFrom">reflected from member</param>
		internal WrenClass(string name, WrenForeignMethod allocator, WrenFinalizer finalizer, IEnumerable<WrenMethod> methods, MemberInfo reflectedFrom)
		{
			Name = name;
			ReflectedFrom = reflectedFrom;
			if (allocator != null)
			{
				_allocator = new WrenForeignMethodBinding(allocator);
				_finalizer = new WrenFinalizerMethodBinding(finalizer);
			}

			_methods.AddRange(methods);
		}

		/// <summary>
		/// find a method in the class with <paramref name="signature"/>
		/// </summary>
		/// <param name="signature">signature to find</param>
		/// <param name="isStatic">is the method static</param>
		public WrenMethod FindMethod(string signature, bool isStatic)
		{
			for (int i = 0; i < _methods.Count; i++)
			{
				WrenMethod m = _methods[i];
				if (m.Signature == signature && m.IsStatic == isStatic) return m;
			}

			return null;
		}

		/// <summary>
		/// return the class as a ForeignClassBinding
		/// </summary>
		/// <returns></returns>
		public WrenForeignClassBinding AsForeign()
		{
			return new WrenForeignClassBinding(_allocator, _finalizer);
		}
	}

	/// <summary>
	/// defines a wren method in c#
	/// </summary>
	public class WrenMethod
	{
		/// <summary>
		/// the method signature
		/// </summary>
		public readonly string Signature;
		
		/// <summary>
		/// is the method static
		/// </summary>
		public readonly bool IsStatic;
		
		/// <summary>
		/// the method binding
		/// </summary>
		public readonly WrenForeignMethodBinding MethodBinding;
		
		/// <summary>
		/// the method type
		/// </summary>
		private readonly WrenMethodType _type;

		internal MemberInfo ReflectedFrom;
		
		// ReSharper disable once UnusedMember.Local
		private WrenMethod() { }

		/// <summary>
		/// create a new method. will create a signatures based on <paramref name="type"/>, <paramref name="name"/> and <paramref name="argumentCount"/>
		/// </summary>
		/// <param name="type">type of the method</param>
		/// <param name="name">name of the method.</param>
		/// <param name="argumentCount">amount of arguments</param>
		/// <param name="method">method to bind</param>
		public WrenMethod(WrenMethodType type, string name, int argumentCount, WrenForeignMethod method)
		{
			Signature = WrenSignature.CreateSignature(type, name, argumentCount);
			IsStatic = type == WrenMethodType.StaticMethod;
			MethodBinding = new WrenForeignMethodBinding(method);
			_type = type;
		}
		
		/// <summary>
		/// create a new method. will create a signatures based on <paramref name="type"/>, <paramref name="name"/> and <paramref name="argumentCount"/>
		/// </summary>
		/// <param name="type">type of the method</param>
		/// <param name="name">name of the method.</param>
		/// <param name="argumentCount">amount of arguments</param>
		/// <param name="method">method to bind</param>
		internal WrenMethod(WrenMethodType type, string name, int argumentCount, WrenForeignMethod method, MemberInfo reflectedFrom)
		{
			Signature = WrenSignature.CreateSignature(type, name, argumentCount);
			IsStatic = type == WrenMethodType.StaticMethod;
			MethodBinding = new WrenForeignMethodBinding(method);
			ReflectedFrom = reflectedFrom;
			_type = type;
		}
	}
}
