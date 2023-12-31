namespace LinqVec.Utils;

public static class ArrExt
{
	public static Arr<T> Toggle<T>(this Arr<T> arr, T value) => arr.Contains(value) switch {
		false => arr.Add(value),
		true => arr.Remove(value)
	};
}