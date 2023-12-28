<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\LibsBase\ReactiveVars\bin\Debug\net8.0\ReactiveVars.dll</Reference>
  <Namespace>ReactiveVars</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

#load "_common\rx"
// System
using System.Threading.Tasks;

// Reactive
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using Obs = System.Reactive.Linq.Observable;
using Disp = System.Reactive.Disposables.CompositeDisposable;

// LanguageExt
using Unit = LanguageExt.Unit;

// ReactiveVars
using ReactiveVars;
using static ReactiveVars.DispMaker;

// LINQPad
using static RxTestMakers;


void Main()
{
	var v = MakeBound(0, D);
	var obs = Sched.CreateObserver<int>();
	var obsOuter = Sched.CreateObserver<int>();
	var obsInner = Sched.CreateObserver<int>();
	v.Subscribe(obs);
	v.WhenOuter.Subscribe(obsOuter);
	v.WhenInner.Subscribe(obsInner);
	
	
	Sched.AdvanceTo(1.Sec());
	
	v.V = 1;
	
	Sched.AdvanceTo(2.Sec());
	
	v.SetInner(2);
	
	Sched.AdvanceTo(3.Sec());
	
	v.V = 3;


	obs.LogMessages("Var");
	obsOuter.LogMessages("Outer");
	obsInner.LogMessages("Inner");
}

public static IBoundVar<T> MakeBound<T>(T init, Disp d) => new BoundVar<T>(init).D(d);

public interface IBoundVar<T> : IRwVar<T>
{
	IObservable<T> WhenOuter { get; }
	IObservable<T> WhenInner { get; }
	void SetInner(T v);
}

public sealed class BoundVar<T> : IBoundVar<T>, IDisposable
{
	private enum UpdateType { Inner, Outer };
	private sealed record Update(UpdateType Type, T Val);
	
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly BehaviorSubject<T> Subj;
	private readonly ISubject<Update> whenUpdate;
	private IObservable<Update> WhenUpdate { get; }
	
	// IRoVar<T>
	// =========
	public IDisposable Subscribe(IObserver<T> observer) => Subj.Subscribe(observer);

	// IRwVar<T>
	// =========
	public T V {
		get => Subj.Value;
		set => SetOuter(value);
	}
	public bool IsDisposed => Subj.IsDisposed;
	
	// IBoundVar<T>
	// ============
	public IObservable<T> WhenOuter => WhenUpdate.Where(e => e.Type == UpdateType.Outer).Select(e => e.Val);
	public IObservable<T> WhenInner => WhenUpdate.Where(e => e.Type == UpdateType.Inner).Select(e => e.Val);
	public void SetInner(T v) => whenUpdate.OnNext(new Update(UpdateType.Inner, v));
	private void SetOuter(T v) => whenUpdate.OnNext(new Update(UpdateType.Outer, v));

	public BoundVar(T init)
	{
		Subj = new BehaviorSubject<T>(init).D(d);
		whenUpdate = new Subject<Update>().D(d);
		WhenUpdate = whenUpdate.AsObservable();
		WhenUpdate.Subscribe(e => Subj.OnNext(e.Val)).D(d);
	}
}
