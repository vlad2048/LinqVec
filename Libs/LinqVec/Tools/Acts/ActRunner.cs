using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Acts.Logic;
using LinqVec.Tools.Events;
using LinqVec.Utils.Rx;
using PowBasics.CollectionsExt;
using PowBasics.ColorCode;
using ReactiveVars;

namespace LinqVec.Tools.Acts;


public interface IRunEvt
{
	string State { get; }
	string Hotspot { get; }
}

public sealed record HotspotHoverRunEvt(
	string State,
	string Hotspot,
	bool On
) : IRunEvt;

public sealed record DragStartRunEvt(
	string State,
	string Hotspot,
	string Act
) : IRunEvt;

public sealed record ConfirmRunEvt(
	string State,
	string Hotspot,
	string Act
) : IRunEvt;



public static class ActRunner
{
	public static IObservable<IRunEvt> Run(
		this ActSetMaker actsMakerInit,
		Evt evt,
		Disp d
	)
	{
		var (curActs, curActsSet, curActsReset) = TrackActs(actsMakerInit, evt.WhenUndoRedo, d);
		var curHotActs = curActs.TrackHotspot(evt.WhenEvt, Rx.Sched, d);
		var actEvt = curHotActs.ToActEvt(evt.WhenEvt, Rx.Sched, d);

		SetCursor(curActs, curHotActs, evt.SetCursor, d);
		InvokeActions(actEvt, curActsSet, curActsReset, d);

		var runEvents = GetRunEvents(curActs, curHotActs, evt.MousePos, actEvt);

		G.Cfg.RunWhen(e => e.Log.Tools, d, [
			() => Log(curActs, curHotActs),
			//() => runEvents.LogD(),
		]);

		return runEvents;
	}



	private static IDisposable Log(IRoVar<ActSet> curActs, IRoVar<HotspotActsRun> curHotActs) =>
		Obs.CombineLatest(
				curActs,
				curHotActs,
				(acts, hotActs) => new {
					ActSetName = acts.Name,
					Hotspots = acts.HotspotActSets.SelectToArray(e => e.Hotspot.Name),
					ActiveHotspot = hotActs.Hotspot.Name,
					Actions = hotActs.Acts.SelectToArray(e => $"{e.Name}({e.Gesture})")
				}
			)
			.DistinctUntilChanged()
			.Subscribe(t =>
			{
				L.Write("    ", 0);
				L.Write($"[{t.ActSetName}]".PadRight(20), 0xfff14b);
				var hotspotsStr = t.Hotspots.Select(e => $"{e}  ").JoinText("");
				foreach (var hotspot in t.Hotspots)
					L.Write($"{hotspot}  ", hotspot == t.ActiveHotspot ? 0x58e856 : 0x1b6b1a);
				L.Write(new string(' ', Math.Max(0, 35 - hotspotsStr.Length)));
				L.WriteLine(t.Actions.JoinText("  "), 0x803299);
			});



	private static (IRoVar<ActSet>, Action<ActSetMaker>, Action) TrackActs(
		ActSetMaker actsMakerInit,
		IObservable<Unit> whenUndoRedo,
		Disp d
	)
	{
		var curActsMaker = Var.Make(actsMakerInit, d);
		whenUndoRedo.Subscribe(_ => curActsMaker.V = actsMakerInit).D(d);
		var curActsSerDisp = new SerDisp().D(d);
		var curActs = curActsMaker.Select(maker => maker(curActsSerDisp.GetNewD())).ToVar(d);
		void Set(ActSetMaker maker) => curActsMaker.V = maker;
		void Reset() => curActsMaker.V = curActsMaker.V;
		return (curActs, Set, Reset);
	}


	private static void SetCursor(
		IRoVar<ActSet> curActs,
		IRoVar<HotspotActsRun> curHotActs,
		Action<Cursor?> setCursor,
		Disp d
	)
	{
		curActs.Subscribe(e => setCursor(e.Cursor)).D(d);
		curHotActs.Subscribe(e => setCursor(e.Hotspot.Cursor)).D(d);
	}

	private static void InvokeActions(
		IObservable<IActEvt> actEvt,
		Action<ActSetMaker> curActSet,
		Action curActReset,
		Disp d
	)
	{
		actEvt.Subscribe(e =>
		{
			switch (e)
			{
				case DragStartActEvt { HotspotAct: var act, PtStart: var ptStart }:
					act.Actions.DragStart(ptStart);
					break;

				case ConfirmActEvt { HotspotAct: var act, Pt: var pt }:
					act.Actions.Confirm(pt).Match(curActSet, curActReset);
					break;
			}
		}).D(d);
	}

