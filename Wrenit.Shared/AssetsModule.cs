using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading;
using Wrenit.Utilities;

namespace Wrenit.Shared
{
	[WrenModule("Assets")]
	public class AssetsModule
	{
		private static Dictionary<int, AssetData> _assets = new Dictionary<int, AssetData>();
		private static int _ids;
		
		public class AssetData
		{
			public int Id;
			public string Path;
		}
		
		[WrenClass]
		public class AssetSystem
		{
			[WrenMethod(WrenMethodType.StaticMethod, 1)]
			[WrenAttribute("args", "a", "path of the asset")]
			private static void Load(WrenVm vm)
			{
				// load argument in slot 1
				if (vm.GetSlotType(1) != WrenValueType.String)
				{
					vm.SetSlotString(0, "Expected string type");
					vm.AbortFiber(0);
					return;
				}

				string path = vm.GetSlotString(1);
				
				// get Asset class variable from module Assets and store in slot 0
				if (vm.HasModule<AssetsModule>() == false)
				{
					vm.AbortFiber($"Cant find module {WrenBuilder.GetResolvedName<AssetsModule>()}");
					return;
				}

				if (vm.HasVariable<AssetsModule, Asset>() == false)
				{
					vm.AbortFiber($"Cant find name {WrenBuilder.GetResolvedName<Asset>()} in module {WrenBuilder.GetResolvedName<AssetsModule>()}");
					return;
				}
				
				vm.GetVariable<AssetsModule, Asset>(0);
				
				// create new foreign
				//		with class in slot 0
				//		store in slot 0
				Asset.Alloc(vm);
				var foreign = vm.GetSlotForeign<AssetData>(0);
				
				// fill in object
				foreign.TypedData.Path = path;
			}
		}
		
		[WrenClass]
		public class Asset
		{

			[WrenAllocator]
			internal static void Alloc(WrenVm vm)
			{
				var foreign = vm.SetSlotNewForeign(0, 0, new AssetData()
				{
					Id = ++_ids,
				});
				_assets.Add(foreign.TypedData.Id, foreign.TypedData);
			}

			[WrenFinalizer]
			internal static void Fin(WrenForeignObject fo)
			{
				_assets.Remove(fo.As<AssetData>().TypedData.Id);
			}

			[WrenMethod(WrenMethodType.FieldGetter)]
			[WrenAttribute("getter")]
			private static void path(WrenVm vm)
			{
				var foreign = vm.GetSlotForeign<AssetData>(0);
				vm.SetSlotString(0, foreign.TypedData.Path);
			}
			
			[WrenMethod(WrenMethodType.FieldGetter)]
			[WrenAttribute("getter")]
			private static void id(WrenVm vm)
			{
				var foreign = vm.GetSlotForeign<AssetData>(0);
				vm.SetSlotDouble(0, foreign.TypedData.Id);
			}
		}
	}
	
	
	[WrenModule("AssetsFail")]
	public class AssetsModule2
	{
		[WrenClass(null, typeof(AssetsModule.Asset))]
		public class ImageAsset
		{
			[WrenMethod(WrenMethodType.Construct)]
			public static void New() { }
		}
	}
}
