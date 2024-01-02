using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Geom;
using LinqVec.Logging;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using LogLib;
using LogLib.Structs;
using ReactiveVars;

namespace LinqVec.Tools.Cmds;


public sealed record CmdOutput(
	//IObservable<IRunEvt> WhenRunEvt,
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
		LogCategories.Setup_Time_Logging(scheduler, d);

		LogCategories.Setup_Evt_Logging(evt.WhenEvt, scheduler, d);

		var stateFun = Var.Make(initStateFun, d);
		var state = stateFun.Select(e => (Func<Disp, ToolState>)(d_ => e(d_))).InvokeAndSequentiallyDispose();
		var isHotspotFrozen = Var.Make(false, d);
		var hotspot = state.TrackHotspot(isHotspotFrozen, evt.MousePos, d);

		LogCategories.Setup_Hotspot_Logging(isHotspotFrozen, hotspot, scheduler, d);

		var cmdEvt = hotspot.ToCmdEvt(state, evt.WhenEvt, scheduler, d);

		LogCategories.Setup_CmdEvt_Logging(cmdEvt, scheduler, d);

		var mouse = evt.MousePos.WhereSome().Prepend(Pt.Zero).ToVar(d);
		SetCursor(state, hotspot, evt.SetCursor, d);
		hotspot.Run_Hotspot_HoverActions(isHotspotFrozen, mouse, d);
		cmdEvt.Run_Cmd_Actions(hotspot, isHotspotFrozen, mouse, e => stateFun.V = e, d);

		//G.Cfg.RunWhen(e => e.Log.LogCmd.Evt, d, [() => evt.WhenEvt.LogD("Evt")]);
		//G.Cfg.RunWhen(e => e.Log.LogCmd.Hotspot, d, [() => hotspot.LogD("Hotspot")]);
		//G.Cfg.RunWhen(e => e.Log.LogCmd.CmdEvt, d, [() => cmdEvt.TimePrefix(scheduler).LogD("CmdEvt")]);
		
		return new CmdOutput(
			cmdEvt
		);
	}






	private static void SetCursor(
		IRoVar<ToolState> state,
		IRoVar<Option<Hotspot>> hotspot,
		Action<Cursor?> setCursor,
		Disp d
	)
	{
		state
			.Subscribe(e => setCursor(e.Cursor)).D(d);
		hotspot
			.WhereSome()
			.Subscribe(e => setCursor(e.HotspotNfo.Cursor)).D(d);
	}


	/*internal static CmdOutput Run(
		this ToolStateFun initStateFun,
		Evt evt,
		Action invalidate,
		IScheduler scheduler,
		DISP d
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
		DISP d
	)
	{
		var curStateFun = Var.Make(initStateFun, d);
		whenUndoRedo
		// Delay needed otherise:
		//		- create curve
		//		- switch tool to select (cancels the curve)
		//		- while the PtrKidCreate unregisters itself it also wrong .WhenUndoRedo event
		//		- and setting the state here will cause the tool to be reset before the kid finished unregistering
			.Delay(TimeSpan.Zero, Rx.Sched)
			.Subscribe(_ => curStateFun.V = initStateFun).D(d);
		var curStateSerDisp = new SerDisp().D(d);
		var curState = curStateFun.Select(maker => maker(curStateSerDisp.GetNewD())).ToVar();
		void Set(ToolStateFun maker) => curStateFun.V = maker;
		void Reset() => curStateFun.V = curStateFun.V;

		return (curState, Set, Reset);
	}



	private static void InvokeActions(
		IObservable<ICmdEvt> cmdEvt,
		IRoVar<Option<Pt>> mouse,
		IRoVar<Hotspot> curHotspot,
		Action<ToolStateFun> curStateSet,
		Action curStateReset,
		DISP d
	)
	{
		Action<bool>? dragActionD = null;

		ISubject<Unit> whenDisposeHover = new Subject<Unit>().D(d);
		var WhenDisposeHover = whenDisposeHover.AsObservable();

		Func<IDisposable> Conv(Func<Action<bool>> fun) => () => Disposable.Create(() => fun()(false));

		curHotspot
			.Where(_ => dragActionD == null)
			.Select(t => t.HotspotNfo.HoverAction)
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
	}

	
	private static IObservable<IRunEvt> GetRunEvents(
		IRoVar<ToolState> curState,
		IRoVar<Hotspot> curHotspot,
		IObservable<ICmdEvt> cmdEvt
	) =>
		Obs.Create<IRunEvt>(obs =>
		{
			var obsD = MkD("CmdRunner");

			curHotspot
				.Subscribe(t => obs.OnNext(new HotspotChangedRunEvt(curState.V.Name, t.HotspotNfo.Name))).D(obsD);

			cmdEvt
				.Subscribe(e =>
				{
					switch (e)
					{
						case DragStartCmdEvt { HotspotCmd: var cmd }:
							obs.OnNext(new DragStartRunEvt(curState.V.Name, curHotspot.V.HotspotNfo.Name, cmd.Name));
							break;

						case ConfirmCmdEvt { HotspotCmd: var cmd }:
							obs.OnNext(new ConfirmRunEvt(curState.V.Name, curHotspot.V.HotspotNfo.Name, cmd.Name));
							//obs.OnNext(new HotspotChangedRunEvt(curState.V.Name, curHotspot.V.Hotspot.Name));
							break;
					}
				}).D(obsD);

			return obsD;
		});
	*/
}
