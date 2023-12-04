using LinqVec.Logic;
using LinqVec.Structs;
using PowMaybe;

namespace LinqVec.Utils;

public static class EnumExt
{
	public static bool ContainsId<T>(this T[] arr, Guid id) where T : IId => arr.Count(e => e.Id == id) == 1;
	public static T[] Add<T>(this T[] arr, T e) => arr.ToList().Append(e).ToArray();

	//public static T GetId<T>(this T[] arr, Guid id) where T : IId => arr.Single(e => e.Id == id);

	public static Maybe<T> GetMayId<T>(this T[] arr, Guid id) where T : class, IId => arr.SingleOrMaybe(e => e.Id == id);

	public static T[] SetId<T>(this T[] arr, Guid id, T e) where T : IId
	{
		var idx = arr.SingleIndexWhere(e => e.Id == id);
		return arr.SetIdx(idx, e);
	}

	public static T[] ChangeId<T>(this T[] arr, Guid id, Func<T, T> fun) where T : IId
	{
		var idx = arr.SingleIndexWhere(e => e.Id == id);
		return arr.ChangeIdx(idx, fun);
	}

	public static T[] RemoveId<T>(this T[] arr, Guid id) where T : IId
	{
		var idx = arr.SingleIndexWhere(e => e.Id == id);
		return arr.RemoveIdx(idx);
	}

	public static T[] SetIdx<T>(this T[] arr, int idx, T e)
	{
		var list = arr.Take(idx).ToList();
		list.Add(e);
		list.AddRange(arr.Skip(idx + 1));
		return list.ToArray();
	}

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

	private static int SingleIndexWhere<T>(this T[] arr, Func<T, bool> predicate) => arr.Select((e, i) => (e, i)).Single(t => predicate(t.e)).i;
}