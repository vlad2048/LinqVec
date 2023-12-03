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
}