<Query Kind="Program">
  <NuGetReference>LanguageExt.Core</NuGetReference>
  <Namespace>Microsoft.Reactive.Testing</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Concurrency</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

#load "_common\rx"
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;
using Obs = System.Reactive.Linq.Observable;
using static RxTestMakers;
using H = System.String;


public static readonly TimeSpan ClickDelay = TimeSpan.FromSeconds(0.5);

/*
	Hotspot[0]
		name:	First
		loc :	0 <= Mouse < 50
		acts:	Drag+Click
		hots[0]	"A" (0 <= Mouse < 25)
		hots[1]	"B" (25 <= Mouse < 50)
	Hotspot[1]
		name:	Second
		loc :	50 <= Mouse < 100
		acts:	Click
		hots[0]	"C" (50 <= Mouse < 75)
		hots[1]	"D" (75 <= Mouse < 100)
*/
void Main()
{
	var set = new HotspotActsSet([
		new HotspotActs(
			new Hotspot("First", mouse => (mouse >= 0 && mouse < 50) switch
			{
				false => None,
				true => (mouse < 25) switch
				{
					true => Option<string>.Some("A"),
					false => Option<string>.Some("B"),
				}
			}),
			[
				new HotspotAct(Gesture.Drag),
				new HotspotAct(Gesture.Click),
			]
		),
		new HotspotActs(
			new Hotspot("Second", mouse => (mouse >= 50 && mouse < 100) switch
			{
				false => None,
				true => (mouse < 75) switch
				{
					true => Option<string>.Some("C"),
					false => Option<string>.Some("D"),
				}
			}),
			[
				new HotspotAct(Gesture.Click),
			]
		),
	]);
}



public enum EvtType
{
	Move,
	Down,
	Up,
}
public record Evt(EvtType Type, int Mouse);
public record EvtTime(Evt Evt, bool IsQuick);
public record EvtMatch(Evt Evt, bool NeedQuick) { public bool Matches(EvtTime e) => e.Evt == Evt && (e.IsQuick || !NeedQuick); }
public record Hotspot(string Name, Func<int, Option<H>> Fun);
public enum Gesture
{
	Drag,
	Click
}
public record HotspotAct(Gesture Gesture);
public record HotspotActs(Hotspot Hotspot, HotspotAct[] Acts);
public record HotspotActsSet(HotspotActs[] Set);




/*
void Main()
{
	var srcEvt = Sched.CreateHotObservable<Evt>([
		OnNext(2.0, Evt.Move),
		
		OnNext(2.7, Evt.Down),
		OnNext(3.1, Evt.Up),
		
		//OnNext(4.0, Evt.Move),
		
		OnNext(3.4, Evt.Down),
		OnNext(3.5, Evt.Up),
	]);
	
	Act[] acts = [
		new Act(Gesture.Click),
		new Act(Gesture.DoubleClick),
	];
	var actEvt = srcEvt.ToActEvt(acts[1], Sched);
	
	var obs = Sched.CreateObserver<ActEvt>();
	actEvt.Subscribe(obs);
	
	Sched.Start();
	
	obs.Messages.Dump();
}


public enum Evt
{
	Move,
	Down,
	Up,
}
public enum Gesture
{
	Drag,
	Click,
	DoubleClick,
}
public record UserAction(Evt Evt, bool IsQuick)
{
	public override string ToString() => $"UserAction({Evt} quick:{IsQuick})";
}
public record UserMatch(Evt Evt, bool NeedQuick)
{
	public override string ToString() => $"UserMatch({Evt} quick:{(NeedQuick ? "Yes" : "*")})";
	public bool IsMatch(UserAction e) => (e.Evt == Evt) switch { false => false, true => NeedQuick switch { false => true, true => e.IsQuick } };
}

public record Act(Gesture Gesture)
{
	public override string ToString() => $"Act({Gesture})";
}

public enum ActEvtType
{
	Trigger,
}
public record ActEvt(ActEvtType Type, Act Act)
{
	public override string ToString() => $"ActEvt({Type}, {Act})";
}

public enum Outcome
{
	Continue,
	Success,
	Failure
}

static class RxEvtUtils
{
	// public static IObservable<ActEvt> ToActEvt(this IObservable<Evt> evt, Act[] acts) => acts.Select(act => evt.ToActEvt(act)).Merge();
	
	public static IObservable<ActEvt> ToActEvt(this IObservable<Evt> evt, Act act, IScheduler sched)
	{
		var seq = act.Gesture.ToMatchSequence();
		
		var res =
			evt.ToUserAction(sched)
				.SkipWhile(e => !seq[0].IsMatch(e))
				.Select((e, i) => (e, i))
				.Select(t => (t.i >= seq.Length) switch
				{
					true => Outcome.Failure,
					false => seq[t.i].IsMatch(t.e) switch
					{
						false => Outcome.Failure,
						true => (t.i < seq.Length - 1) switch
						{
							true => Outcome.Continue,
							false => Outcome.Success
						}
					}
				})
				//.Do(e => $"{e}".Dump())
				.TakeUntil(e => e != Outcome.Continue)
				.Select(e => e switch
				{
					Outcome.Continue => Obs.Never<ActEvt>(),
					Outcome.Failure => Obs.Empty<ActEvt>(),
					Outcome.Success => Obs.Return(new ActEvt(ActEvtType.Trigger, act)),
					_ => throw new ArgumentException()
				})
				.Switch();

		return res;
	}

	private static IObservable<UserAction> ToUserAction(this IObservable<Evt> evt, IScheduler sched) =>
		evt
			.TimeInterval(sched)
			.Select(e => new UserAction(e.Value, e.Interval <= ClickDelay));
	
	
	private static UserMatch[] ToMatchSequence(this Gesture gesture) => gesture switch
	{
		Gesture.Click		=>	[new UserMatch(Evt.Down, false), new UserMatch(Evt.Up, true)],
		Gesture.DoubleClick	=>	[new UserMatch(Evt.Down, false), new UserMatch(Evt.Up, true), new UserMatch(Evt.Down, true), new UserMatch(Evt.Up, true)],
		_ => throw new ArgumentException()
	};
}



static object ToDump(object o) => o switch
{
	Recorded<Notification<ActEvt>> e => e.Fmt(),
	Recorded<Notification<UserAction>> e => e.Fmt(),
	Recorded<Notification<Outcome>> e => e.Fmt(),
	_ => o
};
*/
