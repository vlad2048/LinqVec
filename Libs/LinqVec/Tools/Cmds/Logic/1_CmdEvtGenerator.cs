using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Geom;
using LinqVec.Logging;
using LinqVec.Tools.Cmds.Enums;
using LinqVec.Tools.Cmds.Events;
using LinqVec.Tools.Cmds.Structs;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.Rx;
using LogLib;
using LogLib.Interfaces;
using LogLib.Structs;
using ReactiveVars;

namespace LinqVec.Tools.Cmds.Logic;



interface IUsr : IWriteSer
{
	bool IsQuick { get; }
}
sealed record MoveUsr(bool IsQuick, Pt Pt) : IUsr
{
	public override string ToString() => $"Move({Pt}) IsQuick:{IsQuick}";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}
sealed record LDownUsr(bool IsQuick, Pt Pt, ModKeyState ModKey) : IUsr
{
	public override string ToString() => $"LDown({Pt}, {ModKey}) IsQuick:{IsQuick}";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}
sealed record LUpUsr(bool IsQuick, Pt Pt, ModKeyState ModKey) : IUsr
{
	public override string ToString() => $"LUp({Pt}, {ModKey}) IsQuick:{IsQuick}";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}
sealed record RDownUsr(bool IsQuick, Pt Pt) : IUsr
{
	public override string ToString() => $"RDown({Pt}) IsQuick:{IsQuick}";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}
sealed record RUpUsr(bool IsQuick, Pt Pt) : IUsr
{
	public override string ToString() => $"RUp({Pt}) IsQuick:{IsQuick}";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}
sealed record KeyDownUsr(bool IsQuick, Keys Key) : IUsr
{
	public override string ToString() => $"KeyDown({Key}) IsQuick:{IsQuick}";
	public ITxtWriter Write(ITxtWriter w) => this.Color(w);
}










static class CmdEvtGenerator
{
	public static readonly TimeSpan ClickDelay = TimeSpan.FromMilliseconds(500);


	public static IObservable<ICmdEvt> ToCmdEvt(
		this IRoVar<Option<Hotspot>> hotspot,
		IRoVar<ToolState> state,
		IObservable<IEvt> evt,
		IScheduler scheduler,
		Disp d
	)
	{
		var usrEvt = evt
			.TimeInterval(scheduler)
			.Select(e => e.Value.ToUsr(e.Interval <= ClickDelay))
			.WhereSome()
			.MakeHot(d);

		LogCategories.Setup_UsrEvt_Logging(usrEvt, scheduler, d);

		return hotspot
			.SwitchOption_NeverIfNone(hotspot_ =>
				hotspot_.ToCmdEvt(usrEvt, state.V.Shortcuts, scheduler, d))
			.MakeHot(d);
	}


