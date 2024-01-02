using System.Reactive.Disposables;
using Geom;
using LinqVec.Tools.Cmds.Structs;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;

static class HotspotHoverActionRunner
{
	public static void Run_Hotspot_HoverActions(
		this IRoVar<Option<Hotspot>> hotspot,
		IRoVar<bool> isHotspotFrozen,
		IRoVar<Pt> mouse,
		Disp d
	)
	{
		var serD = new SerialDisposable().D(d);

		Obs.CombineLatest(
			hotspot,
			isHotspotFrozen,
			(hotspotOpt, isHotspotFrozen_) => (hotspotOpt, isHotspotFrozen_)
		)
			.Subscribe(t =>
			{
				serD.Disposable = null;
				if (t.isHotspotFrozen_) return;
				t.hotspotOpt.IfSome(hotspot_ =>
				{
					var stopFun = hotspot_.HotspotNfo.HoverAction(mouse);
					//L.WriteLine("----> Enter");
					serD.Disposable = Disposable.Create(() =>
					{
						//L.WriteLine("----> Exit");
						stopFun(false);
					});
				});
			}).D(d);
	}
}