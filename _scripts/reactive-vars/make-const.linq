<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\LibsBase\ReactiveVars\bin\Debug\net8.0\ReactiveVars.dll</Reference>
  <Namespace>ReactiveVars</Namespace>
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
	var v = MakeConst(7);
	
	v.V.Dump();
	v.V.Dump();
	"done".Dump();
}

public static IRoVar<T> MakeConst<T>(T val) => Obs.Return(val).ToVar();
