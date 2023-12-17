using LinqVec.Tools.Acts.Structs;
using LinqVec.Tools.Events;
using PowRxVar;

namespace LinqVec.Tools.Acts.Logic;

static class HotspotTracker
{
	public static IObservable<Option<HotAct>> Track(
		Evt evt,
		ActNfo[] acts,
		Func<bool> isLocked
	) =>
		Obs.Create<Option<HotAct>>(obs =>
		{
			var d = new Disp();

			var curHot = Option<HotAct>.None;

			evt.MousePos
				.Subscribe(mouseOpt => mouseOpt.Match(
					mouse =>
					{
						foreach (var act in acts)
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
		});
}
