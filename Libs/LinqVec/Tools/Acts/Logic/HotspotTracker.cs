using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;

namespace LinqVec.Tools.Acts.Logic;

static class HotspotTracker
{
	private interface IHotEvt
	{
		Pt Pos { get; }
	}
	private sealed record DownEvt(Pt Pos) : IHotEvt;
	private sealed record UpEvt(Pt Pos) : IHotEvt;
	private sealed record OtherEvt(Pt Pos) : IHotEvt;


	public static IRoVar<HotspotActsRun> TrackHotspot(
		this IRoVar<ActSet> curActs,
		IObservable<IEvt> evt,
		IScheduler scheduler,
		Disp d
	) =>
		Obs.Create<HotspotActsRun>(obs =>
			{
				var obsD = MkD();

				obs.OnNext(HotspotActsRun.Empty);

				var evtHot = evt.Select(e => e.ToHotEvt()).WhereSome();

				var isDragging =
					Obs.Merge(
							evtHot.Where(e => e is DownEvt).Select(_ => true),
							evtHot.Where(e => e is UpEvt).Select(_ => false).Delay(TimeSpan.Zero, scheduler)
						)
						.Prepend(false)
						.ToVar(d);


				evt
					.Select(e => e.ToHotEvt())
					.WhereSome()
					.WithLatestFrom(
						curActs,
						(evt_, curActs_) => new
						{
							Evt = evt_,
							Acts = curActs_
						}
					)
					.Where(_ => !isDragging.V)
					.Subscribe(t =>
						obs.OnNext(
							t.Acts.HotspotActSets
								.Select(f => f.Hotspot.Fun(t.Evt.Pos).Map(g => new HotspotActsRun(f.Hotspot, g, f.ActFuns(g))))
								.Aggregate()
								.IfNone(HotspotActsRun.Empty)
						)
					).D(obsD);

				return obsD;
			})
			.DistinctUntilChanged(e => (e.Hotspot.Name, e.HotspotValue))
			.ToVar(d);


	private static Option<IHotEvt> ToHotEvt(this IEvt evt) =>
		evt switch {
			MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left, Pos: var pos } => new DownEvt(pos),
			MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left, Pos: var pos } => new UpEvt(pos),
			MouseBtnEvt { Pos: var pos } => new OtherEvt(pos),
			MouseMoveEvt { Pos: var pos } => new OtherEvt(pos),
			_ => None,
		};
}




/*
using LinqVec.Tools.Acts.Delegates;
using LinqVec.Tools.Acts.Structs;
using LinqVec.Tools.Events;
using PowRxVar;

namespace LinqVec.Tools.Acts.Logic;

static class HotspotTracker
{
	public static IDisposable Track(
		IRwVar<Option<HotAct>> curHot,
		Evt evt,
		ActSet actSet,
		Func<bool> isLocked
	)
	{
		var d = MkD();
		evt.MousePos
			.Subscribe(mouseOpt => mouseOpt.Match(
				mouse =>
				{
					foreach (var act in actSet.Acts)
					{
						var mayH = act.Hotspot.Fun(mouse);

						var ret = false;
						mayH.IfSome(h =>
						{
							var isAlreadySet = curHot.V.Map(e => e.Act) == act;
							if (!isAlreadySet && !isLocked())
								curHot.V = new HotAct(h, act, mouse);
							ret = true;
						});
						if (ret)
							return;
					}
					if (!isLocked())
						curHot.V = None;
				},
				() =>
				{
					if (!isLocked())
						curHot.V = None;
				})).D(d);

		return d;
	}
}
*/
