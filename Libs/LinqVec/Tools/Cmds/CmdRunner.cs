using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;

namespace LinqVec.Tools.Cmds;


// @formatter:off
public interface ICmdEvt;
public sealed record DragStartCmdEvt(IHotspotCmd HotspotCmd, Pt PtStart) : ICmdEvt { public override string ToString() => $"[{HotspotCmd.Name}].DragStart({PtStart})"; }
public sealed record ConfirmCmdEvt(IHotspotCmd HotspotCmd, Pt Pt) : ICmdEvt { public override string ToString() => $"[{HotspotCmd.Name}].Confirm({Pt})"; }
public sealed record ShortcutCmdEvt(ShortcutNfo ShortcutNfo) : ICmdEvt { public override string ToString() => $"[{ShortcutNfo.Name}].Shortcut({ShortcutNfo.Key})"; }

public interface IRunEvt { string State { get; } string Hotspot { get; } }
public sealed record HotspotChangedRunEvt(string State, string Hotspot) : IRunEvt { public override string ToString() => $"[{State}] HotspotChanged({Hotspot})"; }
public sealed record DragStartRunEvt(string State, string Hotspot, string Cmd) : IRunEvt { public override string ToString() => $"[{State}] DragStart({Hotspot} -> {Cmd})"; }
public sealed record ConfirmRunEvt(string State, string Hotspot, string Cmd) : IRunEvt { public override string ToString() => $"[{State}] Confirm({Hotspot} -> {Cmd})"; }
// @formatter:on


public sealed record CmdOutput(
	IObservable<IRunEvt> WhenRunEvt,
	IObservable<ICmdEvt> WhenCmdEvt
);


public static class CmdRunner
{
	public static CmdOutput Run(
		this ToolStateFun initStateFun,
		Evt evt,
		Action invalidate,
		Disp d
	) => initStateFun.Run(evt, invalidate, Rx.Sched, d);


	internal static CmdOutput Run(
		this ToolStateFun initStateFun,
		Evt evt,
		Action invalidate,
		IScheduler scheduler,
		Disp d
	)
	{
		var (curState, curStateSet, curStateReset) = TrackState(initStateFun, evt.WhenUndoRedo, d);
		var (repeatHotspot, whenRepeatHotspot) = RxEventMaker.Make(d);
		curState.Subscribe(_ => repeatHotspot()).D(d);
		var curHotspot = curState.TrackHotspot(evt.WhenEvt, whenRepeatHotspot, scheduler, d);
		var cmdEvt = curHotspot.ToCmdEvt(curState, evt.WhenEvt, scheduler, d).MakeHot(d);

		SetCursor(curState, curHotspot, evt.SetCursor, d);
		InvokeActions(cmdEvt, curHotspot, curStateSet, curStateReset, d);

		var runEvents = GetRunEvents(curState, curHotspot, cmdEvt);

		runEvents.Subscribe(_ => invalidate()).D(d);

		var cmdOutput = new CmdOutput(runEvents, cmdEvt);

		G.Cfg.RunWhen(e => e.Log.LogCmd.RunEvt, d, [() => cmdOutput.WhenRunEvt.LogD("RunEvt")]);
		G.Cfg.RunWhen(e => e.Log.LogCmd.CmdEvt, d, [() => cmdOutput.WhenCmdEvt.LogD("CmdEvt")]);

		return cmdOutput;
	}


	/*
	private static IDisposable Log(IRoVar<ToolState> curState, IRoVar<HotspotNfoResolved> curHotspot) =>
		Obs.CombineLatest(
				curState,
				curHotspot,
				(state, hotspot) => new {
					StateName = state.Name,
					Hotspots = state.Hotspots.SelectToArray(e => e.Hotspot.Name),
					ActiveHotspot = hotspot.Hotspot.Name,
					Actions = hotspot.Cmds.SelectToArray(e => $"{e.Name}({e.Gesture})")
				}
			)
			.DistinctUntilChanged()
			.Subscribe(t =>
			{
				L.Write("    ", 0);
				L.Write($"[{t.StateName}]".PadRight(20), 0xfff14b);
				var hotspotsStr = t.Hotspots.Select(e => $"{e}  ").JoinText("");
				foreach (var hotspot in t.Hotspots)
					L.Write($"{hotspot}  ", hotspot == t.ActiveHotspot ? 0x58e856 : 0x1b6b1a);
				L.Write(new string(' ', Math.Max(0, 35 - hotspotsStr.Length)));
				L.WriteLine(t.Actions.JoinText("  "), 0x803299);
			});
	*/


