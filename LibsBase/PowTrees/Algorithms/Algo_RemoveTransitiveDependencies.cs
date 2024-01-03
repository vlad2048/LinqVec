namespace PowTrees.Algorithms;

public static class Algo_RemoveTransitiveDependencies
{
	public static TNod<T> RemoveTransitiveDependencies<T>(this TNod<T> root, IEqualityComparer<T>? cmpFun = null)
	{
		var cmp = cmpFun ?? EqualityComparer<T>.Default;
		var descendentMap = root.ToDictionary(e => e, e => GetAllDescendents(e, cmp));
		bool IsInDescendents(IEnumerable<TNod<T>> parents, TNod<T> child) => parents.Any(parent => descendentMap[parent].Contains(child.V));

		TNod<T> Rec(TNod<T> n)
		{
			var listTodo = new Queue<TNod<T>>(n.Children);
			var listDone = new List<TNod<T>>();
			//var listRejected = new List<TNod<T>>();
			var parents = n.GetParents(cmp);

			while (listTodo.TryDequeue(out var child))
			{
				var isSomewhereElse =
					parents.Contains(child.V) ||
					IsInDescendents(listTodo, child) ||
					IsInDescendents(listDone, child);
				if (isSomewhereElse)
				{
					//listRejected.Add(child);
				}
				else
				{
					listDone.Add(child);
				}
			}

			return Nod.Make(n.V, listDone.Select(Rec));
		}

		return Rec(root);
	}

	private static HashSet<T> GetAllDescendents<T>(this TNod<T> root, IEqualityComparer<T> cmp) => new(root.Skip(1).Select(e => e.V), cmp);

	private static HashSet<T> GetParents<T>(this TNod<T> root, IEqualityComparer<T> cmp)
	{
		var set = new HashSet<T>(cmp);
		var node = root.Parent;
		while (node != null)
		{
			set.Add(node.V);
			node = node.Parent;
		}
		return set;
	}


	/*public static TNod<T> RemoveTransitiveDependencies<T>(this TNod<T> root, IEqualityComparer<T>? cmpFun = null)
	{
		bool IsValid(TNod<T> n) => !n.GetParents().Contains(n.V, cmpFun);
		TNod<T> MakeNod(TNod<T> n) => Nod.Make(n.V, n.Children.Where(IsValid).SelectToArray(MakeNod));
		return MakeNod(root);
	}

	private static T[] GetParents<T>(this TNod<T> root)
	{
		var list = new List<T>();
		var node = root.Parent;
		while (node != null)
		{
			list.Add(node.V);
			node = node.Parent;
		}
		return list.ToArray();
	}*/
}