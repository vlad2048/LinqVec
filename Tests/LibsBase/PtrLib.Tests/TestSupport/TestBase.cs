using ReactiveVars;
using Shouldly;

namespace PtrLib.Tests.TestSupport;

class TestBase
{
    protected Disp D = null!;
    private bool dispCheckEnabled;

    protected void DisableDispCheck() => dispCheckEnabled = false;

    [SetUp]
    public void Setup()
    {
	    ResetDispTrackingForTestStart();
		D = MkD();
        dispCheckEnabled = true;

    }

    [TearDown]
    public void TearDown()
    {
        D.Dispose();
        if (dispCheckEnabled)
			LogAndTellIfThereAreUndisposedDisps(false).ShouldBeFalse();
    }
}

/*
static class TestDispExt
{
	public static T ExcludeD<T>(this T t) where T : IHasDisp
	{
        StopTrackingDisp(t.D);
        return t;
	}
}
*/