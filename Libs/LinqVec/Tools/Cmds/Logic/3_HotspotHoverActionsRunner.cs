/*
using System.Reactive.Disposables;
using Geom;
using LinqVec.Tools.Cmds.Structs;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;

static class HotspotHoverActionRunner
{
	public static void Run_Hotspot_HoverActions(
		this IRoVar<Option<Hotspot>> hotspot,
		IRoVar<bool> isDragging,
		IRoVar<Pt> mouse,
		Disp d
	)
	{
		var serD = new SerialDisposable().D(d);

		Obs.CombineLatest(
			hotspot,
			isDragging,
			(hotspotOpt, isDragging_) => (hotspotOpt, isDragging_)
		)
			.Subscribe(t =>
			{
				serD.Disposable = null;
				if (t.isDragging_) return;
				t.hotspotOpt.IfSome(hotspot_ =>
				{
					//LR.LogThread("Hover Start_1");
					var stopFun = hotspot_.HotspotNfo.HoverAction(mouse);
					serD.Disposable = Disposable.Create(() =>
					{
						//LR.LogThread("Hover Stop_1");
						stopFun(false);
						//LR.LogThread("Hover Stop_2");
					});
					//LR.LogThread("Hover Start_2");
				});
			}).D(d);
	}
}
*/