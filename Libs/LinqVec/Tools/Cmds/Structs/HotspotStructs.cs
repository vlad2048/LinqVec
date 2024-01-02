using Geom;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Structs;


public sealed record HotspotCmdsNfo(
	HotspotNfo Hotspot,
	Func<object, IHotspotCmd[]> Cmds
)
{
	public static readonly HotspotCmdsNfo Empty = new(HotspotNfo.Empty, _ => []);
}


public sealed record HotspotNfo(
	string Name,
	Func<Pt, Option<object>> Fun,
	Cursor? Cursor,
	Func<IRoVar<Pt>, Action<bool>> HoverAction
)
{
	public static readonly Func<IRoVar<Pt>, Action<bool>> EmptyAction = _ => _ => { };
	public static readonly HotspotNfo Empty = new("Empty", _ => None, null, EmptyAction);
}