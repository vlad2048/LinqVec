using System.Reactive.Linq;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Acts.Structs;

namespace LinqVec.Tools.Acts.Logic;

static class ActionTriggerer
{
	public static IDisposable TriggerHoverActions(
		this IObservable<Option<HotAct>> curHot,
		Action<ActGfxEvt> sigGfxEvt
	) =>
		curHot
			.StartWith(None)
			.Buffer(2, 1)
			.Select(l => new
			{
				Prev = l[0],
				Next = l[1],
			})
			.Subscribe(t =>
			{
				t.Prev.IfSome(tPrev => tPrev.Act.Actions.HoverOff());
				t.Next.IfSome(tNext =>
				{
					tNext.Act.Actions.HoverOn(tNext.Hot, tNext.MousePos);
					sigGfxEvt(new ActGfxEvt(tNext.Act.Id, ActGfxState.Hover));
				});
				t.Next.IfNone(() =>
				{
					sigGfxEvt(new ActGfxEvt(BaseActIds.Empty, ActGfxState.Hover));
				});
			});

	public static IDisposable TriggerDragAndClickActions(
		this IObservable<IActEvt> actEvt,
		IObservable<Option<HotAct>> curHot,
		Action<bool> setIsHotLocked,
		Action reset,
		Action<ActNfo[]> setActs,
		Action<ActGfxEvt> sigGfxEvt
	) =>
		actEvt
			.WithLatestFrom(
				curHot,
				(evt, hot) => new
				{
					Evt = evt,
					Hot = hot
				}
			)
			.Where(t => t.Hot.IsSome)
			.Select(t => new
			{
				t.Evt,
				Hot = t.Hot.IfNone(() => throw new ArgumentException())
			})
			.Subscribe(t =>
			{
				var hot = t.Hot.Hot;
				var actions = t.Hot.Act.Actions;
				switch (t.Evt)
				{
					case DragStartActEvt { PtStart: var ptStart }:
						setIsHotLocked(true);
						actions.DragStart(hot, ptStart);
						sigGfxEvt(new ActGfxEvt(t.Hot.Act.Id, ActGfxState.DragStart));
						break;

					case ConfirmActEvt { Type: var type, PtStart: var ptStart, PtEnd: var ptEnd }:
						setIsHotLocked(false);
						actions.Confirm(hot, ptEnd);
						if (actions.ConfirmActs != null)
						{
							var acts = actions.ConfirmActs(hot);
							setActs(acts);
						}
						sigGfxEvt(new ActGfxEvt(t.Hot.Act.Id, ActGfxState.Confirm));
						reset();
						break;

					case KeyDownActEvt { Key: var key }:
						break;
				}
			});
}