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

			state.WithLatestFrom(isDragging, (state_, _) => state_)
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
			d
		);
}
