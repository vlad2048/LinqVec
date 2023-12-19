using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Acts.Enums;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using ReactiveVars;

namespace LinqVec.Tools.Acts.Logic;


interface IActEvt
{
	HotspotAct HotspotAct { get; }
}
sealed record DragStartActEvt(HotspotAct HotspotAct, Pt PtStart) : IActEvt
{
	public override string ToString() => $"[{HotspotAct.Name}].DragStart({PtStart})";
}
sealed record ConfirmActEvt(HotspotAct HotspotAct, Pt Pt) : IActEvt
{
	public override string ToString() => $"[{HotspotAct.Name}].Confirm({Pt})";
}


static class ActEvtGenerator
{
	public static readonly TimeSpan ClickDelay = TimeSpan.FromMilliseconds(500);


	public static IObservable<IActEvt> ToActEvt(
		this IRoVar<HotspotActsRun> curHotActs,
		IObservable<IEvt> evt,
		IScheduler scheduler,
		Disp d
	)
	{
		var usrEvt = evt
			.TimeInterval(scheduler)
			.Select(e => e.Value.ToUsr(e.Interval <= ClickDelay))
			.WhereSome();
		return curHotActs
			.Select(e => e.ToActEvt(usrEvt, scheduler, d))
			.Switch()
			.MakeHot(d);
	}


	private static IObservable<IActEvt> ToActEvt(
		this HotspotActsRun acts,
		IObservable<IUsr> evt,
		IScheduler scheduler,
		Disp d
	)
	{
		if (acts.Acts.Select(e => e.Gesture).Distinct().Count() != acts.Acts.Length) throw new ArgumentException("Gestures should be unique for a Hotspot");
		var hasBothSingleAndDoubleClicks = acts.Acts.Any(e => e.Gesture == Gesture.Click) && acts.Acts.Any(e => e.Gesture == Gesture.DoubleClick);

		var whenDragStart =
			acts.Acts
				.FirstOrOption(e => e.Gesture == Gesture.Drag)
				.Map(act =>
					evt
						.SpotMatches(seqDrag)
						.Select(e => (IActEvt)new DragStartActEvt(act, ((LDownUsr)e).Pt))
				)
				.IfNone(Obs.Never<IActEvt>);

		var whenDragEnd =
			whenDragStart
				.Select(_ =>
					evt
						.Where(e => e is LUpUsr)
						.Select(e => (IActEvt)new ConfirmActEvt(acts.Acts.Single(f => f.Gesture == Gesture.Drag), ((LUpUsr)e).Pt))
						.Take(1)
				)
				.Switch();

		var isDragging =
			Obs.Merge(
					whenDragStart.Select(_ => true),
					whenDragEnd.Select(_ => false)
				)
				.Prepend(false)
				.ToVar(d);

		var whenRightClick =
			acts.Acts
				.FirstOrOption(e => e.Gesture == Gesture.RightClick)
				.Map(act =>
					evt
						.Where(_ => !isDragging.V)
						.SpotMatches(seqRightClick)
						.Select(e => (IActEvt)new ConfirmActEvt(act, ((RDownUsr)e).Pt))
				)
				.IfNone(Obs.Never<IActEvt>);

		var whenDoubleClick =
			acts.Acts
				.FirstOrOption(e => e.Gesture == Gesture.DoubleClick)
				.Map(act =>
					evt
						.Where(_ => !isDragging.V)
						.SpotMatches(seqDoubleClick)
						.Select(e => (IActEvt)new ConfirmActEvt(act, ((LDownUsr)e).Pt))
				)
				.IfNone(Obs.Never<IActEvt>)
				.MakeHot(d);

		var whenDoubleClickDelayed = whenDoubleClick.Delay(TimeSpan.Zero, scheduler).MakeHot(d);

		var whenClick =
			acts.Acts
				.FirstOrOption(e => e.Gesture == Gesture.Click)
				.Map(act =>
					hasBothSingleAndDoubleClicks switch
					{
						false =>
							evt
								.Where(_ => !isDragging.V)
								.SpotMatches(seqClick)
								.Select(e => (IActEvt)new ConfirmActEvt(act, ((LDownUsr)e).Pt)),
						true =>
							evt
								.Where(_ => !isDragging.V)
								.SpotMatches(seqClick)
								.IfOtherDoesntHappenWithin(
									Obs.Merge(
										evt.Where(e => e is LDownUsr).ToUnit(),
										whenDoubleClickDelayed.ToUnit()
									)
								, ClickDelay, scheduler)
								.Select(e => (IActEvt)new ConfirmActEvt(act, ((LDownUsr)e).Pt))
					}
				)
				.IfNone(Obs.Never<IActEvt>);


		//void Log(string s) => L.WriteLine($"[{scheduler.Now:HH:mm:ss.f}] {s}");

		return Obs.Merge(
			whenDragStart,
			whenDragEnd,
			whenRightClick,
			whenDoubleClick,
			whenClick
		);
	}



