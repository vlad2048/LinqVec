using LinqVec.Utils;

namespace LinqVec.Logic;


/*
public interface IId
{
	Guid Id { get; }
}

public interface ILens<D, K> where K : IId
{
	bool Exists(D dad);
	K Get(D dad);
	D Set(D dad, K kid);
	D Remove(D dad);
}

public class ArrayLens<D, K>(
	Guid kidId,
	Func<D, K[]> getArr,
	Func<D, K[], D> setArr
) : ILens<D, K> where K : IId
{
	public bool Exists(D dad) => getArr(dad).Count(e => e.Id == kidId) == 1;
	public K Get(D dad) => getArr(dad).Single(e => e.Id == kidId);
	public D Set(D dad, K kid) => setArr(dad, getArr(dad).ChangeId(kidId, _ => kid));
	public D Remove(D dad)
	{
		if (!Exists(dad)) throw new ArgumentException();
		var res = setArr(dad, getArr(dad).RemoveId(kidId));
		if (Exists(res)) throw new ArgumentException();
		return res;
	}
}

public static class LensUtils
{
	public static D Change<D, K>(this ILens<D, K> lens, D dad, Func<K, K> kidFun) where K : IId
	{
		var kid = lens.Get(dad);
		return lens.Set(dad, kidFun(kid));
	}

	public static ILens<D, G> Compose<D, K, G>(ILens<D, K> f, ILens<K, G> g) where K : IId where G : IId =>
		new ComposeLens<D, G>(
			dad =>
			{
				if (!f.Exists(dad)) return false;
				var kid = f.Get(dad);
				if (!g.Exists(kid)) return false;
				return true;
			},
			dad => g.Get(f.Get(dad)),
			(dad, grandKid) =>
			{
				var kidPrev = f.Get(dad);
				var kidNext = g.Set(kidPrev, grandKid);
				var dadNext = f.Set(dad, kidNext);
				return dadNext;
			},
			dad =>
			{
				var kidPrev = f.Get(dad);
				var kidNext = g.Remove(kidPrev);
				var dadNext = f.Set(dad, kidNext);
				return dadNext;
			}
		);

	private class ComposeLens<D, K>(
		Func<D, bool> exists,
		Func<D, K> get,
		Func<D, K, D> set,
		Func<D, D> remove
	) : ILens<D, K> where K : IId
	{
		public bool Exists(D dad) => exists(dad);
		public K Get(D dad) => get(dad);
		public D Set(D dad, K kid) => set(dad, kid);
		public D Remove(D dad) => remove(dad);
	}
}
*/