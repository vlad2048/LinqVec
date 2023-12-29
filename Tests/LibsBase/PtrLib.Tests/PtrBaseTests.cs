using PtrLib.Tests.TestSupport;
using Shouldly;

namespace PtrLib.Tests;

class PtrBaseTests : TestBase
{
	[Test]
	public void _0_ModSet_Twice_Without_DisposeFirst()
	{
		var ptr = new PtrBase<int>(12, D);
		ptr.CheckHistory([12], [], 12);

		ptr.ModSet(MkMod<int>(false, e => e + 1));
		ptr.CheckHistory([12], [], 13);

		Should.Throw<ObjectDisposedException>(() => ptr.ModSet(MkMod<int>(false, e => e + 2)));
	}

	[Test]
	public void _1_ModSet_Twice_With_DisposeFirst()
	{
		var ptr = new PtrBase<int>(12, D);
		ptr.CheckHistory([12], [], 12);

		using (ptr.ModSet(MkMod<int>(false, e => e + 1)))
			ptr.CheckHistory([12], [], 13);
		ptr.CheckHistory([12], [], 12);

		ptr.ModSet(MkMod<int>(false, e => e + 2));
		ptr.CheckHistory([12], [], 14);
	}

	[Test]
	public void _2_ModSet_Twice_With_ApplyFirst()
	{
		var ptr = new PtrBase<int>(12, D);
		ptr.CheckHistory([12], [], 12);

		using (ptr.ModSet(MkMod<int>(true, e => e + 1)))
			ptr.CheckHistory([12], [], 13);
		ptr.CheckHistory([12, 13], [], 13);

		ptr.ModSet(MkMod<int>(false, e => e + 2));
		ptr.CheckHistory([12, 13], [], 15);
	}
}