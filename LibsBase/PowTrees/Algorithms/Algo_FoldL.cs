namespace PowTrees.Algorithms;

public static class Algo_FoldL
{
	/// <summary>
	/// Map a tree recursively.
	/// For each node, we use the node and the mapped parent as input
	/// </summary>
	public static TNod<U> FoldL<T, U>(
		this TNod<T> root,
		Func<TNod<T>, U, U> fun,
		U seed
	)
	{
		TNod<U> Recurse(TNod<T> node, U mayMappedParentVal)
		{
			var mappedNodeVal = fun(node, mayMappedParentVal);
			var mappedChildren = node.Children.Select(child => Recurse(child, mappedNodeVal));
			var mappedNode = Nod.Make(mappedNodeVal, mappedChildren);
			return mappedNode;
		}

		return Recurse(root, seed);
	}


	public static TNod<U> FoldL_Parent<T, U>(
		this TNod<T> root,
		Func<T, U> get,
		Func<U, U, U> fun,
		U seed
	) =>
		root.FoldL(
			(nod, acc) =>
				fun(
					nod.ParentOr(get, seed),
					acc
				),
			seed
		);


	public static IReadOnlyDictionary<T, U> FoldL_Dict<T, U>(
		this TNod<T> root,
		Func<T, U, U> fun,
		U seed
	) where T : notnull
		=>
		root.Zip(root.FoldL((nod, acc) => fun(nod.V, acc), seed)).ToDictionary(e => e.First.V, e => e.Second.V);


	public static IReadOnlyDictionary<T, U> FoldL_Parent_Dict<T, U>(
		this TNod<T> root,
		Func<T, U> get,
		Func<U, U, U> fun,
		U seed
	) where T : notnull
		=>
		root.Zip(
			root.FoldL(
				(nod, acc) =>
					fun(
						nod.ParentOr(get, seed),
						acc
					),
				seed
			)
		).ToDictionary(e => e.First.V, e => e.Second.V);

	
	public static U ParentOr<T, U>(this TNod<T> nod, Func<T, U> fun, U seed) => nod.Parent switch
	{
		not null => fun(nod.Parent.V),
		null => seed
	};
}