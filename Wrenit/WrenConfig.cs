using Wrenit.Interlop;

namespace Wrenit
{
	public struct WrenConfig
	{
		// TODO: static nullables broke unit tests?
		private static WrenConfig _default;
		private static bool _hasDefaults;

		/// <inheritdoc cref="InterlopWrenConfiguration.InitialHeapSize"/>
		public ulong InitialHeapSize;

		/// <inheritdoc cref="InterlopWrenConfiguration.MinHeapSize"/>
		public ulong MinHeapSize;

		/// <inheritdoc cref="InterlopWrenConfiguration.HeapGrowthPercent"/>
		public int HeapGrowthPercent;

		/// <inheritdoc cref="InterlopWrenConfiguration.WriteFn"/>
		public WrenWrite WriteHandler;
		
		/// <inheritdoc cref="InterlopWrenConfiguration.ErrorFn"/>
		public WrenError ErrorHandler;
		
		/// <inheritdoc cref="InterlopWrenConfiguration.ResolveModuleFn"/>
		public WrenResolveModule ResolveModuleHandler;
		
		/// <inheritdoc cref="InterlopWrenConfiguration.LoadModuleFn"/>
		public WrenLoadModule LoadModuleHandler;

		/// <inheritdoc cref="InterlopWrenConfiguration.BindForeignMethodFn"/>
		public WrenBindForeignMethod BindForeignMethodHandler;

		/// <inheritdoc cref="InterlopWrenConfiguration.BindForeignClassFn"/>
		public WrenBindForeignClass BindForeignClassHandler;

		/// <summary>
		/// Get default values from the c api 
		/// </summary>
		/// <returns></returns>
		public static WrenConfig GetDefaults()
		{
			if (_hasDefaults) return _default;
			InterlopWrenConfiguration wrenConfig = new InterlopWrenConfiguration();
			WrenImport.wrenInitConfiguration(wrenConfig);

			_hasDefaults = true;
			_default = new WrenConfig()
			{
				HeapGrowthPercent = wrenConfig.HeapGrowthPercent,
				InitialHeapSize = wrenConfig.InitialHeapSize.ToUInt64(),
				MinHeapSize = wrenConfig.InitialHeapSize.ToUInt64(),
			};

			return _default;
		}
	}
}
