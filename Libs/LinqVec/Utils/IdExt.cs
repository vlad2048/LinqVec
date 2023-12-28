using LinqVec.Structs;

namespace LinqVec.Utils;

public static class IIdExt
{
	public static T[] Add<T>(this T[] xs, T x) where T : IId
	{
		if (xs.Any(e => e.Id == x.Id)) throw new ArgumentException();
		return xs.ToList().Append(x).ToArray();
	}
	public static T[] Del<T>(this T[] xs, Guid xId) where T : IId
	{
		if (xs.All(e => e.Id != xId)) throw new ArgumentException();
		var idx = xs.Idx(xId);
		return xs.Take(idx).Concat(xs.Skip(idx + 1)).ToArray();
	}
	public static T[] Set<T>(this T[] xs, T x) where T : IId
	{
		if (xs.All(e => e.Id != x.Id)) throw new ArgumentException();
		var idx = xs.Idx(x);
		return xs.Take(idx).Append(x).Concat(xs.Skip(idx + 1)).ToArray();
	}
	public static T[] Set<T>(this T[] xs, Guid xId, Func<T, T> fun) where T : IId
	{
		if (xs.All(e => e.Id != xId)) throw new ArgumentException();
		var idx = xs.Idx(xId);
		return xs.Take(idx).Append(fun(xs[idx])).Concat(xs.Skip(idx + 1)).ToArray();
	}
	public static T Get<T>(this T[] xs, Guid xId) where T : IId
	{
		if (xs.All(e => e.Id != xId)) throw new ArgumentException();
		var idx = xs.Idx(xId);
		return xs[idx];
	}

	private static int Idx<T>(this T[] xs, T x) where T : IId => xs.Idx(x.Id);

	private static int Idx<T>(this T[] xs, Guid xId) where T : IId
	{
		for (var i = 0; i < xs.Length; i++)
			if (xs[i].Id == xId)
				return i;
		throw new ArgumentException();
	}


	public static Option<T> MayGet<T>(this T[] xs, Guid xId) where T : IId => xs.FirstOrOption(e => e.Id == xId);
}
