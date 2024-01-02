using LogLib.Structs;

namespace LogLib.Interfaces;

public interface ITxtWriter
{
	int LastSegLength { get; }
	ITxtWriter Write(TxtSegment seg);
	ITxtWriter WriteLine();
}


