using LogLib.Structs;
using LogLib.Utils;

namespace LogLib.Writers;

public sealed class MemoryTxtWriter : ITxtWriter
{
	private readonly List<IChunk> chunks = [];

	public IChunk[] Chunks => chunks.ToArray();
	public ITxtWriter Write(IChunk chunk) => this.Ret(() => chunks.Add(chunk));
	public ITxtWriter WriteBefore(IChunk chunk) => this.Ret(() => chunks.Insert(0, chunk));
}
