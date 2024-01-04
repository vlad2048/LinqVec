using LogLib.Structs;
using LogLib.Utils;
using LogLib.Writers;
using PowBasics.StringsExt;

namespace LogLib;

public sealed record SlotLoc(int Pos, int Size);

public static class ConsoleRenderExt
{
	public static void RenderToConsole(this IEnumerable<IChunk> chunks) => chunks.Render();

	public static void RenderToSlot(this IEnumerable<IChunk> chunks, SlotLoc slot)
	{
		var width = Console.WindowWidth;
		if (slot.Pos >= width) return;
		var maxLng = Math.Min(slot.Size, width - slot.Pos);
		chunks = chunks.Truncate(maxLng);
		Console.CursorLeft = slot.Pos;
		chunks.Render();
	}

	private static void Render(this IEnumerable<IChunk> chunks)
	{
		foreach (var chunk in chunks)
		{
			switch (chunk)
			{
				case TextChunk { Text: var text, Fore: var fore, Back: var back }:
					if (fore != null) ConUtils.SetFore(MkCol(fore.Color));
					if (back != null) ConUtils.SetBack(MkCol(back.Color));
					Console.Write(text);
					ConUtils.SetFore(MkCol(B.gen_colNeutral.Color));
					ConUtils.SetBack(MkCol(B.gen_colBlack.Color));
					break;
				case NewlineChunk:
					Console.WriteLine();
					break;
				default:
					throw new ArgumentException();
			}
		}
	}


	private static IEnumerable<IChunk> Truncate(this IEnumerable<IChunk> chunksSource, int n)
	{
		var chunks = chunksSource.ToArray();
		var lng = chunks.SumOrZero(e => e.Length);
		if (lng <= n) return chunks;
		var chunksNext = new List<IChunk>();
		foreach (var chunk in chunks)
		{
			if (!chunk.NeedsTruncate(n))
			{
				chunksNext.Add(chunk);
			}
			else
			{
				chunksNext.Add(chunk.Truncate(n));
				break;
			}
			n -= chunk.Length;
		}
		return chunksNext;
	}

	private static bool NeedsTruncate(this IChunk chunk, int n) => chunk.Length > n;

	private static IChunk Truncate(this IChunk chunk, int n) => chunk switch
	{
		TextChunk { Text: var text } e => e with { Text = text.Truncate(n) },
		_ => chunk
	};
}