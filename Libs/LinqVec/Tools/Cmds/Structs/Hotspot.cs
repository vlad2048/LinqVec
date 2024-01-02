namespace LinqVec.Tools.Cmds.Structs;

sealed record Hotspot(
	HotspotNfo HotspotNfo,
	object HotspotValue,
	IHotspotCmd[] Cmds,
	bool RepeatFlag
)
{
	public override string ToString() => $"{HotspotNfo.Name} (value:{HotspotValue})";
	public static readonly Hotspot Empty = new(HotspotNfo.Empty, null!, [], false);
}