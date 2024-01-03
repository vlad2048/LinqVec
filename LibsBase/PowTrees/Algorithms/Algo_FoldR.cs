namespace PowTrees.Algorithms;

public static class Algo_FoldR
{
	/// <summary>
	/// Map a tree recursively.
	/// For each node, we use the node and the mapped children as input
	/// </summary>
	public static TNod<U> FoldR<T, U>(
		this TNod<T> root,
		Func<T, IReadOnlyList<U>, U> fun
	)
	{
		TNod<U> Recurse(TNod<T> node)
		{
			var foldedChildren = node.Children
				.Select(Recurse).ToArray();
			var foldedNode = Nod.Make(
				fun(node.V, foldedChildren.Select(e => e.V).ToArray()),
				foldedChildren
			);
			return foldedNode;
		}

		return Recurse(root);
	}

	/*public static TNod<U> FoldR<T, U>(
		this TNod<T> root,
		Func<TNod<T>, IReadOnlyList<TNod<U>>, TNod<U>> fun
	)
	{
		TNod<U> Recurse(TNod<T> node)
		{
			var foldedChildren = node.Children
				.Select(Recurse).ToArray();
			var foldedNode = Nod.Make(
				fun(node, foldedChildren.Select(e => e).ToArray()),
				foldedChildren
			);
			return foldedNode;
		}

		return Recurse(root);
	}*/

	public static Dictionary<T, U> FoldRDict<T, U>(
		this TNod<T> root,
		Func<T, IReadOnlyList<U>, U> fun
	)
		where T : notnull
		=>
			root.Zip(root.FoldR(fun))
				.ToDictionary(
					e => e.First.V,
					e => e.Second.V
				);

	public static Dictionary<TNod<T>, U> FoldRDictN<T, U>(
		this TNod<T> root,
		Func<T, IReadOnlyList<U>, U> fun
	)
		=>
			root.Zip(root.FoldR(fun))
				.ToDictionary(
					e => e.First,
					e => e.Second.V
				);

	public static Dictionary<TNod<T>, U> FoldRLateDictN<T, U>(
		this TNod<T> root,
		Func<T, U> genFun,
		Func<U, U, U> recFun,
		Func<IReadOnlyList<U>, U> combFun
	)
		=>
			root.FoldRDictN<T, (U, U)>(
					(n, t) => (
						combFun(t.Select(e => recFun(e.Item1, e.Item2)).ToArray()),
						genFun(n)
					)
				)
				.MapValue(e => e.Item1);

	/*public static Dictionary<TNod<T>, (U, U)> FoldRLateDictN<T, U>(
	 
		this TNod<T> root,
		Func<T, U> genFun,
		Func<U, U, U> recFun,
		Func<IReadOnlyList<U>, U> combFun
	)
		=>
			root.FoldRDictN<T, (U, U)>(
				(n, t) => (
					combFun(t.Select(e => recFun(e.Item1, e.Item2)).ToArray()),
					genFun(n)
				)
			);*/


	private static Dictionary<K, V> MapValue<K, U, V>(this Dictionary<K, U> dict, Func<U, V> mapFun) where K : notnull
		=>
			dict.ToDictionary(
				e => e.Key,
				e => mapFun(e.Value)
			);
}