using System;
using System.Collections.Generic;
using Wrenit.Interop;

namespace Wrenit
{

	public interface IWrenUtility
	{
		public void Bind(WrenConfig config);
		public void UnBind(WrenConfig config);
	}

	[Flags]
	public enum WrenModules
	{
		None = 0,
		ModuleMeta = 1,
		ModuleRandom = 2,
		All = ModuleMeta & ModuleRandom,
	}
	
	/// <summary>
	/// Configuration used to handle wren callbacks
	/// </summary>
	public class WrenConfig
	{
		/// <summary>
		/// lift of bindable elements bound to this config 
		/// </summary>
		private List<IWrenUtility> _bindables;
		
		/// <inheritdoc cref="InteropWrenConfiguration.InitialHeapSize"/>
		public ulong InitialHeapSize;

		/// <inheritdoc cref="InteropWrenConfiguration.MinHeapSize"/>
		public ulong MinHeapSize;

		/// <inheritdoc cref="InteropWrenConfiguration.HeapGrowthPercent"/>
		public int HeapGrowthPercent;

		/// <inheritdoc cref="InteropWrenConfiguration.WriteFn"/>
		public WrenWrite WriteHandler;
		
		/// <inheritdoc cref="InteropWrenConfiguration.ErrorFn"/>
		public WrenError ErrorHandler;
		
		/// <summary>
		///	<para>
		///		The callback Wren uses to resolve a module name.
		/// </para>
		///
		/// <para>
		/// 	Some host applications may wish to support "relative" imports, where the
		/// 	meaning of an import string depends on the module that contains it. To
		/// 	support that without baking any policy into Wren itself, the VM gives the
		/// 	host a chance to resolve an import string.
		/// </para>
		///
		/// <para>
		/// 	Before an import is loaded, it calls this, passing in the name of the
		/// 	module that contains the import and the import string. The host app can
		/// 	look at both of those and produce a new "canonical" string that uniquely
		/// 	identifies the module. This string is then used as the name of the module
		/// 	going forward. It is what is passed to <see cref="F:InterlopWrenConfiguration.LoadModuleFn"/>, how duplicate
		/// 	imports of the same module are detected, and how the module is reported in
		/// 	stack traces.
		/// </para>
		///
		/// <para>
		/// 	If you leave this function null, then the original import string is
		/// 	treated as the resolved string.
		/// </para>
		///
		/// <para>
		/// 	If an import cannot be resolved by the embedder, it should return null and
		/// 	Wren will report that as a runtime error.
		/// </para>
		///
		/// </summary>
		public WrenResolveModule ResolveModuleHandler;
		
		/// <summary>
		/// The callback Wren uses to load a module.
		///
		/// <para>
		/// 	Since Wren does not talk directly to the file system, it relies on the
		/// 	embedder to physically locate and read the source code for a module. The
		/// 	first time an import appears, Wren will call this and pass in the name of
		/// 	the module being imported. The VM should return the source code for that
		/// 	module.
		/// </para>
		///
		/// <para>
		/// 	This will only be called once for any given module name. Wren caches the
		/// 	result internally so subsequent imports of the same module will use the
		/// 	previous source and not call this.
		/// </para>
		///
		/// <para>
		/// 	If a module with the given name could not be found by the embedder, it
		/// 	should return NULL and Wren will report that as a runtime error.
		/// </para>
		/// </summary>
		public WrenLoadModule LoadModuleHandler;

		/// <summary>
		///	The callback Wren uses to find a foreign method and bind it to a class.
		///
		/// <para>
		/// 	When a foreign method is declared in a class, this will be called with the
		/// 	foreign method's module, class, and signature when the class body is
		/// 	executed. It should return a <see cref="WrenForeignMethod0"/> to the foreign function that will be
		/// 	bound to that method.
		/// </para>
		///
		/// <para>
		/// 	If the foreign function could not be found, this should return null and
		/// 	Wren will report it as runtime error.
		/// </para>
		/// </summary>
		public WrenBindForeignMethod BindForeignMethodHandler;

		/// <summary>
		/// The callback Wren uses to find a foreign class and get its foreign methods.
		///
		/// <para>
		/// 	When a foreign class is declared, this will be called with the class's
		/// 	module and name when the class body is executed. It should return the
		/// 	foreign functions uses to allocate and (optionally) finalize the
		/// 	the foreign object when an instance is created.
		/// </para>
		/// </summary>
		public WrenBindForeignClass BindForeignClassHandler;

		/// <summary>
		/// optional modules
		/// </summary>
		public WrenModules OptionalModules;
		
		/// <summary>
		/// add a utility class to this config. should be called from inside of <see cref="IWrenUtility.Bind"/>
		/// </summary>
		public void AddToCache(IWrenUtility utility)
		{
			_bindables ??= new List<IWrenUtility>();
			_bindables.Add(utility);
		}

		/// <summary>
		/// remove a utility class from this config. should be called from inside of <see cref="IWrenUtility.UnBind"/>
		/// </summary>
		public void RemoveFromCache(IWrenUtility utility)
		{
			_bindables?.Remove(utility);
		}
		
		public WrenConfig()
		{
			Wren.Initialize();
			
			OptionalModules = WrenModules.All;
			HeapGrowthPercent = Wren.DefaultConfig.HeapGrowthPercent;
			InitialHeapSize = Wren.DefaultConfig.InitialHeapSize.ToUInt64();
			MinHeapSize = Wren.DefaultConfig.InitialHeapSize.ToUInt64();
		}
	}
}
