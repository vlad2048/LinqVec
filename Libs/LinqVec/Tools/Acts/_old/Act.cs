/*
using System.Reactive.Linq;
using Geom;
using LinqVec.Tools.Acts.Events;
using LinqVec.Tools.Acts.Logic;
using LinqVec.Tools.Acts.Structs;
using LinqVec.Tools.Enums;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using PowBasics.CollectionsExt;
using PowRxVar;
using UILib;

namespace LinqVec.Tools.Acts;


public sealed record Act(
	string Name,
	Func<Pt, Option<object>> Hotspot,
	Trigger Trigger,
	Cursor? Cursor,
	Action<Option<object>>? OnHover,
	Action<object>? OnTrigger
)
{
	public override string ToString() => Name;

	public static IActExpr Make<H>(
		string name,
		Func<Pt, Option<H>> hotspot,
		Trigger trigger,
		Cursor? cursor,
		Action<Option<H>>? onHover,
		Action<H>? onTrigger
	) where H : notnull => new BaseActExpr(
		new(
			name,
			m => hotspot(m).Select(e => (object)e),
			trigger,
			cursor,
			mayObj => onHover?.Invoke(mayObj.Select(e => (H)e)),
			obj => onTrigger?.Invoke((H)obj)
		)
	);

	public static IActExpr Make(
		string name,
		Func<Pt, Option<Pt>> hotspot,
		Trigger trigger,
		Cursor? cursor,
		Action<Option<Pt>>? onHover,
		Action<Pt>? onTrigger
	) => Make<Pt>(
		name,
		hotspot,
		trigger,
		cursor,
		onHover,
		onTrigger
	);

	public static IActExpr Seq(params IActExpr[] acts) => new SeqActExpr(acts);
	public static IActExpr Amb(params IActExpr[] acts) => new AmbActExpr(acts);
	public static IActExpr Loop(IActExpr act) => new LoopActExpr(act);
}




public static class ActRunner
{
	public static IDisposable Run(this IActExpr act, Evt evt) =>
		act
			.XorHotspots()
			.Compute(evt)
			.RunActions(evt);


    private static IObservable<ISeqEvt> Compute(this IActExpr rootExpr, Evt evt)
    {
	    IObservable<ISeqEvt> Rec(IActExpr actExpr) =>
			actExpr switch
		    {
			    BaseActExpr { Act: var act } => act.ToSeq(evt),
			    SeqActExpr { Acts: var acts } => Seq(acts.SelectToArray(Rec)),
			    AmbActExpr { Acts: var acts } => Amb(acts.SelectToArray(Rec)),
			    LoopActExpr { Act: var act } => Loop(Rec(act)),
			    _ => throw new ArgumentException()
		    };

	    return Rec(rootExpr);
    }


    private static IDisposable RunActions(this IObservable<ISeqEvt> seq, Evt evt) =>
	    seq
		    //.ObserveOnUI()
		    .Subscribe(e =>
		    {
			    switch (e)
			    {
				    case HoverActionSeqEvt { Cursor: var cursor, Action: var action }:
					    action();
					    if (cursor.IsSome)
					    {
						    var cur = cursor.IfNone(() => throw new ArgumentException());
						    evt.SetCursor(cur);
					    }
					    break;
				    case TriggerActionSeqEvt { Action: var action }:
					    action();
					    break;
				    case StartSeqEvt:
					    break;
				    case FinishSeqEvt { Action: var action }:
					    action();
					    break;
			    }
		    });



	private static IObservable<ISeqEvt> Seq(IObservable<ISeqEvt>[] seqs) => seqs.Length switch
    {
	    0 => throw new ArgumentException(),
	    1 => seqs[0],
	    2 => Seq(seqs[0], seqs[1]),
		_ => Seq(seqs[0], Seq(seqs.SkipToArray(1)))
    };

    private static IObservable<ISeqEvt> Seq(IObservable<ISeqEvt> seq0, IObservable<ISeqEvt> seq1) => Seq(seq0, () => seq1);

	private static IObservable<ISeqEvt> Seq(IObservable<ISeqEvt> seq0, Func<IObservable<ISeqEvt>> seq1Fun) =>
	    seq0
		    .Select(e => e switch
		    {
			    HoverActionSeqEvt => Obs.Return(e),
			    TriggerActionSeqEvt => Obs.Return(e),
				StartSeqEvt => Obs.Return(e),
				FinishSeqEvt { Action: var action } =>
				    Obs.Return<ISeqEvt>(new TriggerActionSeqEvt(action)).Concat(
					    seq1Fun()
						    .Where(f => f is not StartSeqEvt)
				    ),
			    _ => throw new ArgumentException()
		    })
		    .Switch();
	private static IObservable<ISeqEvt> Loop(IObservable<ISeqEvt> seq) => Seq(seq, () => Loop(seq));


	private static IObservable<ISeqEvt> Amb(IObservable<ISeqEvt>[] seqs) =>
		Obs.Create<ISeqEvt>(obs =>
		{

			var d = new Disp();

			int? seqIdx = null;
			var objSync = new object();

			seqs
				.Select((Seq, Idx) =>
					Seq
						.Select(Evt => (Evt, Idx))
						.Where(t => seqIdx == null || seqIdx == t.Idx)
				)
				.Merge()
				.Synchronize(objSync)
				.Subscribe(t =>
				{
					if (t.Evt is StartSeqEvt)
						seqIdx = t.Idx;
					obs.OnNext(t.Evt);
				}).D(d);

			return d;
		});
}
*/