	internal static IObservable<ICmdEvt> ToCmdEvt(
		this Hotspot hotspot,
		IObservable<IUsr> usrEvt,
		ShortcutNfo[] shortcuts,
		IScheduler scheduler,
		Disp d
	)
	{
		/*var whenDragStart =
			hotspot.Cmds
				.FirstOrOption(e => e.Gesture == Gesture.Drag)
				.Map(cmd => usrEvt
					.SpotMatches(seqDrag)
					.Select(e => (ICmdEvt)new DragStartCmdEvt(cmd, ((LDownUsr)e).Pt)))
				.IfNone(Obs.Never<ICmdEvt>)
				.Lg("DragStart")
				.MakeHot(d);*/

		/*var whenDragEnd =
			whenDragStart
				.Select(_ =>
					usrEvt
						.Lg("  DragEnd - 1")
						.Where(e => e is LUpUsr)
						.Lg("  DragEnd - 2")
						.Select(e => (ICmdEvt)new ConfirmCmdEvt(hotspot.Cmds.Single(f => f.Gesture == Gesture.Drag), ((LUpUsr)e).Pt))
						.Lg("  DragEnd - 3")
						.Take(1)
						.Lg("  DragEnd - 4")
				)
				.Switch()
				.Lg("  DragEnd - Switch")
				.MakeHot(d);

		return Obs.Merge(
			whenDragStart,
			whenDragEnd
		);*/

		if (hotspot.Cmds.Select(e => e.Gesture).Distinct().Count() != hotspot.Cmds.Length) throw new ArgumentException("Gestures should be unique for a Hotspot");
		var hasBothSingleAndDoubleClicks = hotspot.Cmds.Any(e => e.Gesture == Gesture.Click) && hotspot.Cmds.Any(e => e.Gesture == Gesture.DoubleClick);

		var whenDragStart =
			hotspot.Cmds
				.FirstOrOption(e => e.Gesture == Gesture.Drag)
				.Map(cmd => usrEvt
					.SpotMatches(seqDrag)
					.Select(e => new DragStartCmdEvt(cmd, ((LDownUsr)e).Pt)))
				.IfNone(Obs.Never<DragStartCmdEvt>)
				//.Lg("DragStart")
				.MakeHot(d);

		var whenDragEnd =
			whenDragStart
				.Select(whenDragStart_ =>
					usrEvt
						//.Lg("  DragEnd - 1")
						.Where(e => e is LUpUsr)
						//.Lg("  DragEnd - 2")
						.Select(e => new DragFinishCmdEvt(hotspot.Cmds.Single(f => f.Gesture == Gesture.Drag), whenDragStart_.PtStart, ((LUpUsr)e).Pt))
						//.Lg("  DragEnd - 3")
						.Take(1)
						//.Lg("  DragEnd - 4")
				)
				.Switch()
				//.Lg("  DragEnd - Switch")
				.MakeHot(d);

		var isDragging =
			Obs.Merge(
					whenDragStart.Select(_ => true),
					whenDragEnd.Select(_ => false)
				)
				.Prepend(false)
				.ToVar(d);

		//isDragging.TimePrefix(scheduler).Log(d, "isDragging");

		var whenRightClick =
			hotspot.Cmds
				.FirstOrOption(e => e.Gesture == Gesture.RightClick)
				.Map(cmd =>
					usrEvt
						.Where(_ => !isDragging.V)
						.SpotMatches(seqRightClick)
						.Select(e => new ConfirmCmdEvt(cmd, ((RDownUsr)e).Pt))
				)
				.IfNone(Obs.Never<ConfirmCmdEvt>)
				.MakeHot(d);

		var whenDoubleClick =
			hotspot.Cmds
				.FirstOrOption(e => e.Gesture == Gesture.DoubleClick)
				.Map(cmd =>
					usrEvt
						.Where(_ => !isDragging.V)
						.SpotMatches(seqDoubleClick)
						.Select(e => new ConfirmCmdEvt(cmd, ((LDownUsr)e).Pt))
				)
				.IfNone(Obs.Never<ConfirmCmdEvt>)
				.MakeHot(d);

		var whenDoubleClickDelayed = whenDoubleClick.Delay(TimeSpan.Zero, scheduler)
			.MakeHot(d);

		var whenClick =
			hotspot.Cmds
				.FirstOrOption(e => e.Gesture == Gesture.Click)
				.Map(cmd =>
					hasBothSingleAndDoubleClicks switch
					{
						false =>
							usrEvt
								.Where(_ => !isDragging.V)
								.SpotMatches(seqClick)
								.Select(e => new ConfirmCmdEvt(cmd, ((LDownUsr)e).Pt)),
						true =>
							usrEvt
								.Where(_ => !isDragging.V)
								.SpotMatches(seqClick)
								.IfOtherDoesntHappenWithin(
									Obs.Merge(
										usrEvt.Where(e => e is LDownUsr).ToUnit(),
										whenDoubleClickDelayed.ToUnit()
									)
								, ClickDelay, scheduler)
								.Select(e => new ConfirmCmdEvt(cmd, ((LDownUsr)e).Pt))
					}
				)
				.IfNone(Obs.Never<ConfirmCmdEvt>)
				.MakeHot(d);

		var whenShiftClick =
			hotspot.Cmds
				.FirstOrOption(e => e.Gesture == Gesture.ShiftClick)
				.Map(cmd =>
					usrEvt
						.Where(_ => !isDragging.V)
						.SpotMatches(seqShiftClick)
						.Select(e => new ConfirmCmdEvt(cmd, ((LDownUsr)e).Pt))
				)
				.IfNone(Obs.Never<ConfirmCmdEvt>)
				.MakeHot(d);


		//void Log(string s) => L.WriteLine($"[{scheduler.Now:HH:mm:ss.f}] {s}");

		var whenShortcut =
			shortcuts
				.Select(shortcut =>
					usrEvt
						.OfType<KeyDownUsr>()
						.Where(e => e.Key == shortcut.Key)
						.Select(_ => new ShortcutCmdEvt(shortcut))
				)
				.Merge()
				.MakeHot(d);

		var whenCancel =
			usrEvt.OfType<KeyDownUsr>()
				.Where(e => e.Key == Keys.Escape)
				.Select(_ => new CancelCmdEvt())
				.MakeHot(d);

		whenDragEnd
			.OfType<DragFinishCmdEvt>();
			//.Subscribe(e => L.WriteLine($"***** 1 *****: {e}")).D(d);

		return Obs.Merge<ICmdEvt>(
			whenDragStart,
			whenDragEnd,
			whenRightClick,
			whenDoubleClick,
			whenClick,
			whenShiftClick,
			whenShortcut,
			whenCancel
		);
	}

