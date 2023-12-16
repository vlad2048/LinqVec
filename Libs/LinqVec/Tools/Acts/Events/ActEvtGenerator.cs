using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Geom;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Events;
using LinqVec.Utils.Rx;
using PowBasics.CollectionsExt;
using PowRxVar;

namespace LinqVec.Tools.Acts.Events;

static class ActEvtGenerator
{
	private static readonly TimeSpan ClickDelay = TimeSpan.FromMilliseconds(500);


	private enum UserActionType
	{
		Move,
		DownL,
		UpL,
		DownR,
		UpR,
	}

	private sealed record UserAction(UserActionType Type, bool IsQuick, Pt Pos, Pt LastPos)
	{
		public static readonly UserAction Empty = new(UserActionType.Move, false, Pt.Zero, Pt.Zero);
	}
	private sealed record UserActionMatch(UserActionType Type, bool NeedQuick)
	{
		public bool IsMatch(UserAction e) => (e.Type == Type) switch
		{
			false => false,
			true => NeedQuick switch
			{
				false => true,
				true => e.IsQuick
			}
		};
	}
	private static class Usr
	{
		public static readonly UserActionMatch Move = new(UserActionType.Move, false);
		public static readonly UserActionMatch DownL = new(UserActionType.DownL, false);
		public static readonly UserActionMatch DownR = new(UserActionType.DownR, false);
		public static readonly UserActionMatch DownLQuick = new(UserActionType.DownL, true);
		public static readonly UserActionMatch UpLQuick = new(UserActionType.UpL, true);
		public static readonly UserActionMatch UpRQuick = new(UserActionType.UpR, true);
	}
	private static UserActionMatch[] ToActions(this Gesture gesture) => gesture switch
	{
		Gesture.Drag => [Usr.DownL, Usr.Move],
		Gesture.Click => [Usr.DownL, Usr.UpLQuick],
		Gesture.RightClick => [Usr.DownR, Usr.UpRQuick],
		Gesture.DoubleClick => [Usr.DownL, Usr.UpLQuick, Usr.DownLQuick, Usr.UpLQuick],
		_ => throw new ArgumentException(),
	};
	private static IActEvt ToActEvt(this Gesture gesture, Pt pos, Pt lastPos) => gesture switch
	{
		Gesture.Drag => new DragStartActEvt(lastPos),
		Gesture.Click => new ClickActEvt(pos),
		Gesture.RightClick => new RightClickActEvt(pos),
		Gesture.DoubleClick => new DoubleClickActEvt(pos),
		_ => throw new ArgumentException()
	};

	private static Option<UserAction> ToAction(this IEvt evt, bool isQuick, Pt lastPos) => evt switch
	{
		MouseMoveEvt { Pos: var pos } => new UserAction(UserActionType.Move, isQuick, pos, lastPos),
		MouseBtnEvt { Btn: MouseBtn.Left, UpDown: UpDown.Down, Pos: var pos } => new UserAction(UserActionType.DownL, isQuick, pos, lastPos),
		MouseBtnEvt { Btn: MouseBtn.Left, UpDown: UpDown.Up, Pos: var pos } => new UserAction(UserActionType.UpL, isQuick, pos, lastPos),
		MouseBtnEvt { Btn: MouseBtn.Right, UpDown: UpDown.Down, Pos: var pos } => new UserAction(UserActionType.DownR, isQuick, pos, lastPos),
		MouseBtnEvt { Btn: MouseBtn.Right, UpDown: UpDown.Up, Pos: var pos } => new UserAction(UserActionType.UpR, isQuick, pos, lastPos),
		_ => None
	};

	private sealed class Trk : IDisposable
	{
		private readonly Disp d = new();
		public void Dispose() => d.Dispose();

		private readonly IScheduler scheduler;
		private readonly Gesture gesture;
		private readonly UserActionMatch[] matches;
		private readonly bool needsTimeout;
		private readonly ISubject<Unit> whenReset;
		private readonly ISubject<IActEvt> whenTrigger;
		private int idx;
		private UserAction curAction = UserAction.Empty;
		private IObservable<Unit> WhenReset => whenReset.AsObservable();

