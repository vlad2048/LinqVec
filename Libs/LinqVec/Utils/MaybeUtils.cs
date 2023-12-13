namespace LinqVec.Utils;

public static class MaybeUtils
{
	public static Option<T> FirstOrOption<T>(this IEnumerable<T> src)
	{
		var arr = src.ToArr();
		return (arr.Length > 0) switch
		{
			true => Some(arr[0]),
			false => None
		};
	}

	public static Option<T> Aggregate<T>(params Option<T>[] arr)
	{
		foreach (var elt in arr)
			if (elt.IsSome)
				return elt;
		return None;
	}



	/*public static Maybe<T> Aggregate<T>(params Maybe<T>[] arr)
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
	}*/
}