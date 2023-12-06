using PowMaybe;

namespace LinqVec.Utils;

public static class MaybeUtils
{
	public static Maybe<T> Aggregate<T>(params Maybe<T>[] arr)
	{
		foreach (var elt in arr)
			if (elt.IsSome())
				return elt;
		return May.None<T>();
	}

	public static Maybe<T> SingleOrMaybe<T>(this T[] arr, Func<T, bool> predicate)
	{
		var cnt = arr.ToList().Count(predicate);
		if (cnt != 1) return May.None<T>();
		var elt = arr.Single(predicate);
		return May.Some(elt);
	}
}