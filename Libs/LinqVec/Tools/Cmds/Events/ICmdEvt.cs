using Geom;
using LinqVec.Logging;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Utils;
using LogLib;
using LogLib.Interfaces;
using LogLib.Structs;

namespace LinqVec.Tools.Cmds.Events;

public interface IIHotspotCmdEvt : ICmdEvt
{
	IHotspotCmd HotspotCmd { get; }
};

// @formatter:off
public interface ICmdEvt : IWriteSer;
public sealed record DragStartCmdEvt(IHotspotCmd HotspotCmd, Pt PtStart) : IIHotspotCmdEvt { public override string ToString() => $"DragStart({PtStart})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; public ITxtWriter Write(ITxtWriter w) => this.Color(w); }
public sealed record DragFinishCmdEvt(IHotspotCmd HotspotCmd, Pt PtStart, Pt PtEnd) : IIHotspotCmdEvt { public override string ToString() => $"DragFinish({PtStart})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; public ITxtWriter Write(ITxtWriter w) => this.Color(w); }
public sealed record ConfirmCmdEvt(IHotspotCmd HotspotCmd, Pt Pt) : IIHotspotCmdEvt { public override string ToString() => $"Confirm({Pt})".PadRight(20) + $"(cmd:{HotspotCmd.Name})"; public ITxtWriter Write(ITxtWriter w) => this.Color(w); }
public sealed record ShortcutCmdEvt(ShortcutNfo ShortcutNfo) : ICmdEvt { public override string ToString() => $"Shortcut({ShortcutNfo.Key})".PadRight(20) + $"(hotspot:{ShortcutNfo.Name})"; public ITxtWriter Write(ITxtWriter w) => this.Color(w); }
public sealed record CancelCmdEvt : ICmdEvt { public override string ToString() => "CancelCmdEvt"; public ITxtWriter Write(ITxtWriter w) => this.Color(w); }
// @formatter:on