	private static IObservable<IRunEvt> GetRunEvents(
		IRoVar<ActSet> curActs,
		IRoVar<HotspotActsRun> curHotActs,
		IRoVar<Option<Pt>> mouse,
		IObservable<IActEvt> actEvt
	) =>
		Obs.Create<IRunEvt>(obs =>
		{
			var obsD = MkD();

			curHotActs
				.Buffer(2, 1)
				.Select(e => new {
					Prev = e[0],
					Next = e[1]
				})
				.Where(_ => mouse.V.IsSome)
				.Subscribe(t =>
				{
					obs.OnNext(new HotspotHoverRunEvt(curActs.V.Name, t.Prev.Hotspot.Name, false));
					obs.OnNext(new HotspotHoverRunEvt(curActs.V.Name, t.Next.Hotspot.Name, true));
				}).D(obsD);

			actEvt
				.Subscribe(e =>
				{
					switch (e)
					{
						case DragStartActEvt { HotspotAct: var act }:
							obs.OnNext(new DragStartRunEvt(curActs.V.Name, curHotActs.V.Hotspot.Name, act.Name));
							break;

						case ConfirmActEvt { HotspotAct: var act }:
							obs.OnNext(new ConfirmRunEvt(curActs.V.Name, curHotActs.V.Hotspot.Name, act.Name));
							break;
					}
				}).D(obsD);

			return obsD;
		});



	private static void Write(this TxtWriter w, string text, int color) => w.Write(text, MkCol(color));
	private static void WriteLine(this TxtWriter w, string text, int color) => w.WriteLine(text, MkCol(color));
}




/*
public static class BaseActIds
{
	public const string Empty = nameof(Empty);
}

public static class ActRunner
{
	public static IObservable<ActGfxEvt> Run(
		this ActMaker actsInit,
		Evt evt,
		Disp d
	)
	{
		var curActs = new BehaviorSubject<ActMaker>(actsInit).D(d);
		var actSetDbg = Var.Make(ActSet.Empty).D(d);
		evt.WhenUndoRedo.Subscribe(_ => curActs.OnNext(actsInit)).D(d);
		void Reset() => curActs.OnNext(curActs.Value);
		var whenGfxEvt = new Subject<ActGfxEvt>().D(d);

		var serDisp = new SerDisp().D(d);
		var curHot = Var.Make<Option<HotAct>>(None).D(d);

		curActs.Subscribe(actSetF =>
		{
			var serD = serDisp.GetNewD();
			var actSet = actSetF(serD);
			actSetDbg.V = actSet;

			var isHotLocked = false;
			//var curHot = HotspotTracker.Track(evt, actSet, () => isHotLocked);
			HotspotTracker.Track(curHot, evt, actSet, () => isHotLocked).D(serD);

			var evtD = new Disp(); // TODO
			new ScheduledDisposable(Rx.Sched, evtD).D(serD);
			var actEvt = evt.ToActEvt(actSet.Acts.Select(e => e.Gesture), evtD);

			curHot.SetCursor(actSet.Cursor, evt.SetCursor).D(serD);
			curHot.TriggerHoverActions(actSet.Name, whenGfxEvt.OnNext).D(serD);
			actEvt.TriggerDragAndClickActions(
				actSet.Name,
				curHot,
				e => isHotLocked = e,
				Reset,
				curActs.OnNext,
				whenGfxEvt.OnNext
			).D(serD);
		}).D(d);

		var WhenGfxEvt = whenGfxEvt.AsObservable();

		G.Cfg.RunWhen(e => e.Log.Tools, () =>
		{
			var logD = new Disp();

			Obs.Merge(
					curHot
						.Select(curHot_ => curHot_.Map(e => e.Act))
						.DistinctUntilChanged()
						.Select(curAct => new Msg(0, () =>
						{
							var acts = actSetDbg.V.Acts;
							L.Write("      ");
							foreach (var act in acts)
							{
								var col = act == curAct ? 0x58e856 : 0x1b6b1a;
								L.Write($"{act.Id} ", col);
							}
							L.WriteLine();
						})),
					WhenGfxEvt
						.Where(e => e.State is not ActGfxState.Hover)
						.Select(e => new Msg(1, () => L.WriteLine($"      {e}", 0x6510af))),
					actSetDbg
						.Select(actSet => new Msg(2, () =>
						{
							L.WriteLine($"  [{actSet.Name}]", 0xb7d328);
							foreach (var act in actSet.Acts)
								L.WriteLine($"    - {act.Id} ({act.Gesture})", 0x738519);
						}))
				)
				.Buffer(TimeSpan.FromMilliseconds(10))
				.Where(e => e.Any())
				.Subscribe(msgs =>
				{
					foreach (var msg in msgs.OrderBy(e => e.Priority))
						msg.Action();
				}).D(logD);


			return logD;
		}).D(d);

		return WhenGfxEvt;
	}

	private sealed record Msg(int Priority, Action Action);

}
*/