	private static (IRoVar<ToolState>, Action<ToolStateFun>, Action) TrackState(
		ToolStateFun initStateFun,
		IObservable<Unit> whenUndoRedo,
		Disp d
	)
	{
		var curStateFun = Var.Make(initStateFun, d);
		whenUndoRedo.Subscribe(_ => curStateFun.V = initStateFun).D(d);
		var curStateSerDisp = new SerDisp().D(d);
		var curState = curStateFun.Select(maker => maker(curStateSerDisp.GetNewD())).ToVar(d);
		void Set(ToolStateFun maker) => curStateFun.V = maker;
		void Reset() => curStateFun.V = curStateFun.V;

		return (curState, Set, Reset);
	}


	private static void SetCursor(
		IRoVar<ToolState> curState,
		IRoVar<HotspotNfoResolved> curHotspot,
		Action<Cursor?> setCursor,
		Disp d
	)
	{
		curState.Subscribe(e => setCursor(e.Cursor)).D(d);
		curHotspot.Subscribe(e => setCursor(e.Hotspot.Cursor)).D(d);
	}

	private static void InvokeActions(
		IObservable<ICmdEvt> cmdEvt,
		IRoVar<HotspotNfoResolved> curHotspot,
		Action<ToolStateFun> curStateSet,
		Action curStateReset,
		Disp d
	)
	{
		curHotspot
			.Subscribe(t =>
			{
				t.Hotspot.HoverAction();
			}).D(d);

		Action? curDragFinishAction = null;

		cmdEvt
			.ObserveOnUI()
			.Subscribe(e =>
			{
				switch (e)
				{
					case DragStartCmdEvt { HotspotCmd: var cmd, PtStart: var ptStart }:
					{
						var dragCmd = (DragHotspotCmd)cmd;
						curDragFinishAction = dragCmd.Action(ptStart);
						break;
					}

					case ConfirmCmdEvt { HotspotCmd: var cmd, Pt: var pt }:
					{
						switch (cmd)
						{
							case ClickHotspotCmd clickCmd:
								clickCmd.Action().Match(curStateSet, curStateReset);
								break;
							case DragHotspotCmd dragCmd:
								curDragFinishAction?.Invoke();
								curDragFinishAction = null;
								curStateReset();
								break;
						}

						break;
					}

					case ShortcutCmdEvt { ShortcutNfo: var shortcutNfo }:
					{
						shortcutNfo.Action();
						break;
					}
				}
			}).D(d);
	}

	private static IObservable<IRunEvt> GetRunEvents(
		IRoVar<ToolState> curState,
		IRoVar<HotspotNfoResolved> curHotspot,
		IObservable<ICmdEvt> cmdEvt
	) =>
		Obs.Create<IRunEvt>(obs =>
		{
			var obsD = MkD();

			curHotspot
				.Subscribe(t => obs.OnNext(new HotspotChangedRunEvt(curState.V.Name, t.Hotspot.Name))).D(obsD);

			cmdEvt
				.Subscribe(e =>
				{
					switch (e)
					{
						case DragStartCmdEvt { HotspotCmd: var cmd }:
							obs.OnNext(new DragStartRunEvt(curState.V.Name, curHotspot.V.Hotspot.Name, cmd.Name));
							break;

						case ConfirmCmdEvt { HotspotCmd: var cmd }:
							obs.OnNext(new ConfirmRunEvt(curState.V.Name, curHotspot.V.Hotspot.Name, cmd.Name));
							//obs.OnNext(new HotspotChangedRunEvt(curState.V.Name, curHotspot.V.Hotspot.Name));
							break;
					}
				}).D(obsD);

			return obsD;
		});
}
