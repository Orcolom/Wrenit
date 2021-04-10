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
var PI = 3.1415
var HelloWorld = ""Hello World""
";
		}
	}
}
