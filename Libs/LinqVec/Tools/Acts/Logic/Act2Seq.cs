using System.Reactive.Linq;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Enums;
using LinqVec.Tools.Events;
using PowMaybe;
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

	private static Maybe<Cursor> GetCursor(this Act act, Maybe<object> mh) =>
		from _ in mh
		from cur in act.Cursor.ToMaybe()
		select cur;

	private static IObservable<IHotEvt> ToEvt(this Act act, Evt evt) =>
		Observable.Merge(
			evt.WhenEvt
					.Select(e => e.GetMayMousePos())
					.WhenSome()
					.Select(act.Hotspot)
					.DistinctUntilChanged()
					.Select(e => (IHotEvt)new OverHotEvt(e)),
			evt.WhenEvt
					.Where(e => e.ToTrigger().IsSomeAndEqualTo(act.Trigger))
					.Select(e => e.GetMayMousePos())
					.WhenSome()
					.Select(act.Hotspot)
					.WhenSome()
					.Take(1)
					.Select(e => (IHotEvt)new TriggerHotEvt(e))
			);

	private static Maybe<Trigger> ToTrigger(this IEvtGen<Pt> evt) =>
		evt switch
		{
			MouseBtnEvtGen<Pt> { UpDown: UpDown.Down, Btn: MouseBtn.Left } => May.Some(Trigger.Down),
			MouseBtnEvtGen<Pt> { UpDown: UpDown.Up, Btn: MouseBtn.Left } => May.Some(Trigger.Up),
			MouseBtnEvtGen<Pt> { UpDown: UpDown.Down, Btn: MouseBtn.Right } => May.Some(Trigger.DownRight),
			MouseClickEvtGen<Pt> { Btn: MouseBtn.Left } => May.Some(Trigger.Click),
			_ => May.None<Trigger>()
		};
}