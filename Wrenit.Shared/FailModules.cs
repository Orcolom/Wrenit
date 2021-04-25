using System;
using Wrenit.Utilities;

namespace Wrenit.Shared
{
	[WrenModule]
	[WrenImport((string)null)]
	public class NullImportModule { }
	
	[WrenModule]
	[WrenImport((Type)null)]
	public class NullImport2Module { }
	
	[WrenModule]
	[WrenImport(typeof(NullImportModule))]
	public class MissingImportModule { }
}