	private static IObservable<T> Lg<T>(this IObservable<T> source, string name) => source.Do(e =>
	{
		var nameStr = $"[{name}]".PadRight(32);
		L.WriteLine($"{nameStr}({e})");
	});

	// *************
	// * Sequences *
	// *************
	private static IObservable<IUsr> SpotMatches(this IObservable<IUsr> evt, Match[] seq) => evt.SpotSequenceReturnFirst(seq, (m, e) => m.Matches(e));


	private static readonly Match[] seqDrag = [
		new Match(MatchType.LDown, ModKeysMatch.Empty,  false),
		new Match(MatchType.Move, ModKeysMatch.Empty, false),
	];
	private static readonly Match[] seqClick = [
		new Match(MatchType.LDown, ModKeysMatch.ShiftNo, false),
		new Match(MatchType.LUp, ModKeysMatch.ShiftNo, true),
	];
	private static readonly Match[] seqShiftClick = [
		new Match(MatchType.LDown, ModKeysMatch.ShiftYes, false),
		new Match(MatchType.LUp, ModKeysMatch.ShiftYes, true),
	];
	private static readonly Match[] seqRightClick = [
		new Match(MatchType.RDown, ModKeysMatch.Empty, false),
		new Match(MatchType.RUp, ModKeysMatch.Empty, true),
	];
	private static readonly Match[] seqDoubleClick = [
		new Match(MatchType.LDown, ModKeysMatch.Empty, false),
		new Match(MatchType.LUp, ModKeysMatch.Empty, true),
		new Match(MatchType.LDown, ModKeysMatch.Empty, true),
		new Match(MatchType.LUp, ModKeysMatch.Empty, true),
	];



	// ****************
	// * IUsr & Match *
	// ****************
	private static Option<IUsr> ToUsr(this IEvt e, bool isQuick) => e switch
	{
		MouseMoveEvt { Pos: var pos } => new MoveUsr(isQuick, pos),
		MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Left, Pos: var pos, ModKey: var modKey } => new LDownUsr(isQuick, pos, modKey),
		MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Left, Pos: var pos, ModKey: var modKey } => new LUpUsr(isQuick, pos, modKey),
		MouseBtnEvt { UpDown: UpDown.Down, Btn: MouseBtn.Right, Pos: var pos } => new RDownUsr(isQuick, pos),
		MouseBtnEvt { UpDown: UpDown.Up, Btn: MouseBtn.Right, Pos: var pos } => new RUpUsr(isQuick, pos),
		KeyEvt { UpDown: UpDown.Down, Key: var key } => new KeyDownUsr(isQuick, key),
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

	private sealed class ModKeysMatch
	{
		public bool? Shift { get; }
		public bool? Alt { get; }
		public bool? Ctrl { get; }
		public ModKeysMatch(bool? shift, bool? alt, bool? ctrl)
		{
			Shift = shift;
			Alt = alt;
			Ctrl = ctrl;
		}

		public bool Matches(ModKeyState state) =>
			(!Shift.HasValue || Shift == state.Shift) &&
			(!Alt.HasValue || Alt == state.Alt) &&
			(!Ctrl.HasValue || Ctrl == state.Ctrl);

		public static readonly ModKeysMatch Empty = new(null, null, null);
		public static readonly ModKeysMatch ShiftYes = new(true, null, null);
		public static readonly ModKeysMatch ShiftNo = new(false, null, null);
	}

	private sealed record Match(MatchType Type, ModKeysMatch ModKey, bool NeedQuick)
	{
		public override string ToString() => $"Match({Type}) NeedQuick:{NeedQuick}";
		public bool Matches(IUsr e) => (e.IsQuick || !NeedQuick) switch
		{
			false => false,
			true => (Type, e) switch
			{
				(MatchType.Move, MoveUsr) => true,
				(MatchType.LDown, LDownUsr) => ModKey.Matches(((LDownUsr)e).ModKey),
				(MatchType.LUp, LUpUsr) => ModKey.Matches(((LUpUsr)e).ModKey),
				(MatchType.RDown, RDownUsr) => true,
				(MatchType.RUp, RUpUsr) => true,
				_ => false
			}
		};
	}
}