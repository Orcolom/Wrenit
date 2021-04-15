using Wrenit.Utilities;

namespace Wrenit.Shared
{
	[WrenModule]
	public class Constants
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
			[WrenMethod(WrenMethodType.StaticMethod, "current")]
			private static void GetVersion(WrenVm vm)
			{
				vm.EnsureSlots(1);
				vm.SetSlotString(0, Wren.WrenVersionString);
			}
		}
	}
}
