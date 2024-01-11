using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Utils;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;

static class HotspotTracker
{
	public static IRoVar<Option<Hotspot>> TrackHotspot(
		this IRoVar<ToolState> state,
		IRoVar<bool> isMouseDown,
		IRoVar<Option<Pt>> mousePos,
		Disp d
	) =>
		mousePos
			.WithLatestFrom(state, (mousePos_, state_) => (mousePos_, state_))
			.WithLatestFrom(isMouseDown, (t_, isMouseDown_) => (t_.mousePos_, t_.state_, isMouseDown_))
			.Select(t => !t.isMouseDown_ switch {
				false =>
					Option<Hotspot>.None,
				true =>
					t.mousePos_.Match(
						mousePos_ =>
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
								)),
						() => None
					)
			})
			.Prepend(Option<Hotspot>.None)
			.DistinctUntilChanged(e => e.Map(f => (f.HotspotNfo.Name, f.HotspotValue)))
			.ToVar(d);
}
