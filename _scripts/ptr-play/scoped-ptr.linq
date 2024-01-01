<Query Kind="Program">
  <Namespace>PowBasics.CollectionsExt</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>LINQPad.Controls</Namespace>
</Query>

#load "_common\rx"

// System
using System.Threading.Tasks;

// Reactive
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using Obs = System.Reactive.Linq.Observable;
using Disp = System.Reactive.Disposables.CompositeDisposable;

// LanguageExt
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using Unit = LanguageExt.Unit;

// ReactiveVars
using ReactiveVars;
using static ReactiveVars.DispMaker;

// LINQPad
using static RxTestMakers;



void Main()
{
	var (source, action) = Obs.Interval(TimeSpan.FromSeconds(0.2)).Select(e => (int)e).Take(5)
		.TerminateWithAction();
	
	source.Materialize().Select(e => $"{e}").Subscribe(e => e.Dump());
	
	Util.HorizontalRun(true,
		new Button("OnCompleted", _ => action(true)),
		new Button("OnError", _ => action(false))
	).Dump();
}


public static class Utils
{
	public static (IObservable<T>, Action<bool>) TerminateWithAction<T>(this IObservable<T> source)
	{
		var subj = new AsyncSubject<bool>();
		void Finish(bool commit)
		{
			if (commit) subj.OnNext(true); else subj.OnNext(false);
			subj.OnCompleted();
			subj.Dispose();
		}
		return
		(
			Obs.Using(
				() => MkD(),
				d => Obs.Create<T>(obs =>
				{
					source.Subscribe(obs.OnNext).D(d);
					subj
						.Subscribe(commit =>
						{
							if (commit)
								obs.OnCompleted();
							else
								obs.OnError(new ArgumentException("User cancelled"));
						}).D(d);
					return d;
				})
			),
			Finish
		);
	}
}




public static (IObservable<int>, Action<bool>) Mk()
{
	var subj = new AsyncSubject<bool>();
	void Finish(bool commit)
	{
		if (subj.IsDisposed) throw new ArgumentException();
		$"before: {subj.IsDisposed}".Dump();
		if (commit)
			subj.OnNext(true);
		else
			subj.OnNext(false);
		$"after: {subj.IsDisposed}".Dump();
		try
		{
			subj.OnCompleted();
		}
		catch (Exception)
		{
			$"finally: {subj.IsDisposed}".Dump();
		}
		subj.Dispose();
	}
	return
		(
			Obs.Using(
				() => MkD(),
				d => Obs.Create<int>(obs =>
				{
					Obs.Interval(TimeSpan.FromSeconds(0.2)).Select(e => (int)e)
						.Take(5)
						.Subscribe(e => obs.OnNext(e)).D(d);
					subj
						.Subscribe(commit =>
						{
							if (commit)
								obs.OnCompleted();
							else
								obs.OnError(new ArgumentException("User cancelled"));
						}).D(d);
					return d;
				})
			),
			Finish
		);
}







/*
public sealed class ModUserCancelledException : Exception;





sealed class Ptr<TDoc> : IDisposable
{
	private readonly Disp d;
	public void Dispose() => d.Dispose();

	private readonly IRwVar<Option<IScopedPtr<TDoc>>> kid;

	public IRwVar<TDoc> V { get; }
	
	public Ptr(TDoc init, Disp d)
	{
		this.d = d;
		V = init.Make(d);
		kid = Option<IScopedPtr<TDoc>>.None.Make(d);
	}
	
	
}

interface IScopedPtr<TDoc>
{
	TDoc Add(TDoc doc);
	TDoc Del(TDoc doc);
}
sealed class ScopedPtr<TDoc, TSub> : IDisposable, IScopedPtr<TDoc>
{
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly Func<TDoc, TSub, TDoc> del;
	private readonly Func<TDoc, TSub, TDoc> add;

	public IRwVar<TSub> V { get; }
	public TDoc Del(TDoc doc) => del(doc, V.V);
	public TDoc Add(TDoc doc) => add(doc, V.V);

	public ScopedPtr(
		TSub init,
		Func<TDoc, TSub, TDoc> del,
		Func<TDoc, TSub, TDoc> add
	)
	{
		V = init.Make(d);
		this.add = add;
		this.del = del;
	}
}


record Curve(Guid Id, int[] Pts)
{
	public override string ToString() => $"[{Pts.JoinText(",")}]";
	public static Curve Empty() => new(Guid.NewGuid(), []);
}
record Doc(Curve[] Curves)// : IDoc
{
	public override string ToString() => Curves.JoinText("; ");
	public static readonly Doc Empty = new([]);
	public static Doc Busy() => new([Curve.Empty()]);
}

//record DocSubset
*/




