		public Trk(IScheduler scheduler, Gesture gesture, UserActionMatch[] matches, bool needsTimeout)
		{
			this.scheduler = scheduler;
			this.gesture = gesture;
			this.matches = matches;
			this.needsTimeout = needsTimeout;
			whenReset = new Subject<Unit>().D(d);
			whenTrigger = new Subject<IActEvt>().D(d);
		}

		private void Trigger() => whenTrigger.OnNext(gesture.ToActEvt(curAction.Pos, curAction.LastPos));


		public void Reset()
		{
			idx = 0;
			whenReset.OnNext(Unit.Default);
		}

		public IObservable<IActEvt> WhenTrigger => whenTrigger.AsObservable();

		public void Process(UserAction action)
		{
			curAction = action;
			if (idx >= matches.Length)
			{
				Reset();
			}
			else
			{
				if (matches[idx].IsMatch(action))
				{
					idx++;
					if (idx == matches.Length)
					{
						if (needsTimeout)
						{
							Obs.Timer(ClickDelay, scheduler)
								.TakeUntil(WhenReset)
								.Subscribe(_ => Trigger()).D(d);
						}
						else
						{
							Trigger();
						}
					}
				}
				else
				{
					Reset();
				}
			}
		}

		public static Trk[] Build(IScheduler scheduler, Gesture gestures, IRoDispBase d)
		{
			var gests = Enum.GetValues<Gesture>().Where(e => e != Gesture.None).WhereToArray(e => gestures.HasFlag(e));
			var arrs = gests.SelectToArray(e => e.ToActions());
			var res = new Trk[arrs.Length];
			for (var i = 0; i < arrs.Length; i++)
			{
				var others = arrs.Take(i).Concat(arrs.Skip(i + 1)).ToArray();
				var isPrefix = IsPrefix(arrs[i], others);
				res[i] = new Trk(scheduler, gests[i], arrs[i], isPrefix).D(d);
			}
			return res;
		}

		private static bool IsPrefix(UserActionMatch[] arr, UserActionMatch[][] others) => others.Any(other => IsPrefix(arr, other));
		private static bool IsPrefix(UserActionMatch[] arr, UserActionMatch[] other) => arr.Length <= other.Length && arr.Zip(other.Take(arr.Length)).All(t => t.Item1 == t.Item2);
	}


	public static IObservable<IActEvt> ToActEvt(this Evt evt, Gesture gestures, IRoDispBase d) => evt.WhenEvt.ToActEvt(gestures, Rx.Sched, d);


	public static IObservable<IActEvt> ToActEvt(this IObservable<IEvt> evt, Gesture gestures, IScheduler scheduler, IRoDispBase d) =>
		Obs.Create<IActEvt>(obs =>
			{
				var obsD = new Disp();
				void Send(IActEvt evtDst) => obs.OnNext(evtDst);
				var trks = Trk.Build(scheduler, gestures, obsD);
				var lastTime = DateTimeOffset.MinValue;
				var lastDownPos = Pt.Zero;
				var isDragging = false;
				var dragStartPos = Pt.Zero;

				trks.Select(e => e.WhenTrigger).Merge().Subscribe(triggerEvt =>
				{
					Send(triggerEvt);
					if (triggerEvt is DragStartActEvt { PtStart: var dragStartPos_ })
					{
						isDragging = true;
						dragStartPos = dragStartPos_;
					}
				}).D(obsD);

				evt.Subscribe(e =>
				{
					var isQuick = scheduler.Now - lastTime <= ClickDelay;
					lastTime = scheduler.Now;
					var usrEvtOpt = e.ToAction(isQuick, lastDownPos);
					if (e is MouseBtnEvt { UpDown: UpDown.Down, Pos: var lastDownPos_ }) lastDownPos = lastDownPos_;

					if (isDragging && e is MouseBtnEvt { Btn: MouseBtn.Left, UpDown: UpDown.Up, Pos: var dragEndPos })
					{
						isDragging = false;
						Send(new DragEndActEvt(dragStartPos, dragEndPos));
					}

					usrEvtOpt.IfNone(() =>
					{
						foreach (var trk in trks)
							trk.Reset();
					});

					usrEvtOpt.IfSome(usrEvt =>
					{
						foreach (var trk in trks)
							trk.Process(usrEvt);
					});

				}).D(obsD);

				return obsD;
			})
			.MakeHot(d);



