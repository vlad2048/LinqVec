using Geom;

namespace LinqVec.Tools.Acts.Structs;

public sealed record Hotspot(
	Func<Pt, Option<object>> Fun,
	Cursor? Cursor
);


public sealed record Hotspot<H>(
	Func<Pt, Option<H>> Fun,
	Cursor? Cursor
);

public static class HotspotExt
{
	public static Hotspot<H> WithCursor<H>(this Hotspot<H> hotspot, Cursor cursor) => hotspot with { Cursor = cursor };

	public static Hotspot<Pt> ToPt<H>(this Hotspot<H> hotspot) => new(
		p => hotspot.Fun(p).Map(_ => Pt.Zero),
		hotspot.Cursor
	);

	internal static Hotspot ToNonGeneric<H>(this Hotspot<H> hotspot) => new(
		pt => hotspot.Fun(pt).Map(e => (object)e!),
		hotspot.Cursor
	);
}