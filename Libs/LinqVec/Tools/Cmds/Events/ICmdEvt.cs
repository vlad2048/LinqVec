using Geom;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Tools.Cmds.Structs;

namespace LinqVec.Tools.Cmds.Events;

public interface IIHotspotCmdEvt : ICmdEvt;

// @formatter:off
public interface ICmdEvt;
public sealed record DragStartCmdEvt(DragHotspotCmd HotspotCmd, Pt PtStart) : IIHotspotCmdEvt { public override string ToString() => $"DragStart({PtStart})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; }
public sealed record DragFinishCmdEvt(DragHotspotCmd HotspotCmd, Pt PtStart, Pt PtEnd) : IIHotspotCmdEvt { public override string ToString() => $"DragFinish({PtStart})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; }
public sealed record ConfirmCmdEvt(ClickHotspotCmd HotspotCmd, Pt Pt) : IIHotspotCmdEvt { public override string ToString() => $"Confirm({Pt})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; }
public sealed record ShortcutCmdEvt(ShortcutNfo ShortcutNfo) : ICmdEvt { public override string ToString() => $"Shortcut({ShortcutNfo.Key})".PadRight(20) + $"(hotspot:{ShortcutNfo.Name})"; }
public sealed record CancelCmdEvt : ICmdEvt { public override string ToString() => "CancelCmdEvt"; }
// @formatter:on


static class CmdStorybookSamples
{
	private static readonly Pt p0 = new(-10, -10);
	private static readonly Pt p1 = new(2, 3);
	private static readonly Pt p2 = new(-5, 2);
	private static readonly Pt p3 = new(7, -8);
	private static readonly ClickHotspotCmd clickHotspotCmd = new("CmdName", Gesture.DoubleClick, () => {});
	private static readonly DragHotspotCmd dragHotspotCmd = new("CmdName", (_, _) => _ => { });

	public static ICmdEvt[] Samples = [
		new DragStartCmdEvt(dragHotspotCmd, p3),
		new DragFinishCmdEvt(dragHotspotCmd, p0, p1),
		new ConfirmCmdEvt(clickHotspotCmd, p2),
		new ShortcutCmdEvt(new ShortcutNfo("ShortcutAction", Keys.T, () => { })),
		new CancelCmdEvt(),
	];
}
