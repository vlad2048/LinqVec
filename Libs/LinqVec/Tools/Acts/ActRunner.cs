using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Acts.Logic;
using LinqVec.Tools.Acts.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils.Rx;
using PowRxVar;

namespace LinqVec.Tools.Acts;

public static class BaseActIds
{
	public const string Empty = nameof(Empty);
}

public static class ActRunner
{
	public static IObservable<ActGfxEvt> Run<E>(
		this ActNfo[] actsInit,
		Evt evt,
		IRoDispBase d
	) where E : struct, Enum
	{
		var curActs = new BehaviorSubject<ActNfo[]>(actsInit).D();
		evt.WhenUndoRedo.Subscribe(_ => curActs.OnNext(actsInit)).D(d);
		void Reset() => curActs.OnNext(curActs.Value);
		var whenGfxEvt = new Subject<ActGfxEvt>().D(d);

		var serDisp = new SerDisp().D(d);

		curActs.Subscribe(acts =>
		{
			var serD = serDisp.GetNewD();

			var isHotLocked = false;
			var curHot = HotspotTracker.Track(evt, acts, () => isHotLocked);

			var evtD = new Disp();
			new ScheduledDisposable(Rx.Sched, evtD).D(serD);

			var actEvt = evt.ToActEvt(acts.Select(e => e.Gesture), evtD);

			curHot.SetCursor(evt.SetCursor).D(serD);
			curHot.TriggerHoverActions(whenGfxEvt.OnNext).D(serD);
			actEvt.TriggerDragAndClickActions(
				curHot,
				e => isHotLocked = e,
				Reset,
				curActs.OnNext,
				whenGfxEvt.OnNext
			).D(serD);
		}).D(d);

		return whenGfxEvt.AsObservable();
	}

}