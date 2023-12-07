using System.Reactive;
using LinqVec.Tools.Enums;
using LinqVec.Tools.Events;
using PowMaybe;
using PowRxVar;
using System.Reactive.Linq;

namespace LinqVec.Tools;


public sealed record Hotspot<H>(
	Func<Pt, Maybe<H>> Get,
	Cursor? Cursor
);

public interface IAct
{
	bool IsOver(Pt mousePos);
}

public sealed record Act<H>(
	Evt Evt,
	Trigger Trigger,
	Hotspot<H> Hotspot,
	Action<bool>? OnHover,
	Action<H>? OnTrigger
) : IAct
{
	public bool IsOver(Pt mousePos) => Hotspot.Get(mousePos).IsSome();
}





public interface ISeqEvt<H>;
public sealed record HoverActionSeqEvt<H>(bool On, Action<bool> Action) : ISeqEvt<H>;
public sealed record TriggerActionSeqEvt<H>(Action Action) : ISeqEvt<H>;
public sealed record StartSeqEvt<H> : ISeqEvt<H>;
public sealed record FinishSeqEvt<H>(H Hotspot, Action Action) : ISeqEvt<H>;



public static class Act
{
	public static Act<H> Exclude<H>(this Act<H> act, IAct exclude) =>
		act with
		{
			Hotspot = act.Hotspot with
			{
				Get = mousePos =>
				{
					if (exclude.IsOver(mousePos)) return May.None<H>();
					return act.Hotspot.Get(mousePos);
				}
			}
		};

	public static IObservable<ISeqEvt<H>> ToSeq<H>(this Act<H> act) =>
		Obs.Create<ISeqEvt<H>>(obs =>
		{
			var d = new Disp();
			act.ToEvt().Subscribe(e =>
			{
				switch (e)
				{
					case OverHotEvt<H> { On: var on }:
						obs.OnNext(new HoverActionSeqEvt<H>(on, on_ => act.OnHover?.Invoke(on_)));
						break;
					case TriggerHotEvt<H> { Hotspot: var hotspot }:
						obs.OnNext(new StartSeqEvt<H>());
						obs.OnNext(new FinishSeqEvt<H>(hotspot, () => act.OnTrigger?.Invoke(hotspot)));
						obs.OnCompleted();
						d.Dispose();
						break;
				}
			}).D(d);
			return d;
		});


	public static IObservable<ISeqEvt<HOut>> Seq<HIn, HOut>(IObservable<ISeqEvt<HIn>> seqIn, Func<HIn, IObservable<ISeqEvt<HOut>>> seqOutFun) =>
		seqIn
			.Select(e => e switch
			{
				HoverActionSeqEvt<HIn> { On: var on, Action: var action } => Obs.Return<ISeqEvt<HOut>>(new HoverActionSeqEvt<HOut>(on, action)),
				TriggerActionSeqEvt<HIn> { Action: var action } => Obs.Return<ISeqEvt<HOut>>(new TriggerActionSeqEvt<HOut>(action)),
				StartSeqEvt<HIn> => Obs.Return<ISeqEvt<HOut>>(new StartSeqEvt<HOut>()),
				FinishSeqEvt<HIn> { Hotspot: var hotspot, Action: var action } =>
					Obs.Return<ISeqEvt<HOut>>(new TriggerActionSeqEvt<HOut>(action)).Concat(
						seqOutFun(hotspot)
							.Where(f => f is not StartSeqEvt<HOut>)
					),
				_ => throw new ArgumentException()
			})
			.Switch();


	public static IObservable<ISeqEvt<Unit>> Loop(IObservable<ISeqEvt<Unit>> seq) => Seq(seq, _ => Loop(seq));


	public static IObservable<ISeqEvt<H>> Amb<H>(IObservable<ISeqEvt<H>> seq0, IObservable<ISeqEvt<H>> seq1) =>
		Obs.Create<ISeqEvt<H>>(obs =>
		{

			var d = new Disp();

			int? seqIdx = null;
			var objSync = new object();

			/*Obs.Merge(
				seq0.OfType<HoverActionSeqEvt<H>>().Select(e => (0, e.On)),
				seq1.OfType<HoverActionSeqEvt<H>>().Select(e => (1, e.On))
			)
				.Buffer(TimeSpan.FromMilliseconds(10))
				.Where(indices => indices.Any())
				.Subscribe(indices =>
				{
					L.WriteLine($"Indices: [{indices.Select(e => $"[{e.Item1}:{e.On}]").JoinText(",")}]");
				}).D(d);*/

			seq0
				.Where(_ => seqIdx == null || seqIdx == 0)
				.Synchronize(objSync)
				.Subscribe(e =>
				{
					switch (e)
					{
						case HoverActionSeqEvt<H> { Action: var action }:
							obs.OnNext(e);
							break;
						case TriggerActionSeqEvt<H> { Action: var action }:
							obs.OnNext(e);
							break;
						case StartSeqEvt<H>:
							seqIdx = 0;
							obs.OnNext(e);
							break;
						case FinishSeqEvt<H> { Hotspot: var hotspot, Action: var action }:
							obs.OnNext(e);
							break;
					}
				}).D(d);

			seq1
				.Where(_ => seqIdx == null || seqIdx == 1)
				.Synchronize(objSync)
				.Subscribe(e =>
				{
					switch (e)
					{
						case HoverActionSeqEvt<H> { Action: var action }:
							obs.OnNext(e);
							break;
						case TriggerActionSeqEvt<H> { Action: var action }:
							obs.OnNext(e);
							break;
						case StartSeqEvt<H>:
							seqIdx = 1;
							obs.OnNext(e);
							break;
						case FinishSeqEvt<H> { Hotspot: var hotspot, Action: var action }:
							obs.OnNext(e);
							break;
					}
				}).D(d);

			return d;
		});


	public static IDisposable Run<H>(this IObservable<ISeqEvt<H>> seq, Evt evt)
	{
		var d = new Disp();
		seq
			.Subscribe(e =>
			{
				switch (e)
				{
					case HoverActionSeqEvt<H> { On: var on, Action: var action }:
						action(on);
						break;
					case TriggerActionSeqEvt<H> { Action: var action }:
						action();
						break;
					case StartSeqEvt<H>:
						break;
					case FinishSeqEvt<H> { Hotspot: var hotspot, Action: var action }:
						action();
						break;
				}
			}).D(d);
		return d;
	}
}





public interface IHotEvt<H>;
public sealed record OverHotEvt<H>(bool On) : IHotEvt<H>;
public sealed record TriggerHotEvt<H>(H Hotspot) : IHotEvt<H>;

public static class HotspotExt
{
	public static IObservable<IHotEvt<H>> ToEvt<H>(this Act<H> act) =>
		Obs.Merge(
			act.Evt.WhenEvt
				.Select(e => e.IsOver(act.Hotspot))
				.DistinctUntilChanged()
				.Select(e => (IHotEvt<H>)new OverHotEvt<H>(e)),
			act.Evt.WhenEvt
				.Where(e => e.ToTrigger().IsSomeAndEqualTo(act.Trigger))
				.Select(e => e.GetMayMousePos())
				.WhenSome()
				.Select(act.Hotspot.Get)
				.WhenSome()
				.Take(1)
				.Select(e => (IHotEvt<H>)new TriggerHotEvt<H>(e))
		);



	private static bool IsOver<H>(this IEvtGen<Pt> evt, Hotspot<H> hotspot) =>
		evt.GetMayMousePos().IsSome(out var m) switch
		{
			false => false,
			true => hotspot.Get(m).IsSome()
		};

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
