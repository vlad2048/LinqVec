namespace LinqVec.Tools.Cmds.Structs;

public delegate ToolState ToolStateFun(Disp actD);

public sealed class ToolState(
	string Name,
	Cursor Cursor,
	HotspotCmdsNfo[] Hotspots,
	ShortcutNfo[]? Shortcuts = null
)
{
	public string Name { get; } = Name;
	public Cursor Cursor { get; } = Cursor;
	public HotspotCmdsNfo[] Hotspots { get; } = Hotspots;
	public ShortcutNfo[] Shortcuts { get; } = Shortcuts ?? [];
	internal static readonly ToolState Empty = new("Empty", Cursors.Default, []);
}