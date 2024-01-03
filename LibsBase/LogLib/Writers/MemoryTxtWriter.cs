using LogLib.Interfaces;
using LogLib.Structs;

namespace LogLib.Writers;

sealed class MemoryTxtWriter : ITxtWriter
{
	private readonly List<TxtSegment[]> lines = [];
	private readonly List<TxtSegment> curLine = [];
	internal void Clear()
	{
		lines.Clear();
		curLine.Clear();
	}

	public Txt Txt => new([.. lines, [.. curLine]]);

	public int LastSegLength =>
		curLine.Any() switch
		{
			true => curLine.Last().Text.Length,
			false => lines.Any() switch
			{
				true => lines.Select(l => l.Any() switch
				{
					true => l.Last().Text.Length,
					false => 0
				}).Last(),
				false => 0
			}
		};
	public int AbsoluteX { get; private set; }

	public ITxtWriter Write(TxtSegment seg)
	{
		curLine.Add(seg);
		AbsoluteX += seg.Text.Length;
		return this;
	}
	public ITxtWriter WriteLine()
	{
		lines.Add([.. curLine]);
		curLine.Clear();
		AbsoluteX = 0;
		return this;
	}
}
