<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\LibsBase\ReactiveVars\bin\Debug\net8.0\ReactiveVars.dll</Reference>
  <Namespace>ReactiveVars</Namespace>
</Query>

using Obs = System.Reactive.Linq.Observable;
using Disp = System.Reactive.Disposables.CompositeDisposable;


void Main()
{
	var v = MakeConst(7);
	
	v.V.Dump();
	v.V.Dump();
	"done".Dump();
}

public static IRoVar<T> MakeConst<T>(T val) => Obs.Return(val).ToVar();



public static Disp D = null!;
//public static TestScheduler Sched = null!;
void OnStart()
{
	D?.Dispose();
	D = new Disp();
	//Sched = new TestScheduler();
	Util.HtmlHead.AddStyles(
	"""
		body {
			font-family: Consolas;
		}
	""");
}