using Wrenit.Utilities;

namespace Wrenit.Shared
{
	[WrenModule]
	public class ConstantsModule
	{
		[WrenManualSource]
		private static string Raw()
		{
			return @"
var PI = 3.14159
var HelloWorld = ""Hello World""
var Wrapper = ""Wrenit""
";
		}
		
		[WrenClass]
		public static class Version
		{
			[WrenMethod(WrenMethodType.StaticMethod, "asString")]
			private static void VersionString(WrenVm vm)
			{
				vm.EnsureSlots(1);
				vm.SetSlotString(0, Wren.WrenVersionString);
			}
			
			[WrenMethod(WrenMethodType.StaticMethod, "asMonotone")]
			private static void VersionNumber(WrenVm vm)
			{
				vm.EnsureSlots(1);
				vm.SetSlotInt(0, Wren.WrenVersionNumber);
			}
		}
	}
}
