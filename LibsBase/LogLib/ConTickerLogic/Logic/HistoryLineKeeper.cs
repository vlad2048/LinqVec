using LogLib.Structs;
using LogLib.Utils;

namespace LogLib.ConTickerLogic.Logic;

sealed record ChunkPos(int Pos, TextChunk[] Chunks)
{
	public int Size => Chunks.SumOrZero(e => e.Text.Length);
}

sealed class LogTickerHistory
{
	private readonly List<IChunk> history = new();
	private readonly List<ChunkPos> curLine = new();
	private readonly Action<IChunk[], string> saveAction;

	public LogTickerHistory(Action<IChunk[], string> saveAction)
	{
		this.saveAction = saveAction;
	}

	public void NewTick()
	{
		var tidyLine = HistoryLineKeeper.Tidy(curLine);
		history.AddRange(tidyLine);
		curLine.Clear();
	}

	public void RenderFragment(Txt txt, SlotLoc loc) => curLine.Add(new ChunkPos(loc.Pos, txt.OfType<TextChunk>().ToArray()));

	public void Save(string file) => saveAction(history.ToArray(), file);
}

static class HistoryLineKeeper
{
	public static IChunk[] Tidy(List<ChunkPos> chunks)
	{
		chunks = chunks.OrderBy(e => e.Pos).ToList();
		var list = new List<IChunk>();
		var x = 0;

		void Add(params TextChunk[] cs)
		{
			list.AddRange(cs);
			x += cs.SumOrZero(e => e.Text.Length);

		}

		foreach (var chunk in chunks)
		{
			if (chunk.Pos > x)
				Add(new TextChunk(new string(' ', chunk.Pos - x), None, None));
			Add(chunk.Chunks);
		}

		list.Add(new NewlineChunk());

		return list.ToArray();
	}
}