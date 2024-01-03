using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;

static class HotspotTracker
{
	public static IRoVar<Option<Hotspot>> TrackHotspot(
		this IRoVar<ToolState> state,
		IRoVar<bool> isDragging,
		IRoVar<Option<Pt>> mousePos,
		Disp d
	) =>
		Var.MakeOptionalFromOptionalObs(

			state.WithLatestFrom(isDragging, (state_, isDragging_) => state_)
				.Select(state_ => mousePos.Map2(mousePos_ =>
							state_.Hotspots
								.Select(hotspotCmdsNfo => hotspotCmdsNfo.Hotspot.Fun(mousePos_)
									.Map(hotspotValue => new {
										hotspotValue,
										hotspotCmdsNfo
									})
								)
								.Aggregate()
								.Map(u => new Hotspot(
									u.hotspotCmdsNfo.Hotspot,
									u.hotspotValue,
									u.hotspotCmdsNfo.Cmds(u.hotspotValue),
									false
								))
						))
				.Switch()
				.Select(e => e.Flatten())
				.DistinctUntilChanged(e => e.Map(f => f.HotspotNfo))
				.MakeHot(d),

			/*
			Obs.CombineLatest(
				state,
				hotspotTrackingEnabled,
				(state_, hotspotTrackingEnabled_) => (state_, hotspotTrackingEnabled_)
			)
				.Select(t =>
					t.hotspotTrackingEnabled_ switch
					{
						false => Obs.Return<Option<Option<Hotspot>>>(None),
						true => mousePos.Map2(mousePos_ =>
							t.state_.Hotspots
								.Select(hotspotCmdsNfo => hotspotCmdsNfo.Hotspot.Fun(mousePos_)
									.Map(hotspotValue => new {
										hotspotValue,
										hotspotCmdsNfo
									})
								)
								.Aggregate()
								.Map(u => new Hotspot(
									u.hotspotCmdsNfo.Hotspot,
									u.hotspotValue,
									u.hotspotCmdsNfo.Cmds(u.hotspotValue),
									false
								))
						)
					}
				)
				.Switch()
				.Select(e => e.Flatten())
				.DistinctUntilChanged(e => e.Map(f => f.HotspotNfo))
				.MakeHot(d),
			*/

			d
		);




	/*
	private interface IHotEvt
	{
		Pt Pos { get; }
	}
	private sealed record DownEvt(Pt Pos) : IHotEvt;
	private sealed record UpEvt(Pt Pos) : IHotEvt;
	private sealed record OtherEvt(Pt Pos) : IHotEvt;


	public static IRoVar<Hotspot> TrackHotspot(
		this IRoVar<ToolState> curState,
		IObservable<IEvt> evt,
		IObservable<Unit> whenRepeatHotspot,
		IScheduler scheduler,
		DISP d
	) =>
		Obs.Create<Hotspot>(obs =>
			{
				var obsD = MkD("CmdRunner");

				Hotspot last;

				obs.OnNext(last = Hotspot.Empty);

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
									.Select(f => f.Hotspot.Fun(t.Evt.Pos).Map(g => new Hotspot(f.Hotspot, g, f.Cmds(g), false)))
									.Aggregate()
									.IfNone(Hotspot.Empty)
						)
					).D(obsD);

				whenRepeatHotspot
					.Subscribe(_ =>
					{
						obs.OnNext(last with { RepeatFlag = true });
					}).D(obsD);

				return obsD;
			})
			.DistinctUntilChanged(e => (e.HotspotNfo.Name, e.HotspotValue, e.RepeatFlag))
			.ToVar();


	private static Option<IHotEvt> ToHotEvt(this IEvt evt) =>
		evt switch {
			MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left, Pos: var pos } => new DownEvt(pos),
			MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left, Pos: var pos } => new UpEvt(pos),
			MouseBtnEvt { Pos: var pos } => new OtherEvt(pos),
			MouseMoveEvt { Pos: var pos } => new OtherEvt(pos),
			_ => None,
		};
	*/
}
