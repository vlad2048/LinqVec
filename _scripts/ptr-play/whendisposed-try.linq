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


public class TestDisp : IDisposable
{
	public int Idx { get; }
	public void Dispose() => Log("Dispose()");
	public TestDisp(int idx)
	{
		Idx = idx;
		//Log("Ctor()");
	}
	private void Log(string s) => L($"disp[{Idx}].{s}");
}
public static int idx = 0;
void OnStart() => idx = 0;
public static IDisposable MkDisp() => new TestDisp(idx++);


void Main()
{
	var rxVar = Sched.CreateHotObservable<string>([
		OnNext(10, "A"),
		OnNext(20, "B"),
		OnNext(30, "C"),
	]);
	var whenTrigger = Sched.CreateHotObservable<Unit>([
		OnNext(05, Unit.Default),
		OnNext(15, Unit.Default),
		OnNext(25, Unit.Default),
		OnNext(35, Unit.Default),
	]);
	
	var obs = Sched.CreateObserver<string>();
	
	rxVar.MyDupWhen(whenTrigger).Subscribe(obs);
	
	Sched.Start();
	
	obs.LogMessages();
}



public static class MyRxExt
{

	public static IObservable<T> MyDupWhen<T>(this IObservable<T> source, IObservable<Unit> whenDup) =>
		Obs.Merge(
			whenDup.WithLatestFrom(source, (_, v) => v),
			source
		);
		
	/*public static IObservable<T> MyDupWhen<T>(this IObservable<T> source, IObservable<Unit> whenDup) =>
		Obs.Merge([
			source.ToUnit(),
			whenDup,
		])
		.WithLatestFrom(source, (_, v) => v);*/
}




public static void L(string s) => Console.WriteLine(s);



























