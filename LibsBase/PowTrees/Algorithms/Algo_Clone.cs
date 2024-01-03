namespace PowTrees.Algorithms;

public static class Algo_Clone
{
	public static TNod<T> Clone<T>(this TNod<T> root) => Nod.Make(root.V, root.Children.Select(Clone));
}