using LinqVec.Tools.Acts.Enums;

namespace LinqVec.Tools.Acts.Structs;

public sealed record ActNfo(
	string Id,
	Gesture Gesture,
	Hotspot Hotspot,
	ActActions Actions
	//GfxStatesTuple GfxStates
);