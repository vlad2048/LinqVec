namespace LinqVec.Tools.Cmds.Events;

// @formatter:off
public interface IRunEvt { string State { get; } string Hotspot { get; } }
public sealed record HotspotChangedRunEvt(string State, string Hotspot) : IRunEvt { public override string ToString() => $"[{State}] HotspotChanged({Hotspot})"; }
public sealed record DragStartRunEvt(string State, string Hotspot, string Cmd) : IRunEvt { public override string ToString() => $"[{State}] DragStart({Hotspot} -> {Cmd})"; }
public sealed record ConfirmRunEvt(string State, string Hotspot, string Cmd) : IRunEvt { public override string ToString() => $"[{State}] Confirm({Hotspot} -> {Cmd})"; }
// @formatter:on