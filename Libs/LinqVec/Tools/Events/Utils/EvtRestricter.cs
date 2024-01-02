using Geom;
using ReactiveVars;

namespace LinqVec.Tools.Events.Utils;

public static class EvtRestricter
{
	public static IObservable<IEvt> RestrictToGrid(this IObservable<IEvt> src)
	{
		var sz = C.Grid.TickCount;
		var r = new R(new Pt(-sz, -sz), new Pt(sz, sz));
		return src.RestrictTo(r.Contains);
	}


	public static IObservable<IEvt> RestrictTo(this IObservable<IEvt> src, Func<Pt, bool> predicate) =>
		Obs.Create<IEvt>(obs =>
		{
			var d = MkD("EvtRestricter");

			bool? isIn = null;

			void Send(IEvt e) => obs.OnNext(e);

			src.Subscribe(e =>
			{
				switch (e)
				{
					case MouseMoveEvt { Pos: var pos }:

						// Enter
						if (predicate(pos) && isIn != true)
						{
							Send(new MouseEnterEvt());
							Send(e);
							isIn = true;
						}

						// Leave
						else if (!predicate(pos) && isIn != false)
						{
							Send(new MouseLeaveEvt());
							Send(e);
							isIn = false;
						}

						// Move when in
						else if (predicate(pos) && isIn == true)
						{
							Send(e);
						}

						break;

					case MouseEnterEvt:
						break;

					case MouseLeaveEvt when isIn != false:
						Send(e);
						isIn = false;
						break;

					default:
						Send(e);
						break;
				}
			}).D(d);

			return d;
		});
}