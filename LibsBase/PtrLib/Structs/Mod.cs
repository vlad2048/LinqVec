using ReactiveVars;

namespace PtrLib.Structs;

public sealed record Mod<T>(
	string Name,
	bool Apply,
	IRoVar<Func<T, T>> Fun
)
{
	public override string ToString() => $"Mod({Name}) apply:{Apply}";

	public static readonly Mod<T> Empty = new("Empty", false, Var.MakeConst<Func<T, T>>(e => e));
}



static class ModExt
{
	public static T Apply<T>(this T v, IRoVar<Option<Mod<T>>> mod) =>
		mod.V.Match(
			e => v.Apply(e),
			() => v
		);

	public static T Apply<T>(this T v, Mod<T> mod) => mod.Fun.V(v);
}