	/*
	
	private interface IState;
	private sealed record NoneState : IState;
	private sealed record WaitClickOrDragState(MouseBtn Btn, Pt Pos, DateTime Time) : IState;
	private sealed record WaitDoubleClickDownState : IState;
	private sealed record WaitDoubleClickUpState : IState;
	private sealed record WaitDragState(Pt PosStart) : IState;

	
	public static IObservable<IActEvt> ToActEvt(this IObservable<IEvt> evt, Gesture gestures, IRoDispBase d) =>
		Obs.Create<IActEvt>(obs =>
			{
				var obsD = new Disp();
				void Send(IActEvt evtDst) => obs.OnNext(evtDst);
				var state = Var.Make<IState>(new NoneState()).D(obsD);
				//var timeout = new EvtTimeout().D(obsD);

				var canDrag = gestures.HasFlag(Gesture.Drag);
				var canClick = gestures.HasFlag(Gesture.Click);
				var canRightClick = gestures.HasFlag(Gesture.RightClick);
				var canDoubleClick = gestures.HasFlag(Gesture.DoubleClick);

				evt.Subscribe(e =>
				{

					switch (state.V)
					{
						case NoneState:
							switch (e)
							{
								case MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left, Pos: var mousePos } when canDrag || canClick || canDoubleClick:
									state.V = new WaitClickOrDragState(MouseBtn.Left, mousePos, DateTime.Now);
									break;
								case MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Right, Pos: var mousePos } when canRightClick:
									state.V = new WaitClickOrDragState(MouseBtn.Right, mousePos, DateTime.Now);
									break;
								default:
									break;
							}
							break;

						// (Click -> DoubleClick) | Drag
						// =============================
						case WaitClickOrDragState { Btn: MouseBtn.Left, Pos: var statePos, Time: var stateTime }:
							switch (e)
							{
								case MouseMoveEvt { Pos: var mousePos }:
									Send(new DragStartActEvt(statePos));
									state.V = new WaitDragState(statePos);
									break;
								case MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left, Pos: var mousePos } when canClick || canDoubleClick:
									if (canClick && DateTime.Now - stateTime < ClickDelay)
									{
										Send(new ClickActEvt(mousePos));
										state.V = new NoneState();
									}
									else
									{
										state.V = new NoneState();
									}
									break;
							}
							break;


						// Right Click
						// ===========
						case WaitClickOrDragState { Btn: MouseBtn.Right, Pos: var statePos, Time: var stateTime }:
							switch (e)
							{
								case MouseMoveEvt { Pos: var mousePos }:
									state.V = new NoneState();
									break;
								case MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Right, Pos: var mousePos }:
									if (DateTime.Now - stateTime < ClickDelay)
									{
										Send(new RightClickActEvt(mousePos));
										state.V = new NoneState();
									}
									else
									{
										state.V = new NoneState();
									}
									break;
							}
							break;

						case WaitDoubleClickDownState:
							break;

						case WaitDoubleClickUpState:
							break;

						case WaitDragState { PosStart: var statePosStart }:
							switch (e)
							{
								case MouseBtnEvt { Pos: var mousePos, Btn: MouseBtn.Left, UpDown: UpDown.Up }:
									Send(new DragEndActEvt(statePosStart, mousePos));
									state.V = new NoneState();
									break;
								default:
									break;
							}
							break;
					}



				}).D(obsD);

				return obsD;
			})
			.MakeHot(d);*/
}