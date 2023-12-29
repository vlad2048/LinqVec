using System.Reactive.Disposables;
using Geom;
using LinqVec.Tools.Cmds.Enums;
using H = System.Object;

namespace LinqVec.Tools.Cmds;


// **********
// * Public *
// **********
public delegate ToolState ToolStateFun(Disp actD);


/*
public sealed record ToolState(
       string Name,
       Cursor Cursor,
       HotspotNfo[] Hotspots
   )
   {
       internal static readonly ToolState Empty = new("Empty", Cursors.Default, []);
   }
*/


public sealed class ToolState(
	string Name,
	Cursor Cursor,
	HotspotNfo[] Hotspots,
	ShortcutNfo[]? Shortcuts = null
)
{
	public string Name { get; } = Name;
	public Cursor Cursor { get; } = Cursor;
	public HotspotNfo[] Hotspots { get; } = Hotspots;
	public ShortcutNfo[] Shortcuts { get; } = Shortcuts ?? [];
	/*public void Deconstruct(out string Name, out Cursor Cursor, out HotspotNfo[] Hotspots, out ShortcutNfo[]? Shortcuts)
    {
	    Name = this.Name;
	    Cursor = this.Cursor;
	    Hotspots = this.Hotspots;
	    Shortcuts = this.Shortcuts;
    }*/
	internal static readonly ToolState Empty = new("Empty", Cursors.Default, []);
}

public sealed record HotspotNfo(
	Hotspot Hotspot,
	Func<H, IHotspotCmd[]> Cmds
)
{
	public static readonly HotspotNfo Empty = new(Hotspot.Empty, _ => []);
}

public sealed record ShortcutNfo(
	string Name,
	Keys Key,
	Action Action
);

public static class Kbd
{
	public static ShortcutNfo Make(
		string name,
		Keys key,
		Action action
	) => new(name, key, action);
}

public sealed record Hotspot(
	string Name,
	Func<Pt, Option<H>> Fun,
	Cursor? Cursor,
	Action HoverAction
)
{
	public static readonly Hotspot Empty = new("Empty", _ => None, null, () => { });
}

public interface IHotspotCmd
{
	string Name { get; }
	Gesture Gesture { get; }
}
public sealed record ClickHotspotCmd(
	string Name,
	Gesture Gesture,
	Func<Option<ToolStateFun>> Action
) : IHotspotCmd;
public sealed record DragHotspotCmd(
	string Name,
	Gesture Gesture,
	Func<Pt, Action> Action
) : IHotspotCmd;



// ************
// * Internal *
// ************
sealed record HotspotNfoResolved(
	Hotspot Hotspot,
	H HotspotValue,
	IHotspotCmd[] Cmds,
	bool RepeatFlag
)
{
	public static readonly HotspotNfoResolved Empty = new(Hotspot.Empty, null!, [], false);
}


// ***********
// * Generic *
// ***********
public sealed record Hotspot<TH>(
	string Name,
	Func<Pt, Option<TH>> Fun
)
{
	public Cursor? Cursor { get; init; }
	public Action HoverAction { get; init; } = () => { };
}

public sealed record HotspotActs<TH>(
	Hotspot<TH> Hotspot,
	Func<TH, IHotspotCmd[]> ActFuns
);



public static class HotspotExt
{
	public static HotspotNfo Do<TH>(
		this Hotspot<TH> hotspot,
		Func<TH, IHotspotCmd[]> actFuns
	) => new HotspotActs<TH>(
		hotspot,
		actFuns
	).ToNonGeneric();

	public static Hotspot<TH> WithCursor<TH>(this Hotspot<TH> hotspot, Cursor cursor) => hotspot with { Cursor = cursor };
	public static Hotspot<TH> OnHover<TH>(this Hotspot<TH> hotspot, Action hoverAction) => hotspot with { HoverAction = hoverAction };

	private static HotspotNfo ToNonGeneric<TH>(this HotspotActs<TH> set) => new(
		set.Hotspot.ToNonGeneric(),
		o => set.ActFuns((TH)o)
	);

	private static Hotspot ToNonGeneric<TH>(this Hotspot<TH> hotspot) => new(
		hotspot.Name,
		p => hotspot.Fun(p).Map(e => (H)e!),
		hotspot.Cursor,
		hotspot.HoverAction
	);
}
