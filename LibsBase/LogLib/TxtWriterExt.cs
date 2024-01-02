using System.Drawing;
using System.Runtime.CompilerServices;
using LanguageExt;
using LogLib.Interfaces;
using LogLib.Structs;

namespace LogLib;

public static class LogLibColors
{
	public const int Black = 0x000000;
	public const int Gray = 0x207341;
	public const int Time = 0x207341;
	public const int On = 0x207341;
	public const int Off = 0x207341;
}

public static class TxtWriterExt
{
	// WriteLine
    // =========
	public static ITxtWriter WriteLine(this ITxtWriter w, TxtSegment seg) => w
		.Write(seg)
		.WriteLine();

    // Option<>
    // ========
    public static ITxtWriter WriteMatch<T>(this ITxtWriter w, Option<T> opt, Func<T, ITxtWriter> someFun, Func<ITxtWriter> noneFun) => opt.Match(someFun, noneFun);

	// Specialized
	// ===========
	public static ITxtWriter WriteTime(this ITxtWriter w, DateTimeOffset t) => w
		.Write($"[{t:HH:mm:ss.fffffff}]", LogLibColors.Time)
		.Space(1);

	public static ITxtWriter WriteFlag(this ITxtWriter w, string? name, bool val) => w
        .WriteIf(name != null, $"[{name}:", LogLibColors.Gray)
        .Write(val ? new TxtSegment("on ", LogLibColors.On) : new TxtSegment("off", LogLibColors.Off))
        .Write("]", LogLibColors.Gray)
        .Space(1);

	// Misc
	// ====
    public static ITxtWriter Write(this ITxtWriter w, Func<ITxtWriter> action) { action(); return w; }
    //public static ITxtWriter Write(this ITxtWriter w, (string, int) textCol) => w.Write(new TxtSegment(textCol.Item1, textCol.Item2));
    public static ITxtWriter Write(this ITxtWriter w, string text, int col, [CallerArgumentExpression(nameof(col))] string? colStr = null) => w.Write(new TxtSegment(text, col, colStr));
    public static ITxtWriter WriteLine(this ITxtWriter w, string text, int col, [CallerArgumentExpression(nameof(col))] string? colStr = null) => w.WriteLine(new TxtSegment(text, col, colStr));
    public static ITxtWriter Write(this ITxtWriter w, string text, Color col, [CallerArgumentExpression(nameof(col))] string? colStr = null) => w.Write(new TxtSegment(text, UnMkCol(col), colStr));
    public static ITxtWriter WriteLine(this ITxtWriter w, string text, Color col, [CallerArgumentExpression(nameof(col))] string? colStr = null) => w.WriteLine(new TxtSegment(text, UnMkCol(col), colStr));

    // Utils
    // =====
    public static ITxtWriter Pad(this ITxtWriter w, int n) => w.Space(n - w.LastSegLength);
    public static ITxtWriter Space(this ITxtWriter w, int cnt) => cnt switch
    {
        > 0 => w.Write(new string(' ', cnt), LogLibColors.Black),
        _ => w
    };
    public static ITxtWriter WriteIf(this ITxtWriter w, bool predicate, string text, int col, [CallerArgumentExpression(nameof(col))] string? colStr = null) =>
        predicate switch
        {
            true => w.Write(text, col, colStr),
            false => w
        };
    public static ITxtWriter WriteIf(this ITxtWriter w, bool predicate, Func<ITxtWriter, ITxtWriter> action) =>
	    predicate switch
	    {
		    true => action(w),
		    false => w
	    };

	// Compose Txts
	// ============
	public static ITxtWriter IfType<T, U>(this ITxtWriter w, T e, Action<U> action) where U : T
    {
        if (e is U u) action(u);
        return w;
    }
    public static ITxtWriter If(this ITxtWriter w, bool predicate, Action action)
    {
        if (predicate) action();
        return w;
    }

    public static ITxtWriter Write(this ITxtWriter w, Txt txt) => txt.Segments.Run(w);

    public static ITxtWriter WriteLine(this ITxtWriter w, Txt txt)
    {
        w.Write(txt);
        return w.WriteLine();
    }





    private static int UnMkCol(Color v) => (v.A << 24) + (v.R << 16) + (v.G << 8) + v.B;
}