using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using PowBasics.CollectionsExt;

namespace ReactiveVars;

public static class DispMaker
{
	private sealed record DispNfo(Disp Disp, string File, int Line, bool Disposed)
	{
		public static DispNfo Make(Disp disp, string file, int line) => new(disp, file, line, false);

		public override string ToString() => $"{File}:{Line}";

		public DispNfo FlagDispose() => Disposed switch
		{
			true => throw new ArgumentException("Already disposed"),
			false => this with { Disposed = true }
		};
	}

	private static readonly ConcurrentDictionary<Disp, DispNfo> map = new();

	public static Disp MkD([CallerFilePath] string srcFile = "", [CallerLineNumber] int srcLine = 0)
	{
		var d = new Disp();
		map[d] = DispNfo.Make(d, srcFile, srcLine);
		Disposable.Create(() => map[d] = map[d].FlagDispose()).D(d);
		return d;
	}

	//public static void StopTrackingDisp(Disp d) => map.Remove(d, out _);

	public static void ResetDispTrackingForTestStart() => map.Clear();

	public static bool LogAndTellIfThereAreUndisposedDisps(bool waitForKey = true)
	{
		var isIssue = LogAndTellIfThereAreUndisposedDisps_Inner();
		if (isIssue && waitForKey)
			Console.ReadKey();
		return isIssue;
	}

	private static bool LogAndTellIfThereAreUndisposedDisps_Inner()
	{
		var allDisps = map.Values.WhereToArray(e => !e.Disposed);
		if (allDisps.Length == 0)
		{
			L("");
			LTitle("All Disps released");
			return false;
		}
		else
		{
			var topDisps = allDisps.RemoveSubs();
			LTitle($"{topDisps.Length} unreleased top level Disps (total: {allDisps.Length})");
			foreach (var d in topDisps)
				L($"  {d}");
			L("");
			return true;
		}
	}

	private static void L(string s) => Console.WriteLine(s);
	private static void LTitle(string s)
	{
		L(s);
		L(new string('=', s.Length));
	}

	private static DispNfo[] RemoveSubs(this DispNfo[] ds) =>
		ds
			.WhereToArray(d => ds.Where(e => e != d).All(e => !e.Disp.Contains(d.Disp)));
}