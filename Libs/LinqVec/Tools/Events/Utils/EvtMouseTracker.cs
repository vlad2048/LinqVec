using System.Reactive.Linq;
using Geom;
using PowRxVar;

namespace LinqVec.Tools.Events.Utils;

public static class EvtMouseTracker
{
	public static IObservable<IEvt> TrackMouse(this IObservable<IEvt> src, out IRoVar<Option<Pt>> mousePos, IRoDispBase d)
	{
		var mousePosVar = Var.Make(Option<Pt>.None).D(d);
		mousePos = mousePosVar.ToReadOnly();
		Obs.Merge(
				src.WhenMouseMove().Select(e => Some(e)),
				src.WhenMouseLeave().Select(_ => Option<Pt>.None)
			)
			.Subscribe(v => mousePosVar.V = v).D(d);

		return src;
	}
}