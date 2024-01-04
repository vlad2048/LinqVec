using LogLib.Structs;
using LogLib.Utils;
using LogLib.Writers;
using PowBasics.CollectionsExt;
using PowBasics.StringsExt;

namespace LogLib.Writers;

public sealed class MemoryTxtWriter : ITxtWriter
{
	private readonly List<IChunk> chunks = [];

	public IChunk[] Chunks => chunks.ToArray();
	public ITxtWriter Write(IChunk chunk) => this.Ret(() => chunks.Add(chunk));
	public ITxtWriter WriteBefore(IChunk chunk) => this.Ret(() => chunks.Insert(0, chunk));

	public ITxtWriter SetDefaultFore(Col fore)
	{
		var chunksNext = chunks.SelectToList(e => e.SetDefaultFore(fore));
		chunks.Clear();
		chunks.AddRange(chunksNext);
		return this;
	}
}


file static class ChunkExt
{
	public static IChunk SetDefaultFore(this IChunk chunk, Col fore) => chunk switch {
		TextChunk { Fore: null } e => e with { Fore = fore },
		_ => chunk
	};
}