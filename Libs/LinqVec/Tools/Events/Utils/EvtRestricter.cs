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


	private static IObservable<IEvt> RestrictTo(this IObservable<IEvt> src, Func<Pt, bool> predicate) =>
		Obs.Create<IEvt>(obs =>
		{
			var d = MkD("EvtRestricter");

			bool? isIn = null;

			void Send(IEvt e) => obs.OnNext(e);

			src.Subscribe(e =>
			{
				switch (e)
				{
					// Handle Entering / Leaving the area
					// ==================================
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
					case MouseLeaveEvt:
						break;



					// For Right, Middle and Clicks (any buttons) only let the event through while in the zone
					// =======================================================================================
					case MouseBtnEvt { Btn: not MouseBtn.Left } or MouseClickEvt:
						if (isIn == true)
							Send(e);
						break;

					// For the Left button only
					// ========================
					case MouseBtnEvt { Btn: MouseBtn.Left, UpDown: UpDown.Down }:
						if (isIn == true)
							Send(e);
						break;

					case MouseBtnEvt { Btn: MouseBtn.Left, UpDown: UpDown.Up }:
						if (isIn == true)
							Send(e);
						else
							Send(new MouseLeftBtnUpOutside());
						break;




					// Always let MouseWheel through
					// =============================
					case MouseWheelEvt:
						Send(e);
						break;

					// Always let Keys through
					// =======================
					case KeyEvt:
						Send(e);
						break;




					default:
						throw new ArgumentException();
				}
			}).D(d);

			return d;
		});
}