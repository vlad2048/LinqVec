using ReactiveVars;

namespace RenderLib.Utils;

static class RxExt
{
	public static T D<T>(this (T, IDisposable) t, Disp d)
	{
		t.Item2.D(d);
		return t.Item1;
	}
	public static (T, U) D<T, U>(this (T, U, IDisposable) t, Disp d)
	{
		t.Item3.D(d);
		return (t.Item1, t.Item2);
	}
	public static (T, U, V) D<T, U, V>(this (T, U, V, IDisposable) t, Disp d)
	{
		t.Item4.D(d);
		return (t.Item1, t.Item2, t.Item3);
	}
}