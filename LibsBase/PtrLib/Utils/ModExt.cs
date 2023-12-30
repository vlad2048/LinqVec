using ReactiveVars;

namespace PtrLib.Utils;



static class ModExt
{
	public static T Apply<T>(this T v, IRoVar<Option<Mod<T>>> mod) =>
		mod.V.Match(
			e => v.Apply(e),
			() => v
		);

	public static T Apply<T>(this T v, Mod<T> mod) => mod.Fun.V(v);
}