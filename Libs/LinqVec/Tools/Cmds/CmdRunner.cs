using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Geom;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Events;
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
		InvokeActions(cmdEvt, evt.MousePos, curHotspot, curStateSet, curStateReset, d);

		var runEvents = GetRunEvents(curState, curHotspot, cmdEvt);

		runEvents.Subscribe(_ => invalidate()).D(d);

		var cmdOutput = new CmdOutput(runEvents, cmdEvt);

		G.Cfg.RunWhen(e => e.Log.LogCmd.RunEvt, d, [() => cmdOutput.WhenRunEvt.LogD("RunEvt")]);
		G.Cfg.RunWhen(e => e.Log.LogCmd.CmdEvt, d, [() => cmdOutput.WhenCmdEvt.LogD("CmdEvt")]);

		return cmdOutput;
	}


	private static (IRoVar<ToolState>, Action<ToolStateFun>, Action) TrackState(
		ToolStateFun initStateFun,
		IObservable<Unit> whenUndoRedo,
		Disp d
	)
	{
		var curStateFun = Var.Make(initStateFun, d);
		whenUndoRedo
			/* Delay needed otherise:
			 *	- create curve
			 *  - switch tool to select (cancels the curve)
			 *  - while the PtrKidCreate unregisters itself it also wrong .WhenUndoRedo event
			 *  - and setting the state here will cause the tool to be reset before the kid finished unregistering
			 */
			.Delay(TimeSpan.Zero, Rx.Sched)
			.Subscribe(_ => curStateFun.V = initStateFun).D(d);
		var curStateSerDisp = new SerDisp().D(d);
		var curState = curStateFun.Select(maker => maker(curStateSerDisp.GetNewD())).ToVar();
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
		IRoVar<Option<Pt>> mouse,
		IRoVar<HotspotNfoResolved> curHotspot,
		Action<ToolStateFun> curStateSet,
		Action curStateReset,
		Disp d
	)
	{
		Action<bool>? dragActionD = null;

		ISubject<Unit> whenDisposeHover = new Subject<Unit>().D(d);
		var WhenDisposeHover = whenDisposeHover.AsObservable();

		Func<IDisposable> Conv(Func<Action<bool>> fun) => () => Disposable.Create(() => fun()(false));

		curHotspot
			.Where(_ => dragActionD == null)
			.Select(t => t.Hotspot.HoverAction)
			.Select<Func<IRoVar<Option<Pt>>, Action<bool>>, Func<Action<bool>>>(e => () => e(mouse))
			.Select(Conv)
			.ObserveOnUI()
			.DisposePreviousSequentiallyOrWhen(WhenDisposeHover, d);

		cmdEvt
			.ObserveOnUI()
			.Subscribe(e =>
			{
				switch (e)
				{
					case DragStartCmdEvt { HotspotCmd: var cmd, PtStart: var ptStart }:
					{
						var dragCmd = (DragHotspotCmd)cmd;
						whenDisposeHover.OnNext(Unit.Default);
						dragActionD = dragCmd.DragAction(ptStart, mouse);
						break;
					}

					case ConfirmCmdEvt { HotspotCmd: var cmd, Pt: var pt }:
					{
						switch (cmd)
						{
							case ClickHotspotCmd clickCmd:
								clickCmd.ClickAction().Match(curStateSet, curStateReset);
								break;
							case DragHotspotCmd dragCmd:
								dragActionD?.Invoke(true);
								//dragActionD = Disposable.Empty;
								dragActionD = null;
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



		/*
		curHotspot
			.Select(t => t.Hotspot.HoverAction)
			.Select<Func<IRoVar<Option<Pt>>, IDisposable>, Func<IDisposable>>(e => () => e(mouse))
			.DisposePrevious()
			.MakeHot(d);

		var dragActionD = Disposable.Empty;

		cmdEvt
			.ObserveOnUI()
			.Subscribe(e =>
			{
				switch (e)
				{
					case DragStartCmdEvt { HotspotCmd: var cmd, PtStart: var ptStart }:
					{
						var dragCmd = (DragHotspotCmd)cmd;
						dragActionD = dragCmd.DragAction(ptStart, mouse);
						break;
					}

					case ConfirmCmdEvt { HotspotCmd: var cmd, Pt: var pt }:
					{
						switch (cmd)
						{
							case ClickHotspotCmd clickCmd:
								clickCmd.ClickAction().Match(curStateSet, curStateReset);
								break;
							case DragHotspotCmd dragCmd:
								dragActionD.Dispose();
								dragActionD = Disposable.Empty;
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
		*/
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
