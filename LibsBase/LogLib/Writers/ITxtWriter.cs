using LogLib.Structs;

namespace LogLib.Writers;

public interface ITxtWriter
{
    IChunk[] Chunks { get; }
    ITxtWriter Write(IChunk chunk);
    ITxtWriter WriteBefore(IChunk chunk);
}