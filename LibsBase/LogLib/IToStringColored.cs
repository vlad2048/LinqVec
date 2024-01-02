using LogLib.Interfaces;
using LogLib.Structs;

namespace LogLib;

public interface IWrite
{
	ITxtWriter Write(ITxtWriter w);
}