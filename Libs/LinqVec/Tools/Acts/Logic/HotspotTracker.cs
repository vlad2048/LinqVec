using LinqVec.Tools.Acts.Delegates;
using LinqVec.Tools.Acts.Structs;
using LinqVec.Tools.Events;
using PowRxVar;

namespace LinqVec.Tools.Acts.Logic;

static class HotspotTracker
{
	public static IDisposable Track(
		IRwVar<Option<HotAct>> curHot,
		Evt evt,
		ActSet actSet,
		Func<bool> isLocked
	)
	{
		var d = MkD();
		evt.MousePos
			.Subscribe(mouseOpt => mouseOpt.Match(
				mouse =>
				{
					foreach (var act in actSet.Acts)
					{
						var mayH = act.Hotspot.Fun(mouse);

						var ret = false;
						mayH.IfSome(h =>
						{
							var isAlreadySet = curHot.V.Map(e => e.Act) == act;
							if (!isAlreadySet && !isLocked())
								curHot.V = new HotAct(h, act, mouse);
							ret = true;
						});
						if (ret)
							return;
					}
					if (!isLocked())
						curHot.V = None;
				},
				() =>
				{
					if (!isLocked())
						curHot.V = None;
				})).D(d);

		return d;
	}



	/*public static IObservable<Option<HotAct>> Track(
		Evt evt,
		ActSet actSet,
		Func<bool> isLocked
	) =>
		Obs.Create<Option<HotAct>>(obs =>
		{
			var d = MkD();

			var curHot = Option<HotAct>.None;

			evt.MousePos
				.Subscribe(mouseOpt => mouseOpt.Match(
					mouse =>
					{
						foreach (var act in actSet.Acts)
						{
							var mayH = act.Hotspot.Fun(mouse);

							var ret = false;
							mayH.IfSome(h =>
							{
								var isAlreadySet = curHot.Map(e => e.Act) == act;
								if (!isAlreadySet && !isLocked())
									obs.OnNext(new HotAct(h, act, mouse));
								ret = true;
							});
							if (ret)
								return;
						}
						if (!isLocked())
							obs.OnNext(None);
					},
					() =>
					{
						if (!isLocked())
							obs.OnNext(None);
					})).D(d);


			return d;
		});*/
}
