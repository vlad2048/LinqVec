using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using PowBasics.CollectionsExt;

namespace PowRxVar;

public static class DispMaker
{
	private sealed record DispNfo(string File, int Line, bool Disposed)
	{
		public static DispNfo Make(string file, int line) => new(file, line, false);

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
		map[d] = DispNfo.Make(srcFile, srcLine);
		Disposable.Create(() => map[d] = map[d].FlagDispose()).D(d);
		return d;
	}

	public static void CheckForUndisposedDisps()
	{
		var ds = map.Values.WhereToArray(e => !e.Disposed);
		if (ds.Length == 0)
		{
			LTitle("All Disps released");
		}
		else
		{
			LTitle($"{ds.Length} unreleased Disps");
			foreach (var d in ds)
			{
				L($"  {d}");
			}
			L("");
			Console.ReadKey();
		}
	}

	private static void L(string s) => Console.WriteLine(s);
	private static void LTitle(string s)
	{
		L(s);
		L(new string('=', s.Length));
	}
}