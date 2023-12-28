using System.Reactive.Linq;
using LinqVec.Utils.Rx;

namespace LinqVec.Utils;

public static class OptionExt
{
	public static T? ToNullable<T>(this Option<T> opt) where T : class => opt.MatchUnsafe(e => e, () => null);

	public static T Ensure<T>(this Option<T> opt) => opt.IfNone(() => throw new ArgumentException());

	public static Option<U> OfType<T, U>(this Option<T> opt) where U : T =>
		from v in opt
		where v is U
		select (U)v;

	public static Option<T> FirstOrOption<T>(this IEnumerable<T> src, Func<T, bool>? predicate = null)
	{
		predicate ??= _ => true;
		foreach (var elt in src)
		{
			if (predicate(elt))
				return Some(elt);
		}
		return None;
	}

	public static Option<T> AggregateArr<T>(params Option<T>[] arr) => arr.Aggregate();

	public static Option<T> Aggregate<T>(this IEnumerable<Option<T>> arr)
	{
		foreach (var elt in arr)
			if (elt.IsSome)
				return elt;
		return None;
	}

	public static IObservable<Unit> WhereNone<T>(this IObservable<Option<T>> src) =>
		src
			.Where(e => e.IsNone)
			.ToUnit();

	public static IObservable<T> WhereSome<T>(this IObservable<Option<T>> src) =>
		src
			.Where(e => e.IsSome)
			.Select(e => e.Ensure());
}