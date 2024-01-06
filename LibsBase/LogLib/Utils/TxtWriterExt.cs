using LogLib.Structs;
using LogLib.Writers;
using W = LogLib.Writers.ITxtWriter;

namespace LogLib.Utils;

public static class TxtWriterExt
{
	// Basics
	// ======
	public static W Write(this W w, string text, Option<NamedColor> fore, Option<NamedColor> back) => w.Write(new TextChunk(text, fore, back));
	public static W Write(this W w, string text, Option<NamedColor> fore) => w.Write(text, fore, None);
	public static W Write(this W w, string text) => w.Write(text, None, None);

	public static W WriteLine(this W w, string text, Option<NamedColor> fore, Option<NamedColor> back) => w.Write(text, fore, back).WriteLine();
	public static W WriteLine(this W w, string text, Option<NamedColor> fore) => w.Write(text, fore).WriteLine();
	public static W WriteLine(this W w, string text) => w.Write(text).WriteLine();
	public static W WriteLine(this W w) => w.Write(new NewlineChunk());

	public static W Write(this W w, IEnumerable<IChunk> source)
	{
		foreach (var elt in source)
			w.Write(elt);
		return w;
	}
	public static W WriteLine(this W w, IEnumerable<IChunk> source) => w.Write(source).WriteLine();


	// Spacing
	// =======
	public static W spc(this W w, int n) => w.Write(new string(' ', Math.Max(0, n)));

	// Branching
	// =========
	public static W Write(this W w, Func<W, W> action) => action(w);

	// Merge
	// =====
	public static W Write(this W w, W other)
	{
		foreach (var chunk in other.Chunks)
			w.Write(chunk);
		return w;
	}
	public static W WriteLine(this W w, W other)
	{
		w.Write(other);
		return w.WriteLine();
	}


	// Blocks
	// ======
	public static W Blk(this W w, Action<W> blk)
	{
		var mem = new MemoryTxtWriter();
		blk(mem);
		return w.Write(mem);
	}

	// Actions on the whole (to use with Blocks)
	// =========================================
	public static W PadLeft(this W w, int n)
	{
		var lng = w.Chunks.SumOrZero(e => e.Length);
		if (n <= lng) return w;
		return w.WriteBefore(new TextChunk(new string(' ', n - lng), None, None));
	}
	public static W PadRight(this W w, int n)
	{
		var lng = w.Chunks.SumOrZero(e => e.Length);
		if (n <= lng) return w;
		w.Write(new string(' ', n - lng));
		return w;
	}
	public static W Surround(this W w, IChunk chunkLeft, IChunk chunkRight)
	{
		w.WriteBefore(chunkLeft);
		w.Write(chunkRight);
		return w;
	}
	public static W Surround(this W w, string textLeft, string textRight, NamedColor fore) => w.Surround(new TextChunk(textLeft, fore, None), new TextChunk(textRight, fore, None));
	public static W Surround(this W w, char charLeft, char charRight, NamedColor fore) => w.Surround(new TextChunk(charLeft.ToString(), fore, None), new TextChunk(charRight.ToString(), fore, None));


	// Utils
	// =====
	internal static T Ret<T>(this T w, Action action)
	{
		action();
		return w;
	}
}