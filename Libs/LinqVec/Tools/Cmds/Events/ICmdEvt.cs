using Geom;
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
