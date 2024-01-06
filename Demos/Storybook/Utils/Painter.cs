using LogLib.Structs;
using Storybook.Logic;

namespace Storybook.Utils;

record ChunkR(Rectangle R, TextChunk Chunk);

static class Painter
{
	// char: 9 x 10
	private static readonly Font Font = new("Consolas", 12); //, FontStyle.Bold);
	private const int CharWidth = 9;
	private const int CharHeight = 20;
	private static readonly Color defaultFore = Color.FromArgb(0x808080);
	private static readonly Color defaultBack = Color.FromArgb(0x000000);

	public static ChunkR[] GetChunkMap(Txt chunks)
	{
		var list = new List<ChunkR>();
		var x = 0;
		var y = 0;
		foreach (var chunk in chunks)
		{
			switch (chunk)
			{
				case TextChunk { Text: var text } c:
					list.Add(new ChunkR(
						new Rectangle(
							x,
							y,
							text.Length * CharWidth,
							CharHeight
						),
						c
					));
					x += text.Length * CharWidth;
					break;
				case NewlineChunk:
					y += CharHeight;
					x = 0;
					break;
				default:
					throw new ArgumentException();
			}
		}
		return list.ToArray();
	}

	public static void PaintChunks(Graphics gfx, Txt chunks, PaletteKeeper paletteKeeper)
	{
		var x = 0;
		var y = 0;
		foreach (var chunk in chunks)
		{
			switch (chunk)
			{
				case TextChunk { Text: var text, Fore: var fore, Back: var back }:
					TextRenderer.DrawText(
						gfx,
						text,
						Font,
						new Point(x, y),
						fore.Map(e => paletteKeeper.GetColorForDisplay(e.Name)).IfNone(defaultFore),
						back.Map(e => paletteKeeper.GetColorForDisplay(e.Name)).IfNone(defaultBack),
						TextFormatFlags.Default
					);
					x += text.Length * CharWidth;
					break;
				case NewlineChunk:
					y += CharHeight;
					x = 0;
					break;
				default:
					throw new ArgumentException();
			}
		}
	}
}