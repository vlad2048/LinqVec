using System.Reactive.Linq;
using Geom;
using PowRxVar;

namespace LinqVec.Tools.Events.Utils;

public static class EvtMouseTracker
{
	public static (IRoVar<Option<Pt>>, IDisposable) TrackMouse(this IObservable<IEvt> src)
	{
		var d = new Disp();
		var mousePosVar = Var.Make(Option<Pt>.None).D(d);
		Obs.Merge(
				src.WhenMouseMove().Select(e => Some(e)),
				src.WhenMouseLeave().Select(_ => Option<Pt>.None)
			)
			.Subscribe(v => mousePosVar.V = v).D(d);
		return (mousePosVar.ToReadOnly(), d);
	}
}