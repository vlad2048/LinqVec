using LogLib.Structs;

namespace Storybook.Structs;

enum ColType
{
	Fore,
	Back,
}


sealed record ColorClickedEvt(ColType Type, NamedColor NamedColor);
