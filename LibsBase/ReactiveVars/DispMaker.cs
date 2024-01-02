using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using PowBasics.CollectionsExt;

namespace ReactiveVars;

public static class DispMaker
{
	public static bool DispMakerLoggingEnabled { get; set; }
	private const int IndentSize = 2;

	private const int ColName = 0x8537b0;
	private const int ColNew = 0x3fe861;
	private const int ColDispose = 0xeb3f76;

	private static int indentLevel;
	private static readonly ConcurrentDictionary<Disp, DispNfo> map = new();
	private static readonly Dictionary<string, int> countMap = new();

	internal static void Reset()
	{
		map.Clear();
		countMap.Clear();
		indentLevel = 0;
	}


	public static Disp MkD(string nameBase, [CallerFilePath] string srcFile = "", [CallerLineNumber] int srcLine = 0)
	{
		var name = GetFullName(nameBase);
		Print(name, true, indentLevel++);
		var d = new Disp();
		map[d] = DispNfo.Make(d, srcFile, srcLine);
		Disposable.Create(() =>
		{
			Print(name, false, --indentLevel);
			map[d] = map[d].FlagDispose();
		}).D(d);
		return d;
	}

	private static void Print(string name, bool isNew, int indent)
	{
		if (!DispMakerLoggingEnabled) return;
		var indentStr = new string(' ', indent * IndentSize);
		L.Write(indentStr);
		L.Write(name, ColName);
		var (txt, col) = isNew ? (".new()", ColNew) : (".Dispose()", ColDispose);
		L.WriteLine(txt, col);
	}





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
			LStr("");
			LTitle("All Disps released");
			return false;
		}
		else
		{
			var topDisps = allDisps.RemoveSubs();
			LTitle($"{topDisps.Length} unreleased top level Disps (total: {allDisps.Length})");
			foreach (var d in topDisps)
				LStr($"  {d}");
			LStr("");
			return true;
		}
	}



	private sealed record DispNfo(Disp DISP, string File, int Line, bool Disposed)
	{
		public static DispNfo Make(Disp disp, string file, int line) => new(disp, file, line, false);
		public override string ToString() => $"{File}:{Line}";
		public DispNfo FlagDispose() => Disposed switch
		{
			true => throw new ArgumentException("Already disposed"),
			false => this with { Disposed = true }
		};
	}



	private static void LStr(string s) => Console.WriteLine(s);
	private static void LTitle(string s)
	{
		LStr(s);
		LStr(new string('=', s.Length));
	}

	private static DispNfo[] RemoveSubs(this DispNfo[] ds) =>
		ds
			.WhereToArray(d => ds.Where(e => e != d).All(e => !e.DISP.Contains(d.DISP)));


	private static string GetFullName(string name)
	{
		if (!countMap.TryAdd(name, 0))
			countMap[name]++;
		var cnt = countMap[name];
		return cnt switch {
			0 => name,
			_ => $"{name}[{cnt}]"
		};
	}
}