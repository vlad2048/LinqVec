using System.Reactive.Linq;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Enums;
using LinqVec.Tools.Events;
using PowRxVar;

namespace LinqVec.Tools.Acts.Logic;

static class Act2Seq
{
	public static IObservable<ISeqEvt> ToSeq(this Act act, Evt evt) =>
		Obs.Create<ISeqEvt>(obs =>
		{
			var d = new Disp();
			act.ToEvt(evt).Subscribe(e =>
			{
				switch (e)
				{
					case OverHotEvt { MHotspot: var mh }:
						obs.OnNext(new HoverActionSeqEvt(act.GetCursor(mh), () => act.OnHover?.Invoke(mh)));
						break;
					case TriggerHotEvt { Hotspot: var hotspot }:
						obs.OnNext(new StartSeqEvt());
						obs.OnNext(new FinishSeqEvt(hotspot, () => act.OnTrigger?.Invoke(hotspot)));
						obs.OnCompleted();
						d.Dispose();
						break;
				}
			}).D(d);
			return d;
		});

	private static Option<T> ToOption<T>(this T? obj) where T : class => obj switch
	{
		null => Option<T>.None,
		not null => obj
	};

	private static Option<Cursor> GetCursor(this Act act, Option<object> mh) =>
		from _ in mh
		from cur in act.Cursor.ToOption()
		select cur;

	private static IObservable<IHotEvt> ToEvt(this Act act, Evt evt) =>
		Observable.Merge(
			evt.WhenEvt
					.WhereSelectMousePos()
					.Select(act.Hotspot)
					.DistinctUntilChanged()
					.Select(e => (IHotEvt)new OverHotEvt(e)),
			evt.WhenEvt
					.Where(e => e.ToTrigger().Equals(act.Trigger))
					.WhereSelectMousePos()
					.Select(act.Hotspot)
					.Where(e => e.IsSome)
					.Select(e => e.IfNone(() => throw new ArgumentException()))
					.Take(1)
					.Select(e => (IHotEvt)new TriggerHotEvt(e))
			);

	private static Option<Trigger> ToTrigger(this IEvt evt) =>
		evt switch
		{
			MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left } => Trigger.Down,
			MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left } => Trigger.Up,
			MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Right } => Trigger.DownRight,
			MouseClickEvt { Btn: MouseBtn.Left } => Trigger.Click,
			_ => None
		};
}