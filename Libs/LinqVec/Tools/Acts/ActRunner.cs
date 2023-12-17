using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Tools.Acts.Delegates;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Acts.Logic;
using LinqVec.Tools.Acts.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using PowRxVar;

namespace LinqVec.Tools.Acts;

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