using LogLib.Structs;
using LogLib.Writers;
using W = LogLib.Writers.ITxtWriter;

namespace LogLib.Utils;

public static class TxtWriterExt
{
	public static readonly Col base_colBlack = new(0x000000, nameof(base_colBlack));


	// Basics
	// ======
	public static W Write(this W w, string text, Col? fore = null, Col? back = null) => w.Write(new TextChunk(text, fore, back));
	public static W WriteLine(this W w) => w.Write(new NewlineChunk());
	public static W WriteLine(this W w, string text, Col? fore = null, Col? back = null) => w
		.Write(text, fore, back)
		.WriteLine();

	// Spacing
	// =======
	public static W spc(this W w, int n) => w.Write(new string(' ', Math.Max(0, n)), base_colBlack);

	// Branching
	// =========
	public static W Write(this W _, Func<W> action) => action();

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
		return w.WriteBefore(new TextChunk(new string(' ', n - lng), base_colBlack));
	}
	public static W PadRight(this W w, int n)
	{
		var lng = w.Chunks.SumOrZero(e => e.Length);
		if (n <= lng) return w;
		w.Write(new TextChunk(new string(' ', n - lng), base_colBlack));
		return w;
	}
	public static W Surround(this W w, IChunk chunkLeft, IChunk chunkRight)
	{
		w.WriteBefore(chunkLeft);
		w.Write(chunkRight);
		return w;
	}
	public static W Surround(this W w, string textLeft, string textRight, Col fore) => w.Surround(new TextChunk(textLeft, fore), new TextChunk(textRight, fore));
	public static W Surround(this W w, char charLeft, char charRight, Col fore) => w.Surround(new TextChunk(charLeft.ToString(), fore), new TextChunk(charRight.ToString(), fore));


	// Utils
	// =====
	internal static T Ret<T>(this T w, Action action)
	{
		action();
		return w;
	}
}