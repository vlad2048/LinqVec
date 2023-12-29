using PtrLib.Structs;
using ReactiveVars;

namespace PtrLib.Tests.TestSupport;

class TestMod<T> : IDisposable
{
	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly bool apply;
	private readonly IRwVar<Func<T, T>> fun;

	public TestMod(bool apply, Disp d)
	{
		this.apply = apply;
		this.d = d;
		fun = Var.Make<Func<T, T>>(e => e, d);
	}

	public Func<T, T> Fun { set => fun.V = value; }

	public static implicit operator Mod<T>(TestMod<T> e) => new(
		$"Mod<{typeof(T).Name}>(apply:{e.apply})",
		e.apply,
		e.fun
	);
}

static class Makers
{
	public static Mod<T> MkMod<T>(bool apply, Func<T, T> fun) => new($"ConstMod({typeof(T).Name})", apply, Var.MakeConst(fun));
	public static TestMod<T> MkMod<T>(bool apply, Disp d) => new(apply, d);
}