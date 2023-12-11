using System.Reactive.Linq;
using Geom;
using PowMaybe;
using PowRxVar;

namespace LinqVec.Tools.Events.Utils;

public static class EvtMouseTracker
{
	public static IObservable<IEvt> TrackMouse(this IObservable<IEvt> src, out IRoMayVar<Pt> mousePos, IRoDispBase d)
	{
		mousePos = VarMay.Make(
			Obs.Merge(
				src.WhenMouseMove().Select(May.Some),
				src.WhenMouseLeave().Select(_ => May.None<Pt>())
			)
		).D(d);
		return src;
	}
}