using Geom;
using LinqVec.Tools.Acts.Enums;
using H = System.Object;

namespace LinqVec.Tools.Acts;


// **********
// * Public *
// **********
public delegate ActSet ActSetMaker(Disp actD);

public sealed record ActSet(
    string Name,
    Cursor Cursor,
    params HotspotActs[] HotspotActSets
)
{
    internal static readonly ActSet Empty = new("Empty", Cursors.Default);
}

public sealed record HotspotActs(
	Hotspot Hotspot,
	Func<H, HotspotAct[]> ActFuns
)
{
	public static readonly HotspotActs Empty = new(Hotspot.Empty, _ => []);
}

public sealed record Hotspot(
    string Name,
    Func<Pt, Option<H>> Fun,
    Cursor? Cursor
)
{
    public static readonly Hotspot Empty = new("Empty", _ => None, null);
}

public sealed record HotspotAct(
    string Name,
    Gesture Gesture,
    HotspotActActions Actions
);

public sealed record HotspotActActions(
	Action<Pt> DragStart,
	Func<Pt, Option<ActSetMaker>> Confirm
)
{
	public static readonly HotspotActActions Empty = new(
        _ => {},
        _ => None
	);
}


// ************
// * Internal *
// ************
sealed record HotspotActsRun(
	Hotspot Hotspot,
    H HotspotValue,
	HotspotAct[] Acts
)
{
	public static readonly HotspotActsRun Empty = new(Hotspot.Empty, null!, []);
}

/*sealed class HotspotActsRun(
    Hotspot hotspot,
    H hotspotValue,
    Func<H, HotspotAct[]> actFuns
)
{
    public Hotspot Hotspot => hotspot;
    public HotspotAct[] Acts => actFuns(hotspotValue);
}*/


// ***********
// * Generic *
// ***********
public sealed record Hotspot<TH>(
    string Name,
    Func<Pt, Option<TH>> Fun,
    Cursor? Cursor
);

public sealed record HotspotActs<TH>(
    Hotspot<TH> Hotspot,
    Func<TH, HotspotAct[]> ActFuns
);



public static class HotspotExt
{
    public static Hotspot<TH> WithCursor<TH>(this Hotspot<TH> hotspot, Cursor cursor) => hotspot with { Cursor = cursor };

    public static HotspotActs ToNonGeneric<TH>(this HotspotActs<TH> set) => new(
        set.Hotspot.ToNonGeneric(),
        o => set.ActFuns((TH)o)
    );

    private static Hotspot ToNonGeneric<TH>(this Hotspot<TH> hotspot) => new(
        hotspot.Name,
        p => hotspot.Fun(p).Map(e => (H)e!),
        hotspot.Cursor
    );
}
