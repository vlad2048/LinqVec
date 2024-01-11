using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using LinqVec.Logging;
using LinqVec.Structs;
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
	IRoVar<Option<string>> DragAction,
	IRoVar<Action<Gfx>> PaintActionMay
);




public static class CmdRunner
{
	public static CmdOutput Run(
		this Func<ToolState> stateFun,
		Evt evt,
		LogTicker logTicker,
		Disp d
	) => stateFun.Run(evt, logTicker, Rx.Sched, d);



	internal static CmdOutput Run(
		this Func<ToolState> stateFun,
		Evt evt,
		LogTicker logTicker,
		IScheduler scheduler,
		Disp d
	)
	{
		var (stateRecalc, whenStateRecalc) = RxEventMaker.Make(d);
		var state = whenStateRecalc.Prepend(Unit.Default).Select(_ => stateFun()).ToVar(d);

		var hotspot = state.TrackHotspot(evt.IsMouseDown, evt.MousePos, d);
		logTicker.Log(hotspot.RenderHotspot(), d);

		var cmdEvt = hotspot.ToCmdEvt(state, evt.WhenEvt, scheduler, d);
		logTicker.Log(cmdEvt.RenderCmd(), d);

		var mouse = evt.MousePos.WhereSome().Prepend(Pt.Zero).ToVar(d);
		SetCursor(state, hotspot, evt.SetCursor, d);
		//hotspot.Run_Hotspot_HoverActions(evt.IsDragging, mouse, d);
		var dragAction = cmdEvt.Run_Cmd_Actions(mouse, stateRecalc, d);

		return new CmdOutput(
			dragAction,
			hotspot.Select(m => m.Match(
				e => Mk(gfx =>
				{
					if (evt.IsMouseDown.V) return;
					e.HotspotNfo.HoverAction(e.HotspotValue, gfx);
				}),
				() => Mk(_ => {})
			))
			.ToVar()
		);
	}

	private static Action<Gfx> Mk(Action<Gfx> a) => a;




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
