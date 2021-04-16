using System;
using NUnit.Framework;

namespace Wrenit.UnitTests
{
	[TestFixture]
	public class AssertTest : TestsBase
	{
		[Test]
		public void AssertT()
		{
			var vm = new WrenVm();
			#if DEBUG
			Assert.Throws<IndexOutOfRangeException>(() => vm.GetSlotBool(3));
			#else		
			// exceptions dont get caught before going to native code
			Assert.Throws<AccessViolationException>(() => vm.GetSlotBool(3));
			#endif
		}
	}
}
