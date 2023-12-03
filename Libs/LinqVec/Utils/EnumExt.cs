namespace LinqVec.Utils;

public static class EnumExt
{
	public static T[] Add<T>(this T[] arr, T e) => arr.ToList().Append(e).ToArray();

	public static T[] ChangeIdx<T>(this T[] arr, int idx, Func<T, T> fun)
	{
		var list = arr.Take(idx).ToList();
		list.Add(fun(arr[idx]));
		list.AddRange(arr.Skip(idx + 1));
		return list.ToArray();
	}

	public static T[] RemoveIdx<T>(this T[] arr, int idx)
	{
		var list = arr.Take(idx).ToList();
		list.AddRange(arr.Skip(idx + 1));
		return list.ToArray();
	}
}