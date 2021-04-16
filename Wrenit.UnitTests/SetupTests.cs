using NUnit.Framework;
using Wrenit.Utilities;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class TestsBase
	{
		[SetUp]
		public void Set()
		{
			WrenBuilder.ClearCache();
		}
	}
}
