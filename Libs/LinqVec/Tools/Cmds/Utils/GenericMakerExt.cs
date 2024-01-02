using Geom;
using LinqVec.Tools.Cmds.Structs;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Utils;


public sealed record HotspotCmdsNfo<TH>(
	HotspotNfo<TH> Hotspot,
	Func<TH, IHotspotCmd[]> ActFuns
);


public sealed record HotspotNfo<TH>(
	string Name,
	Func<Pt, Option<TH>> Fun
)
{
	public Cursor? Cursor { get; init; }
	public Func<IRoVar<Pt>, Action<bool>> HoverAction { get; init; } = _ => _ => { };
}



public static class HotspotExt
{
	public static HotspotNfo<TH> WithCursor<TH>(this HotspotNfo<TH> hotspot, Cursor cursor) => hotspot with { Cursor = cursor };

	public static HotspotCmdsNfo Do<TH>(
		this HotspotNfo<TH> hotspot,
		Func<TH, IHotspotCmd[]> actFuns
	) =>
		new HotspotCmdsNfo<TH>(
			hotspot,
			actFuns
		).ToNonGeneric();

	private static HotspotCmdsNfo ToNonGeneric<TH>(this HotspotCmdsNfo<TH> set) => new(
		set.Hotspot.ToNonGeneric(),
		o => set.ActFuns((TH)o)
	);

	private static HotspotNfo ToNonGeneric<TH>(this HotspotNfo<TH> hotspot) => new(
		hotspot.Name,
		p => hotspot.Fun(p).Map(e => (object)e!),
		hotspot.Cursor,
		hotspot.HoverAction
	);
}