using System.Reactive.Linq;
using Geom;
using ReactiveVars;

namespace LinqVec.Tools.Events.Utils;

public static class EvtMouseTracker
{
	public static IRoVar<Option<Pt>> TrackMouse(this IObservable<IEvt> src, Disp d)
	{
		var mousePosVar = Var.Make(Option<Pt>.None, d);
		Obs.Merge(
				src.WhenMouseMove().Select(e => Some(e)),
				src.WhenMouseLeave().Select(_ => Option<Pt>.None)
			)
			.Subscribe(v => mousePosVar.V = v).D(d);
		return mousePosVar;
	}
}