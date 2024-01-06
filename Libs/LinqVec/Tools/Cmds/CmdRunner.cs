using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using LinqVec.Logging;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Logic;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using LogLib;
using LogLib.ConTickerLogic;
using LogLib.Utils;
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
		LogTicker logTicker,
		Disp d
	) => initStateFun.Run(evt, invalidate, logTicker, Rx.Sched, d);



	internal static CmdOutput Run(
		this ToolStateFun initStateFun,
		Evt evt,
		Action invalidate,
		LogTicker logTicker,
		IScheduler scheduler,
		Disp d
	)
	{
		var stateFun = Var.Make(initStateFun, d);
		var state = stateFun.Select(e => (Func<Disp, ToolState>)(d_ => e(d_))).InvokeAndSequentiallyDispose();

		var hotspot = state.TrackHotspot(evt.IsDragging, evt.MousePos, d);
		logTicker.Log(hotspot.RenderHotspot(), d);

		var cmdEvt = hotspot.ToCmdEvt(state, evt.WhenEvt, evt.IsDragging, logTicker, scheduler, d);
		logTicker.Log(cmdEvt.RenderCmd(), d);

		var mouse = evt.MousePos.WhereSome().Prepend(Pt.Zero).ToVar(d);
		SetCursor(state, hotspot, evt.SetCursor, d);
		hotspot.Run_Hotspot_HoverActions(evt.IsDragging, mouse, d);
		cmdEvt.Run_Cmd_Actions(hotspot, evt.IsDragging, mouse, e => stateFun.V = e, d);

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
}
