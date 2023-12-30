﻿using System.Reactive.Disposables;
using Geom;
using LinqVec.Tools.Cmds.Enums;
using ReactiveVars;
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
	Func<IRoVar<Option<Pt>>, IDisposable> HoverAction
)
{
	public static readonly Hotspot Empty = new("Empty", _ => None, null, _ => Disposable.Empty);
}

public interface IHotspotCmd
{
	string Name { get; }
	Gesture Gesture { get; }
}
public sealed record ClickHotspotCmd(
	string Name,
	Gesture Gesture,
	Func<Option<ToolStateFun>> ClickAction
) : IHotspotCmd;
public sealed record DragHotspotCmd(
	string Name,
	Gesture Gesture,
	Func<Pt, IRoVar<Option<Pt>>, IDisposable> DragAction
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
	public Func<IRoVar<Option<Pt>>, IDisposable> HoverAction { get; init; } = _ => Disposable.Empty;
}

public sealed record HotspotNfo<TH>(
	Hotspot<TH> Hotspot,
	Func<TH, IHotspotCmd[]> ActFuns
);



public static class HotspotExt
{
	public static HotspotNfo Do<TH>(
		this Hotspot<TH> hotspot,
		Func<TH, IHotspotCmd[]> actFuns
	) => new HotspotNfo<TH>(
		hotspot,
		actFuns
	).ToNonGeneric();

	public static Hotspot<TH> WithCursor<TH>(this Hotspot<TH> hotspot, Cursor cursor) => hotspot with { Cursor = cursor };

	private static HotspotNfo ToNonGeneric<TH>(this HotspotNfo<TH> set) => new(
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
