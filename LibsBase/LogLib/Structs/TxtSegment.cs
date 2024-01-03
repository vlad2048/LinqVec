using System.Runtime.CompilerServices;
using LogLib.Interfaces;

namespace LogLib.Structs;


public sealed record TxtSegment
{
	public TxtSegment(
		string Text,
		int Color,
		[CallerArgumentExpression(nameof(Color))] string? ColorName = null
	)
	{
		this.Text = Text;
		this.Color = Color;
		this.ColorName = ColorName;
	}
	public string Text { get; init; }
	public int Color { get; init; }
	public string? ColorName { get; init; }
	public void Deconstruct(out string Text, out int Color, out string? ColorName)
	{
		Text = this.Text;
		Color = this.Color;
		ColorName = this.ColorName;
	}
}

static class TxtSegmentExt
{
    public static ITxtWriter Run(this TxtSegment[][] segs, ITxtWriter writer, Func<ITxtWriter, ITxtWriter>? linePrefixFun = null)
    {
	    linePrefixFun ??= e => e;

		foreach (var (line, isNotLast) in segs.Select((line, idx) => (line, idx < segs.Length - 1)))
        {
			writer = linePrefixFun(writer);
			foreach (var seg in line)
	            writer.Write(seg);
            if (isNotLast)
                writer.WriteLine();
        }
        return writer;
    }
    public static ITxtWriter RunLine(this TxtSegment[][] segs, ITxtWriter writer, Func<ITxtWriter, ITxtWriter> linePrefixFun)
    {
	    Run(segs, writer, linePrefixFun);
	    return writer.WriteLine();
	    /*writer = linePrefixFun(writer);
		foreach (var line in segs)
		{
			foreach (var seg in line)
				writer.Write(seg);
			writer.WriteLine();
		}
		return writer;*/
    }
}
