using System;
using System.IO;
using Wren.it.Interlop;

namespace Wren.it
{
	public static class Wrenit
	{
		#if DEBUG
		internal const string DllName = "wren_d.dll";
		#else
		internal const string DllName = "wren.dll";
		#endif

		private static readonly int[] WrenVersion = {0, 4, 0};
		private static readonly string WrenVersionString = $"{WrenVersion[0]}.{WrenVersion[1]}.{WrenVersion[2]}";
		private static readonly int WrenVersionNumber = WrenVersion[0] * 1000000 + WrenVersion[1] * 1000 + WrenVersion[2];

		private static bool _didInitializeCheck = false;
		
		public static bool Initialize()
		{
			if (_didInitializeCheck) return true;

			int version = WrenImport.wrenGetVersionNumber();

			int patch = version % 1000;
			int minor = ((version - patch) / 1000) % 1000;
			int major = ((version - (minor * 1000) - patch) / 1000000) % 1000;

			_didInitializeCheck = true;
			if (version == WrenVersionNumber) return true;

			throw new NotSupportedException(
				$"{DllName} with version {major}.{minor}.{patch} is not supported. Dll with version {WrenVersionString} needed");
		}
	}
}
