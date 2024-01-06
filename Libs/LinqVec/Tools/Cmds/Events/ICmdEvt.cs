using Geom;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Tools.Cmds.Structs;

namespace LinqVec.Tools.Cmds.Events;

public interface IIHotspotCmdEvt : ICmdEvt
{
	IHotspotCmd HotspotCmd { get; }
};

// @formatter:off
public interface ICmdEvt;
public sealed record DragStartCmdEvt(IHotspotCmd HotspotCmd, Pt PtStart) : IIHotspotCmdEvt { public override string ToString() => $"DragStart({PtStart})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; }
public sealed record DragFinishCmdEvt(IHotspotCmd HotspotCmd, Pt PtStart, Pt PtEnd) : IIHotspotCmdEvt { public override string ToString() => $"DragFinish({PtStart})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; }
public sealed record ConfirmCmdEvt(IHotspotCmd HotspotCmd, Pt Pt) : IIHotspotCmdEvt { public override string ToString() => $"Confirm({Pt})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; }
public sealed record ShortcutCmdEvt(ShortcutNfo ShortcutNfo) : ICmdEvt { public override string ToString() => $"Shortcut({ShortcutNfo.Key})".PadRight(20) + $"(hotspot:{ShortcutNfo.Name})"; }
public sealed record CancelCmdEvt : ICmdEvt { public override string ToString() => "CancelCmdEvt"; }
// @formatter:on


static class CmdStorybookSamples
{
	private static readonly Pt p0 = new(-10, -10);
	private static readonly Pt p1 = new(2, 3);
	private static readonly Pt p2 = new(-5, 2);
	private static readonly Pt p3 = new(-4, 5);
	private static readonly Pt p4 = new(7, -8);
	private static readonly IHotspotCmd hotspotCmd = new DragHotspotCmd("CmdName", Gesture.DoubleClick, (_, _) => _ => { });

	public static ICmdEvt[] Samples = [
		new DragStartCmdEvt(hotspotCmd, p4),
		new DragFinishCmdEvt(hotspotCmd, p0, p1),
		new ConfirmCmdEvt(hotspotCmd, p2),
		new ShortcutCmdEvt(new ShortcutNfo("ShortcutAction", Keys.T, () => { })),
		new CancelCmdEvt(),
	];
}
