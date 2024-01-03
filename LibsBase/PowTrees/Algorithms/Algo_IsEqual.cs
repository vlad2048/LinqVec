namespace PowTrees.Algorithms;

public static class Algo_IsEqual
{
	public static bool IsEqual<T>(this TNod<T> rootA, TNod<T> rootB)
	{
		bool Recurse(TNod<T> nodeA, TNod<T> nodeB)
		{
			if (!Equals(nodeA.V, nodeB.V) || nodeA.Children.Count != nodeB.Children.Count)
				return false;
			foreach (var t in nodeA.Children.Zip(nodeB.Children))
			{
				if (!Recurse(t.First, t.Second))
					return false;
			}
			return true;
		}

		return Recurse(rootA, rootB);
	}
}