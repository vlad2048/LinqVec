using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;

static class HotspotTracker
{
	private interface IHotEvt
	{
		Pt Pos { get; }
	}
	private sealed record DownEvt(Pt Pos) : IHotEvt;
	private sealed record UpEvt(Pt Pos) : IHotEvt;
	private sealed record OtherEvt(Pt Pos) : IHotEvt;


	public static IRoVar<HotspotNfoResolved> TrackHotspot(
		this IRoVar<ToolState> curState,
		IObservable<IEvt> evt,
		IObservable<Unit> whenRepeatHotspot,
		IScheduler scheduler,
		Disp d
	) =>
		Obs.Create<HotspotNfoResolved>(obs =>
			{
				var obsD = MkD();

				HotspotNfoResolved last;

				obs.OnNext(last = HotspotNfoResolved.Empty);

				var evtHot = evt.Select(e => e.ToHotEvt()).WhereSome();

				var isDragging =
					Obs.Merge(
							evtHot.Where(e => e is DownEvt).Select(_ => true),
							evtHot.Where(e => e is UpEvt).Select(_ => false)
								.Delay(TimeSpan.Zero, scheduler) // without this delay, the Hotspot can change before the user release the mouse button
						)
						.Prepend(false)
						.ToVar(d);	// !!! we never subscribe to it other than accessing .V (instant subscribe/unsubscribe) so it'll lose its state all the time because of the RefCount()


				evt
					.Select(e => e.ToHotEvt())
					.WhereSome()
					.WithLatestFrom(
						curState,
						(evt_, curState_) => new
						{
							Evt = evt_,
							State = curState_
						}
					)
					.Where(_ => !isDragging.V)
					.Subscribe(t =>
						obs.OnNext(
							last =
								t.State.Hotspots
									.Select(f => f.Hotspot.Fun(t.Evt.Pos).Map(g => new HotspotNfoResolved(f.Hotspot, g, f.Cmds(g), false)))
									.Aggregate()
									.IfNone(HotspotNfoResolved.Empty)
						)
					).D(obsD);

				whenRepeatHotspot
					.Subscribe(_ =>
					{
						obs.OnNext(last with { RepeatFlag = true });
					}).D(obsD);

				return obsD;
			})
			.DistinctUntilChanged(e => (e.Hotspot.Name, e.HotspotValue, e.RepeatFlag))
			.ToVar();


	private static Option<IHotEvt> ToHotEvt(this IEvt evt) =>
		evt switch {
			MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left, Pos: var pos } => new DownEvt(pos),
			MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left, Pos: var pos } => new UpEvt(pos),
			MouseBtnEvt { Pos: var pos } => new OtherEvt(pos),
			MouseMoveEvt { Pos: var pos } => new OtherEvt(pos),
			_ => None,
		};
}
