using System.Drawing;
using LogLib.Structs;
using LogLib.Utils;
using PowBasics.StringsExt;

namespace LogLib;

public sealed record SlotLoc(int Pos, int Size);

public static class ConsoleRenderExt
{
	public static IChunk[] ClipToConsole(this IChunk[] chunks, SlotLoc slot)
	{
		var width = Console.WindowWidth;
		if (slot.Pos >= width) return [];
		var maxLng = Math.Min(slot.Size, width - slot.Pos);
		return chunks.Truncate(maxLng).ToArray();
	}

	public static void RenderToConsole(this IEnumerable<IChunk> chunks) => chunks.Render();

	public static void RenderToConsoleSlot(this IEnumerable<IChunk> chunks, SlotLoc slot)
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
					fore.IfSome(e => ConUtils.SetFore(e.Color));
					back.IfSome(e => ConUtils.SetBack(e.Color));
					Console.Write(text);
					ConUtils.SetFore(Color.FromArgb(0x808080));
					ConUtils.SetBack(Color.FromArgb(0x000000));
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
			if (chunk.Length <= n)
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

	private static IChunk Truncate(this IChunk chunk, int n) => chunk switch
	{
		TextChunk { Text: var text } e => e with { Text = text.Truncate(n) },
		_ => chunk
	};
}