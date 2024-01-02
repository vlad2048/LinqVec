using System.Reactive;
using System.Text;
using Microsoft.Reactive.Testing;
using PowBasics.CollectionsExt;

namespace TestLib;

public static class TestableObserverExt
{
	public static void AssertValues<T>(this ITestableObserver<T> obs, T[] exps)
	{
		var acts = obs.Messages.Where(e => e.Value.Kind == NotificationKind.OnNext).SelectToArray(e => e.Value.Value);
		AssertEq(acts, exps, e => $"{e}");
	}

	public static void AssertNotifications<T>(this ITestableObserver<T> obs, Recorded<Notification<T>>[] exps)
	{
		var acts = obs.Messages.ToArray();
		AssertEq(acts, exps, Fmt);
	}


	private static void AssertEq<T>(IEnumerable<T> actsEnum, IEnumerable<T> expsEnum, Func<T, string> fmt)
	{
		var acts = actsEnum.SelectToArray(fmt);
		var exps = expsEnum.SelectToArray(fmt);
		var pad = acts.Length == 0 ? 0 : acts.Max(e => e.Length);
		var padExp = exps.Length == 0 ? 0 : exps.Max(e => e.Length);
		var hasFlaggedDiff = false;

		L("  ", "Actual".PadRight(pad), " │ ", "Expected");
		string dup(char c, int n) => new(c, n);
		L(dup('═', 2 + pad), "═╪═", dup('═', padExp));

		for (var i = 0; i < Math.Max(acts.Length, exps.Length); i++)
		{
			var showDiff = false;
			if (!hasFlaggedDiff && IsDiff(acts, exps, i))
			{
				hasFlaggedDiff = true;
				showDiff = true;
			}

			var act = i < acts.Length ? acts[i] : "_";
			var exp = i < exps.Length ? exps[i] : "_";

			L(showDiff ? "->" : "  ", act.PadRight(pad), " │ ", exp);
		}
	}

	private static void L(params string[] s) => Console.WriteLine(s.JoinText(""));

	private static bool IsDiff(string[] acts, string[] exps, int idx) =>
		(idx >= acts.Length || idx >= exps.Length) switch {
			true => true,
			false => acts[idx] != exps[idx]
		};

	private static string Fmt<T>(this Recorded<Notification<T>> e)
	{
		var tStr = $"{TimeSpan.FromTicks(e.Time).TotalSeconds:F1}s";
		var notStr = e.Value.Kind switch
		{
			NotificationKind.OnNext => $"{e.Value.Value}",
			NotificationKind.OnError => $"OnError([{e.Value.Exception!.GetType().Name}] {e.Value.Exception.Message})",
			NotificationKind.OnCompleted => "OnCompleted()",
			_ => throw new ArgumentException()
		};
		return $"[{tStr}] {notStr}";
	}

}