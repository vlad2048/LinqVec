<Query Kind="Program">
  <Reference>C:\dev\big\LinqVec\Libs\LinqVec\bin\Debug\net8.0-windows\LinqVec.dll</Reference>
  <Namespace>LinqVec.Tools.Events</Namespace>
  <Namespace>LinqVec.Tools.Acts.Enums</Namespace>
  <Namespace>PowRxVar</Namespace>
  <Namespace>LinqVec.Tools.Acts.Events</Namespace>
</Query>

#load "_common\rx"
global using Obs = System.Reactive.Linq.Observable;
using static RxTestMakers;


void Main()
{
	
}





static class GenEvt
{
	public static IObservable<IActEvt> ToActEvt(this IObservable<IEvt> evt, Gesture gestures, IRoDispBase d) =>
		Obs.Create<IActEvt>(obs =>
		{
			var obsD = new Disp();
			return obsD;
		});
}