	// *************
	// * Sequences *
	// *************
	private static IObservable<IUsr> SpotMatches(this IObservable<IUsr> evt, Match[] seq) => evt.SpotSequenceReturnFirst(seq, (m, e) => m.Matches(e));


	private static readonly Match[] seqDrag = [
		new Match(MatchType.LDown, false),
		new Match(MatchType.Move, false),
	];
	private static readonly Match[] seqClick = [
		new Match(MatchType.LDown, false),
		new Match(MatchType.LUp, true),
	];
	private static readonly Match[] seqRightClick = [
		new Match(MatchType.RDown, false),
		new Match(MatchType.RUp, true),
	];
	private static readonly Match[] seqDoubleClick = [
		new Match(MatchType.LDown, false),
		new Match(MatchType.LUp, true),
		new Match(MatchType.LDown, true),
		new Match(MatchType.LUp, true),
	];



	// ****************
	// * IUsr & Match *
	// ****************
	private interface IUsr
	{
		bool IsQuick { get; }
	}
	private sealed record MoveUsr(bool IsQuick, Pt Pt) : IUsr
	{
		public override string ToString() => $"Move({Pt}) IsQuick:{IsQuick}";
	}
	private sealed record LDownUsr(bool IsQuick, Pt Pt) : IUsr
	{
		public override string ToString() => $"LDown({Pt}) IsQuick:{IsQuick}";
	}
	private sealed record LUpUsr(bool IsQuick, Pt Pt) : IUsr
	{
		public override string ToString() => $"LUp({Pt}) IsQuick:{IsQuick}";
	}
	private sealed record RDownUsr(bool IsQuick, Pt Pt) : IUsr
	{
		public override string ToString() => $"RDown({Pt}) IsQuick:{IsQuick}";
	}
	private sealed record RUpUsr(bool IsQuick, Pt Pt) : IUsr
	{
		public override string ToString() => $"RUp({Pt}) IsQuick:{IsQuick}";
	}
	private static Option<IUsr> ToUsr(this IEvt e, bool isQuick) => e switch
	{
		MouseMoveEvt { Pos: var pos } => new MoveUsr(isQuick, pos),
		MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left, Pos: var pos } => new LDownUsr(isQuick, pos),
		MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left, Pos: var pos } => new LUpUsr(isQuick, pos),
		MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Right, Pos: var pos } => new RDownUsr(isQuick, pos),
		MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Right, Pos: var pos } => new RUpUsr(isQuick, pos),
		_ => None
	};


	private enum MatchType
	{
		Move,
		LDown,
		LUp,
		RDown,
		RUp,
	}

	private sealed record Match(MatchType Type, bool NeedQuick)
	{
		public override string ToString() => $"Match({Type}) NeedQuick:{NeedQuick}";
		public bool Matches(IUsr e) => (e.IsQuick || !NeedQuick) switch
		{
			false => false,
			true => (Type, e) switch
			{
				(MatchType.Move, MoveUsr) => true,
				(MatchType.LDown, LDownUsr) => true,
				(MatchType.LUp, LUpUsr) => true,
				(MatchType.RDown, RDownUsr) => true,
				(MatchType.RUp, RUpUsr) => true,
				_ => false
			}
		};
	}
}